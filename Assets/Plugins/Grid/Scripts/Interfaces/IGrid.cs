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

        public Coordinate GetTileCoordinates(ITile tile) =>
            GetTileCoordinates(GetTileIndex(tile));
        
        public Coordinate GetTileCoordinates(int id)
        {
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

        public void PickArea(AreaPickData areaPick, Action<AreaPickEvent> onPick)
        {
            currentlySelecting = true;

            List<Coordinate> coordinates;
            if (areaPick.originArea.HasValue)
            {
                Area area = areaPick.originArea.Value;
                Coordinate coordinate = areaPick.originCoordinate ?? Coordinate.Zero;
                coordinates = area.GetCoordinates(coordinate, Width, Height);
            }
            else
            {
                coordinates = new();
                for (int i = 0; i < gridSize; i++)
                    coordinates.Add(GetTileCoordinates(i));
            }

            for (int i = 0; i < coordinates.Count; i++)
            {
                Coordinate currentCoordinate = coordinates[i];
                if (!GetTileAt(currentCoordinate, out var tile)) continue;
                tile.onEnter += OnEnterTile;
                tile.onExit += OnExitTile;
                tile.onClick += OnClickTile;
                tile.AddState(ITile.State.Selectable);
                /*tile.RemoveState(IsInFilter(tile.pieceID, filter) ?
                    ITile.State.Valid : ITile.State.Invalid);*/
            }
            
            void OnEnterTile(ITile tile)
            {
                var currentCoordinate = GetTileCoordinates(tile);
                SelectArea(currentCoordinate, areaPick.pickArea, areaPick.areaFilter);
            }

            void OnExitTile(ITile tile)
            {
                var currentCoordinate = GetTileCoordinates(tile);
                UnSelectArea(currentCoordinate, areaPick.pickArea, areaPick.areaFilter);
            }

            void OnClickTile(ITile tile)
            {
                var currentCoordinate = GetTileCoordinates(tile);
                UnSelectArea(currentCoordinate, areaPick.pickArea, areaPick.areaFilter);
                AreaPickEvent evt = new(this, areaPick, currentCoordinate);
                List<Coordinate> areaCoordinates =
                    areaPick.pickArea.GetCoordinates(currentCoordinate, Width, Height);
                for (int j = 0; j < areaCoordinates.Count; j++)
                {
                    if (!GetTileAt(areaCoordinates[j], out var currentTile) ||
                        !IsInFilter(currentTile.pieceID, areaPick.areaFilter))
                        continue;
                    evt.tiles.Add(currentTile);
                }

                for (int j = 0; j < coordinates.Count; j++)
                {
                    if (!GetTileAt(coordinates[j], out var areaTile)) continue;
                    areaTile.RemoveState(ITile.State.Selectable);
                    areaTile.onEnter -= OnEnterTile;
                    areaTile.onExit -= OnExitTile;
                    areaTile.onClick -= OnClickTile;
                }

                onPick?.Invoke(evt);
            }
            if (hoveredTile == null || !coordinates.Contains(GetTileCoordinates(hoveredTile))) return;

            SelectArea(GetTileCoordinates(hoveredTile), areaPick.pickArea, areaPick.areaFilter);
        }
    }

    public struct AreaPickData
    {
        public Coordinate? originCoordinate;
        public Area? originArea;
        public Area pickArea;
        public int areaFilter;

        public AreaPickData(Area pickArea, int areaFilter = -1)
        {
            this.pickArea = pickArea;
            this.areaFilter = areaFilter;
            originArea = null;
            originCoordinate = null;
        }
    }

    public struct AreaPickEvent
    {
        public AreaPickData pickData;
        public IGrid grid;
        
        public List<ITile> tiles;
        public Coordinate pickOrigin;
        public Area area => pickData.pickArea;

        public AreaPickEvent(IGrid currentGrid, AreaPickData data, Coordinate origin)
        {
            grid = currentGrid;
            pickData = data;
            tiles = new();
            pickOrigin = origin;
        }
    }
}