using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GridUtils
{

    private PGGbeforeRefactor pggBeforeRefactor;

    // 생성자에서 MainClass 인스턴스를 전달받음
    public GridUtils(PGGbeforeRefactor pggBeforeRefactor)
    {
        this.pggBeforeRefactor = pggBeforeRefactor;
    }

    // 그리드 초기화
    public void InitializeGrid()
    {
        for (int i = 0; i < pggBeforeRefactor.prevHeight; i++)
        {
            for (int j = 0; j < pggBeforeRefactor.prevWidth; j++)
            {
                pggBeforeRefactor.grid_to_Calculate[i, j] = "0";
                pggBeforeRefactor.grid_UserAble[i, j] = "0";
                pggBeforeRefactor.visited[i, j] = false;
            }
        }
    }

    public void ClearAll()
    {
        for (int row = 0; row < pggBeforeRefactor.prevHeight; row++)
        {
            for (int col = 0; col < pggBeforeRefactor.prevWidth; col++)
            {
                if (pggBeforeRefactor.grid[row, col] != null)
                {
                    // Debug.Log($"Destroying cell at position ({row}, {col})");
                    UnityEngine.Object.Destroy(pggBeforeRefactor.grid[row, col]);
                    pggBeforeRefactor.grid[row, col] = null;
                    UnityEngine.Object.Destroy(pggBeforeRefactor.grid_innerWall[row, col]);
                    pggBeforeRefactor.grid_innerWall[row, col] = null;

                    pggBeforeRefactor.grid_to_Calculate[row, col] = null;
                    pggBeforeRefactor.grid_UserAble[row, col] = null;

                }
            }
        }
        // Debug.Log("All cells cleared.");
    }

    public void CopyArrayAndModifyXto0(string[,] sourceArray, string[,] destinationArray, int height, int width)
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                // 배열 복사
                destinationArray[i, j] = sourceArray[i, j];

                // "X" 값을 "0"으로 변환
                if (destinationArray[i, j] == "X")
                {
                    destinationArray[i, j] = "0";
                }
            }
        }
    }

}