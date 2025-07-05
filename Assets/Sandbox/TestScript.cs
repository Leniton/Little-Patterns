using System;
using System.Collections;
using System.Collections.Generic;
using GridSystem;
using GridSystem.UI;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private IGrid grid;
    [SerializeField] private Piece piece;
    [SerializeField] private Transform canvas;
    
    // Start is called before the first frame update
    void Start()
    {
        grid = FindObjectOfType<GridManager>();
        if (grid == null) grid = FindObjectOfType<GridSystem.VisualElements.GridManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            grid.PickArea(new()
            {
                pickArea = Area.Square(1),
                originArea = Area.Square(1),
                originCoordinate = new(2,2),
            }, null);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Piece newPiece = Instantiate(piece,canvas);
            grid.WarpToSpot(newPiece, new(2, 2));
        }
    }
}
