using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rules : MonoBehaviour
{
    public GridGenerator GridGeneratorObject;
    
    public int[,] ProcessGeneration()
    {
        // Assumption: A cell can only be either 0 or 1.
        int[,] Grid = GridGeneratorObject.GetGrid();
        int Rows = GridGeneratorObject.GetRows();
        int Columns = GridGeneratorObject.GetColumns();
        int[,] NextGrid = new int[Columns, Rows];

        for (int x = 1; x < Rows-1; x++)
        {
            for (int y = 1; y < Columns-1; y++)
            {
                // Find number of alive neighbours
                int neighboursCount = 0;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        neighboursCount += Grid[x+i,y+j];
                    }
                }

                // Remove the cell itself from the neighbours count
                neighboursCount -= Grid[x,y];

                /// Rules
                // Lonely cell -> Will die
                if ((Grid[x,y] == 1) && (neighboursCount < 2))
                {
                    NextGrid[x,y] = 0;
                }

                // Overpopulated -> Will die
                else if ((Grid[x,y] == 1) && (neighboursCount >3))
                {
                    NextGrid[x, y] = 0;
                }

                // Good conditions -> New cell is born
                else if ((Grid[x,y] == 0) && (neighboursCount == 3))
                {
                    NextGrid[x, y] = 1;
                }

                // None of the above -> No change
                else
                {
                    NextGrid[x, y] = Grid[x, y];
                }
            }
        }

        return NextGrid;
    }
}
