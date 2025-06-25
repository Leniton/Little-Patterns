using System;
using System.Collections;
using System.Collections.Generic;
using GridSystem;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private IGrid grid;
    
    // Start is called before the first frame update
    void Start()
    {
        grid = FindObjectOfType<GridSystem.UI.GridManager>();
        if (grid == null) grid = FindObjectOfType<GridSystem.VisualElements.GridManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) grid.ChooseTileInRange(Coordinate.Zero, Area.Point, Area.Point, null);
    }
}
