using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DecisionRooms
{
    private PGGbeforeRefactor pggBeforeRefactor;
    // 생성자에서 MainClass 인스턴스를 전달받음
    public DecisionRooms(PGGbeforeRefactor pggBeforeRefactor)
    {
        this.pggBeforeRefactor = pggBeforeRefactor;
    }
    
    public static void FindLargestSquare(string[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        int[,] dp = new int[rows, cols]; // DP 배열로 최대 정사각형 크기 추적
        int maxSize = 0;
        (int, int) bottomRight = (0, 0);

        // DP를 이용해 각 위치에서 만들 수 있는 최대 정사각형 크기를 계산
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (matrix[i, j] == "M")
                {
                    if (i == 0 || j == 0)  // 첫 번째 행이나 열은 그대로 사용
                    {
                        dp[i, j] = 1;
                    }
                    else
                    {
                        dp[i, j] = Math.Min(Math.Min(dp[i - 1, j], dp[i, j - 1]), dp[i - 1, j - 1]) + 1;
                    }

                    // 최대 정사각형 크기와 위치 갱신
                    if (dp[i, j] > maxSize)
                    {
                        maxSize = dp[i, j];
                        bottomRight = (i, j);
                    }
                }
            }
        }

        // 가장 큰 정사각형을 "K"로 바꾸기
        for (int i = bottomRight.Item1; i > bottomRight.Item1 - maxSize; i--)
        {
            for (int j = bottomRight.Item2; j > bottomRight.Item2 - maxSize; j--)
            {
                matrix[i, j] = "K";
            }
        }
    }

    // 배열에서 가장 적은 개수의 문자열을 'T'로 변경하는 함수
    public static void ReplaceMinStringWithT(string[,] array, string[] stringsToCheck)
    {
        Dictionary<string, int> countMap = CountStrings(array, stringsToCheck);
        string minString = FindMinString(countMap);
        ReplaceInternalWithT(array, minString);
    }

    // 배열에서 특정 문자열의 개수를 세는 함수
    static Dictionary<string, int> CountStrings(string[,] array, string[] stringsToCheck)
    {
        Dictionary<string, int> countMap = new Dictionary<string, int>();
        foreach (string str in stringsToCheck)
        {
            countMap[str] = 0;
        }

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                if (countMap.ContainsKey(array[i, j]))
                {
                    countMap[array[i, j]]++;
                }
            }
        }
        return countMap;
    }

    // 가장 적은 개수의 문자열을 찾는 함수
    static string FindMinString(Dictionary<string, int> countMap)
    {
        string minString = "A";
        int minCount = int.MaxValue;

        foreach (var kvp in countMap)
        {
            if (kvp.Value < minCount)
            {
                minCount = kvp.Value;
                minString = kvp.Key;
            }
        }
        return minString;
    }

    // 배열 내부에서 특정 문자가 있는 사각형 내부만 'T'로 변경하는 함수
    public static void ReplaceInternalWithT(string[,] array, string targetString)
    {
        int top = array.GetLength(0), bottom = 0, left = array.GetLength(1), right = 0;

        // 특정 문자가 있는 경계 찾기
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                if (array[i, j] == targetString)
                {
                    if (i < top) top = i;
                    if (i > bottom) bottom = i;
                    if (j < left) left = j;
                    if (j > right) right = j;
                }
            }
        }

        // 사각형 내부 포함 'T'로 변경
        for (int i = top; i <= bottom; i++)
        {
            for (int j = left; j <= right; j++)
            {
                array[i, j] = "T";
                
            }
        }
    }

}