using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class InnerWallFunctions
{
    #region Add Inner Wall

    private PGGbeforeRefactor pggBeforeRefactor;
    private CheckGrids checkGrids;
    private DrawRectangles drawRectangle;

    // 생성자에서 MainClass 인스턴스를 전달받음
    public InnerWallFunctions(PGGbeforeRefactor pggBeforeRefactor)
    {
        this.pggBeforeRefactor = pggBeforeRefactor;
        checkGrids = new CheckGrids(pggBeforeRefactor);
        drawRectangle = new DrawRectangles(pggBeforeRefactor);
    }


    public IEnumerator AddInnerWall(int row, int col, GameObject[,] grid_innerWall, GameObject floorPrefab, GameObject doorPrefab, float spacing, Transform parentTransform)
    {
        
        Vector3 position = new Vector3(row * spacing, 0, col * spacing);

        // 부모 오브젝트 생성
        GameObject cell = new GameObject($"innerCell_{row}_{col}");
        cell.transform.position = position;
        cell.transform.parent = parentTransform;

        // 보라색 블록 배치 여부
        string placeInnerWall = checkGrids.CheckGrid(row, col);

        // 주변 네 방향의 그리드 상태 확인
        string above = checkGrids.CheckGrid(row - 1, col);
        string below = checkGrids.CheckGrid(row + 1, col);
        string left = checkGrids.CheckGrid(row, col - 1);
        string right = checkGrids.CheckGrid(row, col + 1);

        GameObject prefabToInstantiate = null;
        Quaternion wallRotation = Quaternion.identity;
        float positionX = position.x;
        float positionZ = position.z;

        if (IsPlaceableWall(placeInnerWall))
        {
            var (hasAbove, hasBelow, hasLeft, hasRight, connectionCount) = GetWallConnections(above, below, left, right);

            AdjustForSpecialCases(ref hasAbove, ref hasBelow, ref hasLeft, ref hasRight, ref connectionCount, above, below, left, right);

            prefabToInstantiate = SelectPrefab(connectionCount, hasAbove, hasBelow, hasLeft, hasRight, ref wallRotation, ref positionX, ref positionZ);
        }
        else if (placeInnerWall == "X_Horizontal" || placeInnerWall == "X_Vertical")
        {
            prefabToInstantiate = doorPrefab;
            wallRotation = (placeInnerWall == "X_Vertical") ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
        }

        InstantiateBlock(prefabToInstantiate, cell, positionX, positionZ, wallRotation);

        pggBeforeRefactor.grid_innerWall[row, col] = cell; // 부모 오브젝트를 그리드에 저장    

        yield return null;
    }

    // 벽을 배치할 수 있는지 확인하는 함수
    bool IsPlaceableWall(string placeInnerWall) 
    {
        return placeInnerWall == "A" || placeInnerWall == "B" || placeInnerWall == "C" || placeInnerWall == "D" || placeInnerWall == "P";
    }

    (bool, bool, bool, bool, int) GetWallConnections(string above, string below, string left, string right)
    {
        bool hasAbove = IsValidConnection(above);
        bool hasBelow = IsValidConnection(below);
        bool hasLeft = IsValidConnection(left);
        bool hasRight = IsValidConnection(right);

        int connectionCount = (hasAbove ? 1 : 0) + (hasBelow ? 1 : 0) + (hasLeft ? 1 : 0) + (hasRight ? 1 : 0);

        return (hasAbove, hasBelow, hasLeft, hasRight, connectionCount);
    }

    // 유효한 벽 연결인지 확인하는 함수
    bool IsValidConnection(string gridState)
    {
        return gridState != "0" && gridState != "M" && gridState != null;
    }

    // 특수한 케이스를 처리하는 함수
    void AdjustForSpecialCases(ref bool hasAbove, ref bool hasBelow, ref bool hasLeft, ref bool hasRight, ref int connectionCount, string above, string below, string left, string right)
    {
        if (above == "X_Vertical" && hasBelow && hasLeft) 
        {
            hasAbove = false;
            connectionCount--;
        }
        else if (above == "X_Vertical" && !hasBelow && hasLeft && hasRight)
        {
            hasAbove = false;
            connectionCount--;
        }
        else if (below == "X_Vertical")
        {
            hasBelow = false;
            connectionCount--;
        }
        else if (left == "X_Horizontal" && !hasRight && hasAbove && hasBelow)
        {
            hasLeft = false;
            connectionCount--;
        }
        else if (right == "X_Horizontal" && hasAbove)
        {
            hasRight = false;
            connectionCount--;
        }
        else if (left == "X_Horizontal" && hasRight && hasAbove && !hasBelow)
        {
            hasLeft = false;
            connectionCount--;
        }     
        else if (left == "X_Horizontal" && hasRight && hasAbove && hasBelow)
        {
            hasLeft = false;
            connectionCount--;
        }    
    }

    // 연결된 벽 상태에 따라 적절한 프리팹을 선택하는 함수
    GameObject SelectPrefab(int connectionCount, bool hasAbove, bool hasBelow, bool hasLeft, bool hasRight, ref Quaternion wallRotation, ref float positionX, ref float positionZ)
    {
        GameObject prefabToInstantiate = null;

        if (connectionCount == 4)
        {
            prefabToInstantiate = pggBeforeRefactor.fourSidesCornerBlockPrefab;
        }
        else if (connectionCount == 3)
        {
            prefabToInstantiate = pggBeforeRefactor.threeSidesCornerBlockPrefab;
            wallRotation = SelectRotationForThreeConnections(hasAbove, hasBelow, hasLeft);
        }
        else if (connectionCount == 2)
        {
            prefabToInstantiate = SelectPrefabForTwoConnections(hasAbove, hasBelow, hasLeft, hasRight, ref wallRotation, ref positionX, ref positionZ);
        }
        else if (connectionCount == 1)
        {
            prefabToInstantiate = pggBeforeRefactor.oneSideCornerBlockPrefab;
        }

        return prefabToInstantiate;
    }

    // 세 방향 연결에 따른 회전 설정 함수
    Quaternion SelectRotationForThreeConnections(bool hasAbove, bool hasBelow, bool hasLeft)
    {
        if (!hasAbove) return Quaternion.Euler(0, 0, 0);
        if (!hasBelow) return Quaternion.Euler(0, 180, 0);
        if (!hasLeft) return Quaternion.Euler(0, 270, 0);
        return Quaternion.Euler(0, 90, 0);
    }

    // 두 방향 연결에 맞는 프리팹과 회전, 위치 조정
    GameObject SelectPrefabForTwoConnections(bool hasAbove, bool hasBelow, bool hasLeft, bool hasRight, ref Quaternion wallRotation, ref float positionX, ref float positionZ)
    {
        if (hasAbove && hasBelow) return pggBeforeRefactor.oneSideCornerBlockPrefab;
        if (hasLeft && hasRight) 
        {
            wallRotation = Quaternion.Euler(0, 90, 0);
            return pggBeforeRefactor.oneSideCornerBlockPrefab;
        }
        if (hasAbove && hasRight) 
        {
            wallRotation = Quaternion.Euler(0, 270, 0);
            positionX -= 0.25f;
            return pggBeforeRefactor.twoSidesCornerBlockPrefab;
        }
        if (hasAbove && hasLeft) 
        {
            wallRotation = Quaternion.Euler(0, 180, 0);
            positionZ -= 0.25f;
            return pggBeforeRefactor.twoSidesCornerBlockPrefab;
        }
        if (hasBelow && hasRight) 
        {
            wallRotation = Quaternion.Euler(0, 0, 0);
            positionZ += 0.25f;
            return pggBeforeRefactor.twoSidesCornerBlockPrefab;
        }
        wallRotation = Quaternion.Euler(0, 90, 0);
        positionX += 0.25f;
        return pggBeforeRefactor.twoSidesCornerBlockPrefab;
    }

    // 프리팹을 인스턴스화하는 함수
    void InstantiateBlock(GameObject prefab, GameObject parent, float positionX, float positionZ, Quaternion rotation)
    {
        if (prefab == null) return;
        
        Vector3 blockPosition = new Vector3(positionX, 0.5f, positionZ);
        GameObject instantiatedBlock = UnityEngine.Object.Instantiate(prefab, blockPosition, rotation);
        instantiatedBlock.transform.parent = parent.transform;
    }
    
    public void CalculateInnerWall(int block_x, int block_y)
    {
        Vector2Int[] startingPoints = new Vector2Int[]
        {
            new Vector2Int(0, block_y), //TopLeft
            new Vector2Int(block_x, 0), //TopRight
            new Vector2Int(pggBeforeRefactor.height, block_y), //BottomLeft
            new Vector2Int(block_x, pggBeforeRefactor.width) //BottomRight
        };

        // 직사각형의 크기 범위
        int minWidth = 3; 
        int minHeight = 3;
        int maxWidth = Mathf.CeilToInt((pggBeforeRefactor.height + pggBeforeRefactor.width) / 4.0f);
        int maxHeight = Mathf.CeilToInt((pggBeforeRefactor.height + pggBeforeRefactor.width) / 4.0f);

        // 각 위치에서 직사각형 그리기
        char[] labels = { 'A', 'B', 'C', 'D' };
        for (int i = 0; i < startingPoints.Length; i++)
        {
            int width = UnityEngine.Random.Range(minWidth, maxWidth);
            int height = UnityEngine.Random.Range(minHeight, maxHeight);

            /* 여기까진 오케이 */
            drawRectangle.DrawRectangle(startingPoints[i], new Vector2Int(width, height), labels[i]);
        }
        
        drawRectangle.DrawRectangleBlank();
        
    }

    #endregion

    public void grid_to_CalculatePrintGrid()
    {
        
        for (int i = 0; i < pggBeforeRefactor.prevHeight; i++)
        {
            string row = "";
            for (int j = 0; j < pggBeforeRefactor.prevWidth; j++)
            {
                row += pggBeforeRefactor.grid_UserAble[i, j].ToString() + " ";
            }
            Debug.Log(row);
        }
    }



}