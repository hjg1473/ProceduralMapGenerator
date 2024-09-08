using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ReplaceGrid
{
    private PGGbeforeRefactor pggBeforeRefactor;
    // 생성자에서 MainClass 인스턴스를 전달받음
    public ReplaceGrid(PGGbeforeRefactor pggBeforeRefactor)
    {
        this.pggBeforeRefactor = pggBeforeRefactor;
    }



    #region Processs

    public static void ProcessArray(string[,] array, char[] targetChars)
    {
        var candidates = FindDoorCandidates(array, targetChars);
        ReplaceWithX(array, candidates);
    }

    private static Dictionary<char, List<(int, int)>> FindDoorCandidates(string[,] array, char[] targetChars)
    {
        int rows = array.GetLength(0) - 1;
        int cols = array.GetLength(1) - 1;
        var candidates = new Dictionary<char, List<(int, int)>>();

        foreach (char ch in targetChars)
        {
            candidates[ch] = new List<(int, int)>();
        }

        for (int i = 1; i < rows; i++)
        {
            for (int j = 1; j < cols; j++)
            {
                if (array[i, j].Length == 1)
                {
                    char currentChar = array[i, j][0]; // string에서 char로 변환
                    if (Array.Exists(targetChars, element => element == currentChar))
                    {
                        // 상하로 연속된 문자만 후보에 추가 (상하 모두 같은 문자)
                        if (i > 0 && i < rows - 1)
                        {
                            if (array[i - 1, j][0] == currentChar && array[i + 1, j][0] == currentChar)
                            {
                                candidates[currentChar].Add((i, j));
                            }
                        }

                        // 좌우로 연속된 문자만 후보에 추가 (좌우 모두 같은 문자)
                        if (j > 0 && j < cols - 1)
                        {
                            if (array[i, j - 1][0] == currentChar && array[i, j + 1][0] == currentChar)
                            {
                                candidates[currentChar].Add((i, j));
                            }
                        }
                    }
                }
            }
        }

        return candidates;
    }

    private static void ReplaceWithX(string[,] array, Dictionary<char, List<(int, int)>> candidates)
    {
        foreach (var candidate in candidates)
        {
            if (candidate.Value.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, candidate.Value.Count);
                var selected = candidate.Value[randomIndex];
                array[selected.Item1, selected.Item2] = "X"; // string으로 변경
            }
        }
    }

    // DFS 탐색
    int DFS(int x, int y, List<(int, int)> positions)
    {
        if (x < 0 || x >= pggBeforeRefactor.prevHeight || y < 0 || y >= pggBeforeRefactor.prevWidth || pggBeforeRefactor.grid_UserAble[x, y] != "0" || pggBeforeRefactor.visited[x, y])
            return 0;

        pggBeforeRefactor.visited[x, y] = true;
        positions.Add((x, y));
        int size = 1;

        // 상하좌우 및 대각선 방향
        int[] dx = { 1, -1, 0, 0, 1, 1, -1, -1 };
        int[] dy = { 0, 0, 1, -1, 1, -1, 1, -1 };

        for (int i = 0; i < 8; i++)
        {
            size += DFS(x + dx[i], y + dy[i], positions);
        }

        return size;
    }

    // 가장 큰 0의 영역을 "M"으로 바꾸는 함수
    public void ReplaceLargestZeroArea()
    {
        int largestArea = 0;
        List<(int, int)> largestAreaPositions = new List<(int, int)>();

        for (int i = 0; i < pggBeforeRefactor.prevHeight; i++)
        {
            for (int j = 0; j < pggBeforeRefactor.prevWidth; j++)
            {
                if (pggBeforeRefactor.grid_UserAble[i, j] == "0" && !pggBeforeRefactor.visited[i, j])
                {
                    List<(int, int)> currentAreaPositions = new List<(int, int)>();
                    
                    int areaSize = DFS(i, j, currentAreaPositions);

                    if (areaSize > largestArea)
                    {
                        largestArea = areaSize;
                        largestAreaPositions = new List<(int, int)>(currentAreaPositions);
                    }
                }
            }
        }

        // 가장 큰 영역의 좌표를 "M"으로 대체
        foreach (var pos in largestAreaPositions)
        {
            pggBeforeRefactor.grid_UserAble[pos.Item1, pos.Item2] = "M";
        }
    }

    #endregion
    

    #region ReplaceMOnEdgeWithS

    public void ReplaceMOnEdgeWithS()
    {
        List<(int, int)> edgePositions = FindEdgeMPositions();

        // 랜덤으로 세 개 선택해서 "S", "Q", "E"로 변경
        if (edgePositions.Count >= 3)
        {
            var (first, second, third) = GetThreeRandomUniqueIndices(edgePositions.Count);
            var (x1, y1) = edgePositions[first];
            var (x2, y2) = edgePositions[second];
            var (x3, y3) = edgePositions[third];

            // 첫 번째 선택한 위치는 "S", 두 번째는 "Q", 세 번째는 "E"
            pggBeforeRefactor.grid_UserAble[x1, y1] = "S";
            pggBeforeRefactor.grid_UserAble[x2, y2] = "Q";
            pggBeforeRefactor.grid_UserAble[x3, y3] = "E";

            // 맞은편에서 "M"이 3연속으로 있는 그룹을 찾아 변경
            HandleOppositeMConsecutiveGroups(x1, y1);
        }
    }

    // 가장자리에서 "M"을 찾는 함수 (꼭짓점 제외)
    List<(int, int)> FindEdgeMPositions()
    {
        List<(int, int)> edgePositions = new List<(int, int)>();
        
        for (int i = 0; i < pggBeforeRefactor.prevHeight; i++)
        {
            for (int j = 0; j < pggBeforeRefactor.prevWidth; j++)
            {
                if (pggBeforeRefactor.grid_UserAble[i, j] == "M" && IsEdge(i, j))
                {
                    edgePositions.Add((i, j));
                }
            }
        }
        return edgePositions;
    }

    // 인덱스가 꼭짓점 제외한 가장자리인지 확인하는 함수
    bool IsEdge(int i, int j)
    {
        return (i == 0 && j > 0 && j < pggBeforeRefactor.prevWidth - 1) ||      // 상단
            (i == pggBeforeRefactor.prevHeight - 1 && j > 0 && j < pggBeforeRefactor.prevWidth - 1) || // 하단
            (j == 0 && i > 0 && i < pggBeforeRefactor.prevHeight - 1) ||      // 좌측
            (j == pggBeforeRefactor.prevWidth - 1 && i > 0 && i < pggBeforeRefactor.prevHeight - 1);   // 우측
    }


    // 랜덤으로 서로 다른 3개의 인덱스를 반환하는 함수
    // 두 개 이상의 요소를 가진 튜플을 반환
    (int, int, int) GetThreeRandomUniqueIndices(int count)
    {
        int first = UnityEngine.Random.Range(0, count);
        int second, third;

        // 두 번째 인덱스는 첫 번째와 겹치지 않게 선택
        do
        {
            second = UnityEngine.Random.Range(0, count);
        } while (second == first);

        // 세 번째 인덱스는 첫 번째, 두 번째와 겹치지 않게 선택
        do
        {
            third = UnityEngine.Random.Range(0, count);
        } while (third == first || third == second);

        return (first, second, third);
    }

    // 맞은편에서 "M"이 3연속으로 있는 그룹을 찾아 처리하는 함수
    void HandleOppositeMConsecutiveGroups(int x1, int y1)
    {
        List<List<(int, int)>> consecutiveMPositionGroups = new List<List<(int, int)>>();

        // 상하 맞은편 처리
        if (x1 == 0)
        {
            FindConsecutiveMInRowGroups(pggBeforeRefactor.prevHeight - 1, consecutiveMPositionGroups);
        }
        else if (x1 == pggBeforeRefactor.prevHeight - 1)
        {
            FindConsecutiveMInRowGroups(0, consecutiveMPositionGroups);
        }
        // 좌우 맞은편 처리
        else if (y1 == 0)
        {
            FindConsecutiveMInColumnGroups(pggBeforeRefactor.prevWidth - 1, consecutiveMPositionGroups);
        }
        else if (y1 == pggBeforeRefactor.prevWidth - 1)
        {
            FindConsecutiveMInColumnGroups(0, consecutiveMPositionGroups);
        }

        // "M"이 3연속인 그룹을 랜덤으로 선택하여 "W"로 변경
        if (consecutiveMPositionGroups.Count > 0)
        {
            int randomGroupIndex = UnityEngine.Random.Range(0, consecutiveMPositionGroups.Count);
            foreach (var (mx, my) in consecutiveMPositionGroups[randomGroupIndex])
            {
                pggBeforeRefactor.grid_UserAble[mx, my] = "W";
            }
        }
    }

    // 가로줄에서 "M"이 3연속인 위치 찾기
    private void FindConsecutiveMInRowGroups(int row, List<List<(int, int)>> groups)
    {
        for (int j = 1; j < pggBeforeRefactor.prevWidth - 3; j++)
        {
            if (pggBeforeRefactor.grid_UserAble[row, j] == "M" && pggBeforeRefactor.grid_UserAble[row, j + 1] == "M" && pggBeforeRefactor.grid_UserAble[row, j + 2] == "M")
            {
                List<(int, int)> group = new List<(int, int)> { (row, j), (row, j + 1), (row, j + 2) };
                groups.Add(group);
            }
        }
    }

    // 세로줄에서 "M"이 3연속인 위치 찾기
    private void FindConsecutiveMInColumnGroups(int col, List<List<(int, int)>> groups)
    {
        for (int i = 1; i < pggBeforeRefactor.prevHeight - 3; i++)
        {
            if (pggBeforeRefactor.grid_UserAble[i, col] == "M" && pggBeforeRefactor.grid_UserAble[i + 1, col] == "M" && pggBeforeRefactor.grid_UserAble[i + 2, col] == "M")
            {
                
                List<(int, int)> group = new List<(int, int)> { (i, col), (i + 1, col), (i + 2, col) };
                groups.Add(group);
            }
        }
    }

    #endregion

}
