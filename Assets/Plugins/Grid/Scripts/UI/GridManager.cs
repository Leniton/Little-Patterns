using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GridSystem.UI
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class GridManager : MonoBehaviour, IGrid
    {
        [SerializeField] int width, height;

        public int Width
        {
            get { return width; }
            private set { width = value; }
        }

        public int Height
        {
            get { return height; }
            private set { height = value; }
        }

        [SerializeField] private Tile gridTile;
        private GridLayoutGroup grid;
        public List<Tile> tiles = new();
        public ITile hoveredTile { get; set; }
        public bool currentlySelecting { get; set; }

        public Action<ITile> onClick { get; set; }
        public Action<ITile> onEnter { get; set; }
        public Action<ITile> onExit { get; set; }

        private void Awake()
        {
            grid = GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            SetUpGrid();
        }

        public void SetUpGrid()
        {
            grid.constraintCount = height;
            tiles.Clear();
            for (int i = 0; i < width * height; i++)
            {
                Tile tile = Instantiate(gridTile, transform);
                tile.name = $"Tile [{i % width},{i / width}]";
                tiles.Add(tile);
                tiles[i].onClick = onClick;
                tiles[i].onEnter = onEnter;
                tiles[i].onExit = onExit;
            }
        }

        public ITile GetTileAtIndex(int index) => tiles[index];

        public int GetTileIndex(ITile tile) => tiles.IndexOf(tile as Tile);
    }
}