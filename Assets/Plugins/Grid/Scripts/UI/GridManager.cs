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
        public List<ITile> tiles { get; set; } = new();
        public ITile hoveredTile { get; set; }
        public bool currentlySelecting { get; set; }

        public Action<ITile> onClick { get; set; }
        public Action<ITile> onEnter { get; set; }
        public Action<ITile> onExit { get; set; }

        private void Awake()
        {
            if (IGrid.Instance != this)
            {
                if (IGrid.Instance == null) IGrid.Instance = this;
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }

            grid = GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            SetUpGrid();
        }

        public void SetUpGrid()
        {
            IGrid _grid = this;
            grid.constraintCount = height;
            tiles.Clear();
            for (int i = 0; i < width * height; i++)
            {
                Tile tile = Instantiate(gridTile, transform);
                tile.name = $"Tile [{i % width},{i / width}]";
                tiles.Add(tile);
                tiles[i].onClick = _grid.OnClick;
                tiles[i].onEnter = _grid.OnEnter;
                tiles[i].onExit = _grid.OnExit;
            }
        }
    }
}