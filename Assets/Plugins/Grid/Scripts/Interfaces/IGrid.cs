using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridSystem
{
    public interface IGrid
    {
        public static IGrid Instance;

        public int Width { get; }
        public int Height { get; }

        public int gridSize => Width * Height;

        public ITile hoveredTile { get; set; }
        public bool currentlySelecting { get; set; }

        public Action<ITile> onClick { get; set; }
        public Action<ITile> onEnter { get; set; }
        public Action<ITile> onExit { get; set; }

        public void SetUpGrid();

        public int GetTileIndex(ITile tile);
        public ITile GetTileAtIndex(int index);

        public Coordinate GetTileCoordinates(ITile tile)
        {
            int id = GetTileIndex(tile);
            int x = id % Width;
            int y = id / Width;

            //print($"x:{x} | y:{y}");
            return new Coordinate(x, y);
        }

        public bool GetTileAt(Coordinate coordinates, out ITile tile)
        {
            tile = (coordinates.x < 0 || coordinates.y < 0 || coordinates.x >= Width || coordinates.y >= Height) ? 
                null : GetTileAtIndex((Width * coordinates.y) + coordinates.x);
            return tile != null;
        }

        public void WarpToSpot(IPiece piece, Coordinate coordinates)
        {
            if (!GetTileAt(coordinates, out var newTile)) return;
            if (GetTileAt(coordinates, out var currentTile)) currentTile.RemovePiece(piece);
            piece.coordinate = coordinates;
            newTile.PlacePiece(piece);
        }

        public void ChooseTileInRange(Coordinate origin, Area area, Area selectArea,
            Action<Coordinate, Coordinate, Area> onSelectTile,
            int filter = -1, int selectFilter = -1)
        {
            currentlySelecting = true;

            List<Coordinate> coordinates = area.GetCoordinates(origin, Width, Height);
            for (int i = 0; i < coordinates.Count; i++)
            {
                Coordinate currentCoordinate = coordinates[i];
                //Debug.Log($"{currentCoordinate.x} | {currentCoordinate.y}");
                if (GetTileAt(currentCoordinate, out var tile))
                {
                    tile.onEnter += (value) => SelectArea(currentCoordinate, selectArea, selectFilter);
                    tile.onExit += (value) => UnSelectArea(currentCoordinate, selectArea, selectFilter);
                    tile.onClick += (value) =>
                        ClickArea(onSelectTile, origin, currentCoordinate, selectArea, selectFilter);
                    tile.RemoveState(IsInFilter(tile.pieceID, filter) ? 
                        ITile.State.Valid : ITile.State.Invalid);
                }
            }

            if (hoveredTile == null || !coordinates.Contains(GetTileCoordinates(hoveredTile))) return;

            SelectArea(GetTileCoordinates(hoveredTile), selectArea, selectFilter);
        }

        public void SelectArea(Coordinate origin, Area area, int filter = -1)
        {
            if (!currentlySelecting) return;

            List<Coordinate> coordinates = area.GetCoordinates(origin, Width, Height);
            for (int i = 0; i < coordinates.Count; i++)
            {
                if (GetTileAt(coordinates[i], out var currentTile))
                {
                    currentTile.AddState(IsInFilter(currentTile.pieceID, filter) ? 
                        ITile.State.Valid : ITile.State.Invalid);
                }
            }
        }

        public void UnSelectArea(Coordinate origin, Area area, int filter = -1)
        {
            //return;
            if (!currentlySelecting) return;

            List<Coordinate> coordinates = area.GetCoordinates(origin, Width, Height);
            for (int i = 0; i < coordinates.Count; i++)
            {
                if (GetTileAt(coordinates[i], out var currentTile))
                {
                    currentTile.RemoveState(IsInFilter(currentTile.pieceID, filter) ? 
                        ITile.State.Valid : ITile.State.Invalid);
                }
            }
        }

        public void ClickArea(Action<Coordinate, Coordinate, Area> clickAction, Coordinate origin, Coordinate point,
            Area area, int filter = -1)
        {
            //return;
            if (!currentlySelecting) return;

            List<Coordinate> coordinates = area.GetCoordinates(point, Width, Height);
            for (int i = 0; i < coordinates.Count; i++)
            {
                if (GetTileAt(coordinates[i], out var currentTile) && 
                    IsInFilter(currentTile.pieceID, filter))
                {
                    clickAction?.Invoke(origin, point, area);
                    return;
                }
            }
        }

        public void StopSelecting()
        {
            for (int i = 0; i < gridSize; i++)
            {
                ITile tile = GetTileAtIndex(i);
                tile.SetState(ITile.State.Generic);
                tile.onClick = OnClick;
                tile.onEnter = OnEnter;
                tile.onExit = OnExit;
            }

            currentlySelecting = false;
        }

        public void OnClick(ITile tile) => onClick?.Invoke(tile);

        public void OnEnter(ITile tile)
        {
            hoveredTile = tile;
            onEnter?.Invoke(tile);
        }

        public void OnExit(ITile tile)
        {
            hoveredTile = null;
            onExit?.Invoke(tile);
        }

        public static bool IsInFilter(int value, int filter) =>
            filter <= 0 || NumberUtil.ContainsAnyBits(value, filter);
    }
}