using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class PGGRefactored : MonoBehaviour
{
    // Prefabs
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject cornerPrefab;
    [SerializeField] private GameObject yellowBlockPrefab;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject startPrefab;
    [SerializeField] private GameObject quizDoorPrefab;
    [SerializeField] private GameObject exitPrefab;
    [SerializeField] private GameObject windowPrefab;
    [SerializeField] private GameObject emptyPrefab;
    [SerializeField] private GameObject oneSideCornerBlockPrefab;
    [SerializeField] private GameObject twoSidesCornerBlockPrefab;
    [SerializeField] private GameObject threeSidesCornerBlockPrefab;
    [SerializeField] private GameObject fourSidesCornerBlockPrefab;

    // Grid properties
    [SerializeField, Range(1, 50)] private int width = 1;
    [SerializeField, Range(1, 50)] private int height = 1;
    [SerializeField, Range(1, 100)] private int randomSeed = 1;
    
    private GameObject[,] grid;
    private GameObject[,] gridInnerWall;
    private string[,] gridToCalculate;
    private string[,] gridUserAble;
    private bool[,] visited;
    
    private int prevWidth;
    private int prevHeight;
    private int prevSeed;
    private float spacing = 1.0f;

    // UI Bindings
    [SerializeField] private TextMeshProUGUI numOfShapesText;
    [SerializeField] private TextMeshProUGUI lengthOfTimeText;

    private void Start()
    {
        InitializeGridProperties();
        InitializeGrids();
    }

    private void Update()
    {
        HandleGridChanges();
    }

    private void InitializeGridProperties()
    {
        prevWidth = width;
        prevHeight = height;
        prevSeed = randomSeed;
        Random.InitState(randomSeed);
    }

    private void InitializeGrids()
    {
        grid = new GameObject[height, width];
        gridInnerWall = new GameObject[height, width];
        gridToCalculate = new string[height, width];
        gridUserAble = new string[height, width];
        visited = new bool[height, width];
    }

    private void HandleGridChanges()
    {
        if (HasGridSizeChanged() || HasSeedChanged())
        {
            ClearAllObjects();
            if (HasGridSizeChanged()) ResizeGrids();
            if (HasSeedChanged()) Random.InitState(randomSeed);

            InitializeGrid();
            BuildGrid();
        }
    }

    private bool HasGridSizeChanged()
    {
        return prevWidth != width || prevHeight != height;
    }

    private bool HasSeedChanged()
    {
        return prevSeed != randomSeed;
    }

    private void ResizeGrids()
    {
        prevWidth = width;
        prevHeight = height;
        InitializeGrids();
    }

    private void ClearAllObjects()
    {
        for (int row = 0; row < prevHeight; row++)
        {
            for (int col = 0; col < prevWidth; col++)
            {
                DestroyGridObjects(row, col);
            }
        }
    }

    private void DestroyGridObjects(int row, int col)
    {
        if (grid[row, col] != null)
        {
            Destroy(grid[row, col]);
            Destroy(gridInnerWall[row, col]);

            grid[row, col] = null;
            gridInnerWall[row, col] = null;
            gridToCalculate[row, col] = null;
            gridUserAble[row, col] = null;
        }
    }

    private void InitializeGrid()
    {
        for (int i = 0; i < prevHeight; i++)
        {
            for (int j = 0; j < prevWidth; j++)
            {
                gridToCalculate[i, j] = "0";
                gridUserAble[i, j] = "0";
                visited[i, j] = false;
            }
        }
    }

    private void BuildGrid()
    {
        numOfShapesText.text = $"{width * height}";
        DateTime started = DateTime.Now;

        // Example of a simplified method
        PlaceRandomYellowBlock();
        CalculateInnerWalls();
        ProcessGrid();
    }

    private void PlaceRandomYellowBlock()
    {
        int randomRow = Random.Range(1, height - 1);
        int randomCol = Random.Range(1, width - 1);
        StartCoroutine(AddFloor(randomRow, randomCol, true));
    }

    private void CalculateInnerWalls()
    {
        for (int i = 0; i < prevHeight; i++)
        {
            for (int j = 0; j < prevWidth; j++)
            {
                StartCoroutine(AddInnerWall(i, j));
            }
        }
    }

    private void ProcessGrid()
    {
        // Process the grid as necessary, removing any duplication or redundant steps
        ReplaceLargestZeroArea();
        ReplaceMOnEdgeWithS();
    }

    private IEnumerator AddInnerWall(int row, int col)
    {
        Vector3 position = new Vector3(row * spacing, 0, col * spacing);
        string placeInnerWall = CheckGrid(row, col);

        // Process and instantiate appropriate prefab based on conditions
        GameObject prefabToInstantiate = SelectInnerWallPrefab(placeInnerWall, row, col);
        InstantiatePrefab(prefabToInstantiate, position, row, col);

        yield return null;
    }

    private GameObject SelectInnerWallPrefab(string placeInnerWall, int row, int col)
    {
        // Simplified prefab selection logic
        bool hasAbove = CheckGrid(row - 1, col) != "0";
        bool hasBelow = CheckGrid(row + 1, col) != "0";
        bool hasLeft = CheckGrid(row, col - 1) != "0";
        bool hasRight = CheckGrid(row, col + 1) != "0";

        int connectionCount = (hasAbove ? 1 : 0) + (hasBelow ? 1 : 0) + (hasLeft ? 1 : 0) + (hasRight ? 1 : 0);

        if (connectionCount == 4) return fourSidesCornerBlockPrefab;
        if (connectionCount == 3) return threeSidesCornerBlockPrefab;
        if (connectionCount == 2) return twoSidesCornerBlockPrefab;
        if (connectionCount == 1) return oneSideCornerBlockPrefab;
        if (placeInnerWall == "X_Horizontal" || placeInnerWall == "X_Vertical") return doorPrefab;

        return null;
    }

    private void InstantiatePrefab(GameObject prefab, Vector3 position, int row, int col)
    {
        if (prefab != null)
        {
            GameObject cell = new GameObject($"innerCell_{row}_{col}");
            cell.transform.position = position;
            cell.transform.parent = transform;

            Vector3 blockPosition = new Vector3(position.x, 0.5f, position.z); // Adjust block height
            GameObject instantiatedBlock = Instantiate(prefab, blockPosition, Quaternion.identity);
            instantiatedBlock.transform.parent = cell.transform;

            gridInnerWall[row, col] = cell;
        }
    }

    private string CheckGrid(int row, int col)
    {
        if (row < 0 || row >= gridToCalculate.GetLength(0) || col < 0 || col >= gridToCalculate.GetLength(1))
            return "0";

        return gridToCalculate[row, col];
    }

    private void ReplaceLargestZeroArea()
    {
        int largestArea = 0;
        List<(int, int)> largestAreaPositions = new List<(int, int)>();

        for (int i = 0; i < prevHeight; i++)
        {
            for (int j = 0; j < prevWidth; j++)
            {
                if (gridUserAble[i, j] == "0" && !visited[i, j])
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

        foreach (var pos in largestAreaPositions)
        {
            gridUserAble[pos.Item1, pos.Item2] = "M";
        }
    }

    private int DFS(int x, int y, List<(int, int)> positions)
    {
        if (x < 0 || x >= prevHeight || y < 0 || y >= prevWidth || gridUserAble[x, y] != "0" || visited[x, y])
            return 0;

        visited[x, y] = true;
        positions.Add((x, y));
        int size = 1;

        int[] dx = { 1, -1, 0, 0, 1, 1, -1, -1 };
        int[] dy = { 0, 0, 1, -1, 1, -1, 1, -1 };

        for (int i = 0; i < 8; i++)
        {
            size += DFS(x + dx[i], y + dy[i], positions);
        }

        return size;
    }

    public void ReplaceMOnEdgeWithS()
    {
        List<(int, int)> edgePositions = new List<(int, int)>();

        for (int i = 0; i < prevHeight; i++)
        {
            for (int j = 0; j < prevWidth; j++)
            {
                if (IsEdgeM(i, j))
                    edgePositions.Add((i, j));
            }
        }

        ReplaceEdgeMPositions(edgePositions);
        ReplaceConsecutiveMWithW(edgePositions);
    }

    private bool IsEdgeM(int i, int j)
    {
        return gridUserAble[i, j] == "M" &&
            ((i == 0 || i == prevHeight - 1 || j == 0 || j == prevWidth - 1) && !(i == 0 && j == 0) && !(i == 0 && j == prevWidth - 1) && !(i == prevHeight - 1 && j == 0) && !(i == prevHeight - 1 && j == prevWidth - 1));
    }

    private void ReplaceEdgeMPositions(List<(int, int)> edgePositions)
    {
        if (edgePositions.Count >= 3)
        {
            int firstRandomIndex = Random.Range(0, edgePositions.Count);
            int secondRandomIndex;
            int thirdRandomIndex;

            do
            {
                secondRandomIndex = Random.Range(0, edgePositions.Count);
            } while (secondRandomIndex == firstRandomIndex);

            do
            {
                thirdRandomIndex = Random.Range(0, edgePositions.Count);
            } while (thirdRandomIndex == firstRandomIndex || thirdRandomIndex == secondRandomIndex);

            gridUserAble[edgePositions[firstRandomIndex].Item1, edgePositions[firstRandomIndex].Item2] = "S";
            gridUserAble[edgePositions[secondRandomIndex].Item1, edgePositions[secondRandomIndex].Item2] = "Q";
            gridUserAble[edgePositions[thirdRandomIndex].Item1, edgePositions[thirdRandomIndex].Item2] = "E";
        }
    }

    private void ReplaceConsecutiveMWithW(List<(int, int)> edgePositions)
    {
        List<List<(int, int)>> consecutiveMPositionGroups = new List<List<(int, int)>>();

        if (edgePositions.Count > 0)
        {
            FindConsecutiveMInRowGroups(0, consecutiveMPositionGroups);
            FindConsecutiveMInColumnGroups(0, consecutiveMPositionGroups);

            if (consecutiveMPositionGroups.Count > 0)
            {
                int randomGroupIndex = Random.Range(0, consecutiveMPositionGroups.Count);
                foreach (var (mx, my) in consecutiveMPositionGroups[randomGroupIndex])
                {
                    gridUserAble[mx, my] = "W";
                }
            }
        }
    }

    private void FindConsecutiveMInRowGroups(int row, List<List<(int, int)>> groups)
    {
        for (int j = 1; j < prevWidth - 3; j++)
        {
            if (gridUserAble[row, j] == "M" && gridUserAble[row, j + 1] == "M" && gridUserAble[row, j + 2] == "M")
            {
                List<(int, int)> group = new List<(int, int)> { (row, j), (row, j + 1), (row, j + 2) };
                groups.Add(group);
            }
        }
    }

    private void FindConsecutiveMInColumnGroups(int col, List<List<(int, int)>> groups)
    {
        for (int i = 1; i < prevHeight - 3; i++)
        {
            if (gridUserAble[i, col] == "M" && gridUserAble[i + 1, col] == "M" && gridUserAble[i + 2, col] == "M")
            {
                List<(int, int)> group = new List<(int, int)> { (i, col), (i + 1, col), (i + 2, col) };
                groups.Add(group);
            }
        }
    }

    private IEnumerator AddFloor(int row, int col, bool placeYellowBlock)
    {
        if (grid[row, col] == null)
        {
            Vector3 position = new Vector3(row * spacing, 0, col * spacing);

            GameObject cell = new GameObject($"Cell_{row}_{col}");
            cell.transform.position = position;
            cell.transform.parent = transform;

            GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity);
            floor.transform.parent = cell.transform;

            if (placeYellowBlock)
            {
                Vector3 yellowBlockPosition = new Vector3(position.x, 0.5f, position.z);
                GameObject yellowBlock = Instantiate(yellowBlockPrefab, yellowBlockPosition, Quaternion.identity);
                yellowBlock.transform.parent = cell.transform;
            }

            if (IsEdgeCell(row, col))
            {
                GameObject outerWallPrefab = SelectOuterWallPrefab(row, col);
                Vector3 wallPosition = new Vector3(position.x, 0.5f, position.z);
                Quaternion wallRotation = GetWallRotation(row, col);

                GameObject wall = Instantiate(outerWallPrefab, wallPosition, wallRotation);
                wall.transform.parent = cell.transform;
            }

            HandleCornerPlacement(row, col, position, cell);

            grid[row, col] = cell;
        }

        yield return null;
    }

    private bool IsEdgeCell(int row, int col)
    {
        return (row == 0 || row == height - 1 || col == 0 || col == width - 1) &&
               !(row == 0 && col == 0) &&
               !(row == 0 && col == width - 1) &&
               !(row == height - 1 && col == 0) &&
               !(row == height - 1 && col == width - 1);
    }

    private GameObject SelectOuterWallPrefab(int row, int col)
    {
        if (gridUserAble[row, col] == "S") return startPrefab;
        if (gridUserAble[row, col] == "W") return windowPrefab;
        if (gridUserAble[row, col] == "Q") return quizDoorPrefab;
        if (gridUserAble[row, col] == "E") return exitPrefab;

        return wallPrefab;
    }

    private Quaternion GetWallRotation(int row, int col)
    {
        if (col == 0) return Quaternion.Euler(0, 180, 0);
        if (row == 0) return Quaternion.Euler(0, 270, 0);
        if (row == height - 1 || col == width - 1) return Quaternion.identity;
        return Quaternion.Euler(0, 90, 0);
    }

    private void HandleCornerPlacement(int row, int col, Vector3 position, GameObject cell)
    {
        if (row == 0 && col == 0)
        {
            Vector3 cornerPosition = new Vector3(position.x, 0.5f, position.z + 0.25f);
            Quaternion rotation = Quaternion.Euler(0, 0, 0);
            Instantiate(cornerPrefab, cornerPosition, rotation, cell.transform);
        }
        else if (row == 0 && col == width - 1)
        {
            Vector3 cornerPosition = new Vector3(position.x + 0.25f, 0.5f, position.z);
            Quaternion rotation = Quaternion.Euler(0, 90, 0);
            Instantiate(cornerPrefab, cornerPosition, rotation, cell.transform);
        }
        else if (row == height - 1 && col == 0)
        {
            Vector3 cornerPosition = new Vector3(position.x - 0.25f, 0.5f, position.z);
            Quaternion rotation = Quaternion.Euler(0, 270, 0);
            Instantiate(cornerPrefab, cornerPosition, rotation, cell.transform);
        }
        else if (row == height - 1 && col == width - 1)
        {
            Vector3 cornerPosition = new Vector3(position.x, 0.5f, position.z - 0.25f);
            Quaternion rotation = Quaternion.Euler(0, 180, 0);
            Instantiate(cornerPrefab, cornerPosition, rotation, cell.transform);
        }
    }
}
