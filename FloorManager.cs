using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FloorManager
{
    private PGGbeforeRefactor pggBeforeRefactor;
    private CheckGrids checkGrids;

    // 생성자에서 MainClass 인스턴스를 전달받음
    public FloorManager(PGGbeforeRefactor pggBeforeRefactor)
    {
        this.pggBeforeRefactor = pggBeforeRefactor;
        checkGrids = new CheckGrids(pggBeforeRefactor);
    }


    #region Add Floor

    public IEnumerator AddFloor(int row, int col, bool placeYellowBlock)
    {
        if (pggBeforeRefactor.grid[row, col] == null)
        {
            Vector3 position = new Vector3(row * pggBeforeRefactor.spacing, 0, col * pggBeforeRefactor.spacing);
            GameObject cell = CreateParentCell(row, col, position, pggBeforeRefactor.transform);

            // 바닥 생성 및 부모 오브젝트에 추가
            CreateFloor(position, cell);

            // 노란색 블록 배치 여부에 따라 추가
            if (placeYellowBlock)
            {
                CreateYellowBlock(position, cell);
            }

            // 가장자리 벽을 추가
            if (IsEdge(row, col))
            {
                AddWall(row, col, position, cell);
            }

            // 꼭짓점에 프리팹 배치
            AddCornerPrefab(row, col, position, cell);

            pggBeforeRefactor.grid[row, col] = cell; // 부모 오브젝트를 그리드에 저장
        }

        yield return null;
    }

    // 부모 오브젝트를 생성하는 함수
    GameObject CreateParentCell(int row, int col, Vector3 position, Transform parentTransform)
    {
        GameObject cell = new GameObject($"Cell_{row}_{col}");
        cell.transform.position = position;
        cell.transform.parent = parentTransform;
        return cell;
    }

    // 바닥을 생성하는 함수
    void CreateFloor(Vector3 position, GameObject cell)
    {
        GameObject floor = UnityEngine.Object.Instantiate(pggBeforeRefactor.floorPrefab, position, Quaternion.identity);
        floor.transform.parent = cell.transform;
    }

    // 노란색 블록을 생성하는 함수
    void CreateYellowBlock(Vector3 position, GameObject cell)
    {
        Vector3 yellowBlockPosition = new Vector3(position.x, 0.5f, position.z); // 바닥 위에 배치
        GameObject yellowBlock = UnityEngine.Object.Instantiate(pggBeforeRefactor.yellowBlockPrefab, yellowBlockPosition, Quaternion.identity);
        yellowBlock.transform.parent = cell.transform;
    }

    // 가장자리 벽을 추가하는 함수
    void AddWall(int row, int col, Vector3 position, GameObject cell)
    {
        GameObject outerWallPrefab = GetOuterWallPrefab(row, col);
        Vector3 wallPosition = new Vector3(position.x, 0.5f, position.z);
        Quaternion wallRotation = GetWallRotation(row, col);

        GameObject wall = UnityEngine.Object.Instantiate(outerWallPrefab, wallPosition, wallRotation);
        wall.transform.parent = cell.transform;
    }

    // 외벽 프리팹을 결정하는 함수
    GameObject GetOuterWallPrefab(int row, int col)
    {
        GameObject outerWallPrefab = pggBeforeRefactor.wallPrefab;

        string gridValue = checkGrids.Check_grid_UserAble(row, col);

        if (gridValue == "S") return pggBeforeRefactor.startPrefab;
        if (gridValue == "Q") return pggBeforeRefactor.quizDoorPrefab;
        if (gridValue == "E") return pggBeforeRefactor.exitPrefab;

        if (gridValue == "W")
        {
            if (IsHorizontalWindow(row, col) || IsVerticalWindow(row, col))
            {
                return pggBeforeRefactor.windowPrefab;
            }
            if (IsEmptyHorizontalWindow(row, col) || IsEmptyVerticalWindow(row, col))
            {
                return pggBeforeRefactor.emptyPrefab;
            }
        }

        return outerWallPrefab;
    }

    // 벽의 회전을 결정하는 함수
    Quaternion GetWallRotation(int row, int col)
    {
        if (col == 0) return Quaternion.Euler(0, 180, 0);
        if (row == 0) return Quaternion.Euler(0, 270, 0);
        if (col == pggBeforeRefactor.width - 1) return Quaternion.identity; // 오른쪽 벽은 기본 회전
        return Quaternion.Euler(0, 90, 0); // 나머지 벽은 90도 회전
    }

    // 꼭짓점에 프리팹을 추가하는 함수
    void AddCornerPrefab(int row, int col, Vector3 position, GameObject cell)
    {
        if (row == 0 && col == 0) // 왼쪽 아래 꼭짓점
        {
            Vector3 cornerPosition = new Vector3(position.x, 0.5f, position.z + 0.25f);
            Quaternion rotation = Quaternion.Euler(0, 0, 0); // 기본 회전
            UnityEngine.Object.Instantiate(pggBeforeRefactor.cornerPrefab, cornerPosition, rotation, cell.transform);
        }
        else if (row == 0 && col == pggBeforeRefactor.width - 1) // 오른쪽 아래 꼭짓점
        {
            Vector3 cornerPosition = new Vector3(position.x + 0.25f, 0.5f, position.z);
            Quaternion rotation = Quaternion.Euler(0, 90, 0); // 90도 회전
            UnityEngine.Object.Instantiate(pggBeforeRefactor.cornerPrefab, cornerPosition, rotation, cell.transform);
        }
        else if (row == pggBeforeRefactor.height - 1 && col == 0) // 왼쪽 위 꼭짓점
        {
            Vector3 cornerPosition = new Vector3(position.x - 0.25f, 0.5f, position.z);
            Quaternion rotation = Quaternion.Euler(0, 270, 0); // 270도 회전
            UnityEngine.Object.Instantiate(pggBeforeRefactor.cornerPrefab, cornerPosition, rotation, cell.transform);
        }
        else if (row == pggBeforeRefactor.height - 1 && col == pggBeforeRefactor.width - 1) // 오른쪽 위 꼭짓점
        {
            Vector3 cornerPosition = new Vector3(position.x, 0.5f, position.z - 0.25f);
            Quaternion rotation = Quaternion.Euler(0, 180, 0); // 180도 회전
            UnityEngine.Object.Instantiate(pggBeforeRefactor.cornerPrefab, cornerPosition, rotation, cell.transform);
        }
    }

    // 가장자리인지 확인하는 함수
    bool IsEdge(int row, int col)
    {
        return (row == 0 || row == pggBeforeRefactor.height - 1 || col == 0 || col == pggBeforeRefactor.width - 1) &&
            !(row == 0 && col == 0) &&
            !(row == 0 && col == pggBeforeRefactor.width - 1) &&
            !(row == pggBeforeRefactor.height - 1 && col == 0) &&
            !(row == pggBeforeRefactor.height - 1 && col == pggBeforeRefactor.width - 1);
    }

    // 창문 위치와 상태를 확인하는 함수들
    bool IsHorizontalWindow(int row, int col)
    {
        return checkGrids.Check_grid_UserAble(row, col - 1) == "W" && checkGrids.Check_grid_UserAble(row, col + 1) == "W";
    }

    bool IsVerticalWindow(int row, int col)
    {
        return checkGrids.Check_grid_UserAble(row - 1, col) == "W" && checkGrids.Check_grid_UserAble(row + 1, col) == "W";
    }

    bool IsEmptyHorizontalWindow(int row, int col)
    {
        return (checkGrids.Check_grid_UserAble(row, col - 1) == "W" && checkGrids.Check_grid_UserAble(row, col + 1) != "W") ||
            (checkGrids.Check_grid_UserAble(row, col + 1) == "W" && checkGrids.Check_grid_UserAble(row, col - 1) != "W");
    }

    bool IsEmptyVerticalWindow(int row, int col)
    {
        return (checkGrids.Check_grid_UserAble(row - 1, col) == "W" && checkGrids.Check_grid_UserAble(row + 1, col) != "W") ||
            (checkGrids.Check_grid_UserAble(row + 1, col) == "W" && checkGrids.Check_grid_UserAble(row - 1, col) != "W");
    }

    #endregion

}