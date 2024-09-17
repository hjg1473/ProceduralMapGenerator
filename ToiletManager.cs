using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Linq;

public class ToiletManager
{
    private PGGbeforeRefactor pggBeforeRefactor;
    private CheckGrids checkGrids;
    

    // 생성자에서 MainClass 인스턴스를 전달받음
    public ToiletManager(PGGbeforeRefactor pggBeforeRefactor)
    {
        this.pggBeforeRefactor = pggBeforeRefactor;
        checkGrids = new CheckGrids(pggBeforeRefactor);
    }

    // 화장실(T) 의 개수
    int calculateToiletSize()
    {
        int count = 0;
        
        for (int i = 0; i < pggBeforeRefactor.grid_Toilet.GetLength(0); i++)
        {
            for (int j = 0; j < pggBeforeRefactor.grid_Toilet.GetLength(1); j++)
            {
                if (checkGrids.Check_grid_toilet(i, j) == "T")
                {
                    count++;
                }
            }
        }

        return count;
    }

    // T가 가장자리인지 확인하는 함수
    public static void edgeTChange(string[,] array)
    {
        // 가장자리의 좌표 저장할 리스트
        List<(int, int)> edgePositions = new List<(int, int)>();

        // 배열을 순회하며 T의 가장자리 찾기
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                if (array[i, j] == "T")
                {
                    // 상
                    if (i > 0 && array[i - 1, j] != "T")
                        edgePositions.Add((i, j));
                    // 하
                    if (i < array.GetLength(0) - 1 && array[i + 1, j] != "T")
                        edgePositions.Add((i, j));
                    // 좌
                    if (j > 0 && array[i, j - 1] != "T")
                        edgePositions.Add((i, j));
                    // 우
                    if (j < array.GetLength(1) - 1 && array[i, j + 1] != "T")
                        edgePositions.Add((i, j));
                }
            }
        }

        var randomEdges = edgePositions.OrderBy(x => Random.value).Take(4).ToList(); // UnityEngine.Random.value 사용

        // 선택된 가장자리를 'a', 'b', 'c', 'd'로 변경
        string[] replacements = { "a", "b", "c", "d" };
        for (int i = 0; i < randomEdges.Count; i++)
        {
            (int row, int col) = randomEdges[i];
            array[row, col] = replacements[i];
        }

    
    }

    

}