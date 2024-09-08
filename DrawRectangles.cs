using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DrawRectangles
{
    private PGGbeforeRefactor pggBeforeRefactor;

    // 생성자에서 MainClass 인스턴스를 전달받음
    public DrawRectangles(PGGbeforeRefactor pggBeforeRefactor)
    {
        this.pggBeforeRefactor = pggBeforeRefactor;
    }

    #region Draw Rectangle
    // 사각형 그리는 함수
    public void DrawRectangle(Vector2Int topLeft, Vector2Int size, char label)
    {
        int x = topLeft.x;
        int y = topLeft.y;
        int rectangleWidth = size.x;
        int rectangleHeight = size.y;

        // 방향에 따른 변화량 설정
        (int xMultiplier, int yMultiplier) = GetDirectionMultipliers(label);

        for (int i = 0; i < rectangleHeight; i++)
        {
            for (int j = 0; j < rectangleWidth; j++)
            {
                int currentX = x + j * xMultiplier;
                int currentY = y + i * yMultiplier;

                // 그리기 범위가 그리드 바깥으로 나가지 않도록 체크
                if (IsWithinGrid(currentX, currentY))
                {
                    UpdateGridValue(currentX, currentY, label);
                }
            }
        }
    }

    // 방향에 따른 좌표 변화량을 반환하는 함수
    (int xMultiplier, int yMultiplier) GetDirectionMultipliers(char label)
    {
        return label switch
        {
            'A' => (1, 1),   // TopLeft
            'B' => (-1, 1),  // TopRight
            'C' => (-1, -1), // BottomLeft
            'D' => (1, -1),  // BottomRight
            _ => throw new ArgumentException($"Invalid label: {label}") // 유효하지 않은 label 처리
        };
    }


    // 그리드 범위 체크 함수
    bool IsWithinGrid(int x, int y)
    {
        return x >= 0 && x < pggBeforeRefactor.grid_to_Calculate.GetLength(0) && y >= 0 && y < pggBeforeRefactor.grid_to_Calculate.GetLength(1);
    }

    // 그리드 값 업데이트 함수
    void UpdateGridValue(int x, int y, char label)
    {
        if (pggBeforeRefactor.grid_to_Calculate[x, y] == "0")
        {
            pggBeforeRefactor.grid_to_Calculate[x, y] = label.ToString();
        }
        else
        {
            pggBeforeRefactor.grid_to_Calculate[x, y] = "P";  // 중복된 부분은 'P'로 표시
        }
    }


    public void DrawRectangleBlank()
    {
        // 변환할 위치를 기록할 2D 배열
        bool[,] toBeZeroed = new bool[pggBeforeRefactor.prevHeight, pggBeforeRefactor.prevWidth];

        // 대상 문자의 배열
        char[] targetChars = { 'A', 'B', 'C', 'D' };

        // 각 문자에 대해 빈 공간을 뚫을 위치를 기록
        foreach (char targetChar in targetChars)
        {
            MarkPositionsToZero(targetChar, toBeZeroed);
        }

        // 두 번째 패스: 실제로 변환을 적용
        ApplyZeroedPositions(toBeZeroed);
    }

    // targetChar에 맞는 빈 공간을 기록하는 함수
    void MarkPositionsToZero(char targetChar, bool[,] toBeZeroed)
    {
        for (int i = 0; i < pggBeforeRefactor.prevHeight; i++)
        {
            for (int j = 0; j < pggBeforeRefactor.prevWidth; j++)
            {
                if (pggBeforeRefactor.grid_to_Calculate[i, j] == targetChar.ToString() && IsSurroundedBySameChar(i, j, targetChar))
                {
                    toBeZeroed[i, j] = true;
                }
            }
        }
    }

    // 상하좌우가 모두 targetChar인지 확인하는 함수
    bool IsSurroundedBySameChar(int i, int j, char targetChar)
    {
        string targetStr = targetChar.ToString();

        string above = i > 0 ? pggBeforeRefactor.grid_to_Calculate[i - 1, j] : "0";
        string below = i < pggBeforeRefactor.prevHeight - 1 ? pggBeforeRefactor.grid_to_Calculate[i + 1, j] : "0";
        string left = j > 0 ? pggBeforeRefactor.grid_to_Calculate[i, j - 1] : "0";
        string right = j < pggBeforeRefactor.prevWidth - 1 ? pggBeforeRefactor.grid_to_Calculate[i, j + 1] : "0";

        return above == targetStr && below == targetStr && left == targetStr && right == targetStr;
    }

    // toBeZeroed 배열에 표시된 위치를 실제로 "0"으로 변환하는 함수
    void ApplyZeroedPositions(bool[,] toBeZeroed)
    {
        for (int i = 0; i < pggBeforeRefactor.prevHeight; i++)
        {
            for (int j = 0; j < pggBeforeRefactor.prevWidth; j++)
            {
                if (toBeZeroed[i, j])
                {
                    pggBeforeRefactor.grid_to_Calculate[i, j] = "0";
                }
            }
        }
    }

    #endregion
}