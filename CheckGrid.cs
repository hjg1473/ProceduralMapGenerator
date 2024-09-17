using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CheckGrids
{
    private PGGbeforeRefactor pggBeforeRefactor;
    // 생성자에서 MainClass 인스턴스를 전달받음
    public CheckGrids(PGGbeforeRefactor pggBeforeRefactor)
    {
        this.pggBeforeRefactor = pggBeforeRefactor;
    }
    #region Check Grid

    // 공통적인 그리드 범위 체크 함수
    bool IsValidGridIndex<T>(T[,] grid, int row, int col)
    {
        return row >= 0 && row < grid.GetLength(0) && col >= 0 && col < grid.GetLength(1);
    }

    // 그리드에서 값을 가져오는 공통 함수
    string GetGridValue<T>(T[,] grid, int row, int col)
    {
        if (!IsValidGridIndex(grid, row, col))
        {
            Debug.LogError("Invalid row or column index.");
            return "0";
        }

        return grid[row, col].ToString();
    }

    // 그리드를 체크하는 함수 (grid_to_Calculate)
    public string CheckGrid(int row, int col)
    {
        string cellValue = GetGridValue(pggBeforeRefactor.grid_to_Calculate, row, col);

        if (cellValue == "X")
        {
            bool isHorizontal = DetermineIfHorizontal(row, col); // 가정된 함수
            return isHorizontal ? "X_Horizontal" : "X_Vertical";
        }

        return cellValue;
    }

    // 그리드를 체크하는 함수 (grid_UserAble)
    public string Check_grid_UserAble(int row, int col)
    {
        return GetGridValue(pggBeforeRefactor.grid_UserAble, row, col);
    }

    // 그리드를 체크하는 함수 
    public string Check_grid_toilet(int row, int col)
    {
        return GetGridValue(pggBeforeRefactor.grid_Toilet, row, col);
    }

    #endregion


    /* 둘다 있으면 어떻게 할 것인가. */
    bool DetermineIfHorizontal(int row, int col)
    {
        // 그리드 범위 체크
        if (row < 0 || row >= pggBeforeRefactor.grid_to_Calculate.GetLength(0) || col < 0 || col >= pggBeforeRefactor.grid_to_Calculate.GetLength(1))
        {
            Debug.LogError("Invalid row or column index.");
            return false; // 범위를 벗어나면 기본적으로 세로로 취급
        }

        // 현재 위치의 좌우를 검사하여 가로로 판단
        string left = CheckGrid(row, col - 1);
        string right = CheckGrid(row, col + 1);

        // 좌우에 "X"가 있으면 가로(ㅡ)로 판단
        if ((left == "A" && right == "A") || (left == "B" && right == "B") || 
        (left == "C" && right == "C") || (left == "D" && right == "D"))
        {
            return false; // 가로(ㅡ)로 판단
        }

        

        // 상하에 "X"가 있으면 세로(ㅣ)로 판단
        string above = CheckGrid(row - 1, col);
        string below = CheckGrid(row + 1, col);
        
        if ((above == "A" && below == "A") || (above == "B" && below == "B") || 
        (above == "C" && below == "C") || (above == "D" && below == "D"))
        {
            return true; // 세로(ㅣ)로 판단
        }

        // 추가적인 판단 로직을 필요에 따라 추가할 수 있음
        // 예시: 주변에 아무것도 없으면 기본적으로 세로로 판단
        return false;
    }
}