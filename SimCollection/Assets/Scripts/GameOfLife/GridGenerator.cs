using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public int[,] Grid;
    public Sprite sprite;
    public int Columns = 100;
    public int Rows = 100;
    public float TileSize = 1.0f;

    public bool IsRunning = false;
    public Rules RulesManager;

    private GameObject[,] CreatedTiles;

    float elapsed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= 5f)
        {
            if (IsRunning)
            {
                Grid = RulesManager.ProcessGeneration();
                UpdateGridTiles();
            }
            else
            {
                return;
            }
        }
    }

    public void CreateGrid()
    {
        Grid = new int[Columns, Rows];
        ClearGridTiles();
        CreatedTiles = new GameObject[Columns, Rows];

        for (int i = 0; i < Columns; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                Grid[i, j] = Random.Range(0,2);
                CreateGridTiles(i, j, Grid[i, j]);
            }
        }
    }

    private void CreateGridTiles(int x, int y, float value)
    {
        GameObject currentObject = new GameObject("x: " + x + " y: " + y);
        currentObject.transform.position = new Vector3(x - TileSize/2, y - TileSize/2);
        var spriteRenderer = currentObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = new Color(value, value, value);

        CreatedTiles[x, y] = currentObject;
    }

    private void UpdateGridTiles()
    {
        for (int i = 0; i < Columns; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                GameObject currentTile = CreatedTiles[i, j];
                int alive = Grid[i, j];
                currentTile.GetComponent<SpriteRenderer>().color = new Color(alive, alive, alive);

            }
        }
    }

    private void ClearGridTiles()
    {
        if (CreatedTiles != null)
        {
            foreach (GameObject tile in CreatedTiles)
            {
                Destroy(tile);
            }
        }
    }

    public int[,] GetGrid()
    {
        return Grid;
    }

    public int GetColumns()
    {
        return Columns;
    }

    public int GetRows()
    {
        return Rows;
    }

    public void ToggleRunning()
    {
        IsRunning = !IsRunning;
    }
}

