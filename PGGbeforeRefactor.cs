using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class PGGbeforeRefactor : MonoBehaviour
{

    [SerializeField] public GameObject floorPrefab;
    [SerializeField] public GameObject oneSidetoiletFloorPrefab;
    [SerializeField] public GameObject twoSidetoiletFloorPrefab;
    [SerializeField] public GameObject threeSidetoiletFloorPrefab;
    [SerializeField] public GameObject wallPrefab;
    [SerializeField] public GameObject cornerPrefab;
    [SerializeField] public GameObject yellowBlockPrefab;
    [SerializeField] public GameObject doorPrefab;
    [SerializeField] public GameObject startPrefab;
    [SerializeField] public GameObject quizDoorPrefab;
    [SerializeField] public GameObject exitPrefab;
    [SerializeField] public GameObject windowPrefab;
    [SerializeField] public GameObject emptyPrefab;
    [SerializeField] public GameObject oneSideCornerBlockPrefab;
    [SerializeField] public GameObject twoSidesCornerBlockPrefab;
    [SerializeField] public GameObject threeSidesCornerBlockPrefab;
    [SerializeField] public GameObject fourSidesCornerBlockPrefab;
    [SerializeField] public GameObject toiletSinkPrefab;
    [SerializeField] public GameObject toiletBidetPrefab;
    [SerializeField] public GameObject toiletCabinetPrefab;
    [SerializeField] public GameObject toiletTisuePrefab;
    [SerializeField] public GameObject kitchenSinkPrefab;
    [SerializeField] public GameObject kitchenValvePrefab;
    [SerializeField] public GameObject kitchenRefPrefab;
    [SerializeField] public GameObject kitchenCabinetPrefab;

    public float spacing = 1.0f; // 평면 사이의 간격
    [SerializeField, Range(1, 50)]
    public int width = 1;
    public int prevWidth = 1;

    [SerializeField, Range(1, 50)]
    public int height = 1;
    public int prevHeight = 1;

    // [SerializeField]
    public GameObject[,] grid = null;
    public GameObject[,] grid_innerWall = null;
    public GameObject[,] grid_ToiletObject = null;
    public string[,] grid_to_Calculate = null;
    public string[,] grid_UserAble = null;
    public string[,] grid_Kitchen = null;
    public string[,] grid_Toilet = null;
    public bool[,] visited = null;

    [SerializeField, Range(1, 100)]
    private int randomSeed = 1;

    private int prevSeed = 1;

    // [SerializeField]
    // private TextMeshProUGUI numOfShapesText = null;
    // [SerializeField]
    // private TextMeshProUGUI lengthOfTimeText = null;

    private InnerWallFunctions innerWallFunctions;
    private GridUtils gridUtils;
    private CheckGrids checkGrids; 
    private ReplaceGrid replaceGrid; 
    private FloorManager floorManger;
    private DecisionRooms decisionRooms;
    private ToiletManager toiletmanager;

    void Start()
    {
        
        prevWidth = width;
        prevHeight = height;

        Random.InitState(randomSeed);
        grid = new GameObject[height, width];
        grid_innerWall = new GameObject[height, width];
        grid_ToiletObject = new GameObject[height, width];

        // 내부 벽 계산용
        grid_to_Calculate = new string[height, width];
        // 현관문 계산용
        grid_UserAble = new string[height, width];
        grid_Kitchen = new string[height, width]; 
        grid_Toilet = new string[height, width];
        visited = new bool[height, width];
        // InitializeGrid();

        // BuildGrid();
        innerWallFunctions = new InnerWallFunctions(this);
        gridUtils = new GridUtils(this);
        checkGrids = new CheckGrids(this);
        replaceGrid = new ReplaceGrid(this);
        floorManger = new FloorManager(this);
        decisionRooms = new DecisionRooms(this);
        toiletmanager = new ToiletManager(this);
    }

    // Update is called once per frame
    void Update()
    {
        bool seedChanged = prevSeed != randomSeed;
        bool sizeChanged = prevWidth != width || prevHeight != height;

        if (seedChanged || sizeChanged)
        {
            // 그리드를 다시 빌드하기 전에 기존 오브젝트를 삭제
            gridUtils.ClearAll();
    
            if (sizeChanged)
            {
                // 그리드 크기 재설정
                prevWidth = width;
                prevHeight = height;
                grid = new GameObject[height, width];
                grid_innerWall = new GameObject[height, width];
                grid_ToiletObject = new GameObject[height, width];

                grid_to_Calculate = new string[height, width];
                grid_UserAble = new string[height, width];
                grid_Kitchen = new string[height, width];
                grid_Toilet = new string[height, width];
                visited = new bool[height, width];
            }
            // 새로운 시드로 초기화
            if (seedChanged)
            {
                Random.InitState(randomSeed);
                prevSeed = randomSeed;
            }
            gridUtils.InitializeGrid();
            BuildGrid();
        }
    }

    void BuildGrid()
    {
        /* 랜덤 선택 */
        int randomRow = Random.Range(1, height - 1);
        int randomCol = Random.Range(1, width - 1);
        // 직사각형 그리기. 
        innerWallFunctions.CalculateInnerWall(randomRow, randomCol);
        // 여기서 면적이 가장 작은 문자를 선택. 화장실로. 
        // 중첩된 반복문을 사용하여 배열 복사
        gridUtils.CopyArrayAndModifyXto0(grid_to_Calculate, grid_Toilet, prevHeight, prevWidth);
        // A, B, C, D 중에 가장 작은 개수의 문자를 T로 변경
        
        // 문 생성
        char[] targetChars = { 'A', 'B', 'C', 'D' };
        ReplaceGrid.ProcessArray(grid_to_Calculate, targetChars);
        // 중첩된 반복문을 사용하여 배열 복사
        gridUtils.CopyArrayAndModifyXto0(grid_to_Calculate, grid_UserAble, prevHeight, prevWidth);


        // 현관문 후보 grid_UserAble 사용. 0을 M으로 바꾸기
        replaceGrid.ReplaceLargestZeroArea();
        // 내부문을 이용하는 곳도 M으로 변경.
        CheckGridEdges(grid_UserAble, prevHeight, prevWidth);
        // 여러 M 중 하나를 S + W + E 로 변경.
        replaceGrid.ReplaceMOnEdgeWithS();

        // 중첩된 반복문을 사용하여 UserAble 배열 복사
        gridUtils.CopyArrayAndModifyXto0(grid_UserAble, grid_Kitchen, prevHeight, prevWidth);
        // 주방의 영역을 K로 설정

        DecisionRooms.ReplaceMinStringWithT(grid_Toilet, new string[] { "A", "B", "C", "D" });
        ToiletManager.edgeTChange(grid_Toilet);

        // 그리드 출력
        // innerWallFunctions.grid_to_CalculatePrintGrid();
        
        for (int i = 0; i < prevHeight; i++)
        {
            string row = "";
            for (int j = 0; j < prevWidth; j++)
            {
                row += grid_Toilet[i, j].ToString() + " ";
            }
            Debug.Log(row);
        }
        // 배열을 기준으로 오브젝트 생성
        for(int row = 0; row < prevHeight; row++)
        {
            for(int col = 0; col < prevWidth; col++)
            {
                StartCoroutine(floorManger.AddFloor(row, col, row == randomRow && col == randomCol));
                if(checkGrids.Check_grid_UserAble(row, col) != "S" && checkGrids.Check_grid_UserAble(row, col) != "Q")
                    StartCoroutine(innerWallFunctions.AddInnerWall(row, col, grid_innerWall, floorPrefab, doorPrefab, spacing, transform));
            }
        }

    }

    public void CheckGridEdges(string[,] grid_UserAble, int prevHeight, int prevWidth)
    {
        // 검사할 타겟 배열
        string[] targets = { "A", "B", "C", "D" };

        // 세로 검사
        for (int i = 1; i < prevHeight - 1; i++)
        {
            // 왼쪽 검사
            if (targets.Contains(grid_UserAble[i, 0]) &&
                targets.Contains(grid_UserAble[i - 1, 0]) &&
                targets.Contains(grid_UserAble[i + 1, 0]) &&
                grid_UserAble[i, 1] == "M")
            {
                grid_UserAble[i, 0] = "M";  // 현재 위치를 "M"으로 변경
            }
            // 오른쪽 검사
            if (targets.Contains(grid_UserAble[i, prevWidth - 1]) &&
                targets.Contains(grid_UserAble[i - 1, prevWidth - 1]) &&
                targets.Contains(grid_UserAble[i + 1, prevWidth - 1]) &&
                grid_UserAble[i, prevWidth - 2] == "M")
            {
                grid_UserAble[i, prevWidth - 1] = "M";  // 현재 위치를 "M"으로 변경
            }
        }

        // 가로 검사
        for (int j = 1; j < prevWidth - 1; j++)
        {
            // 위쪽 검사
            if (targets.Contains(grid_UserAble[0, j]) &&
                targets.Contains(grid_UserAble[0, j - 1]) &&
                targets.Contains(grid_UserAble[0, j + 1]) &&
                grid_UserAble[1, j] == "M")
            {
                grid_UserAble[0, j] = "M";  // 현재 위치를 "M"으로 변경
            }
            // 아래쪽 검사
            if (targets.Contains(grid_UserAble[prevHeight - 1, j]) &&
                targets.Contains(grid_UserAble[prevHeight - 1, j - 1]) &&
                targets.Contains(grid_UserAble[prevHeight - 1, j + 1]) &&
                grid_UserAble[prevHeight - 2, j] == "M")
            {
                grid_UserAble[prevHeight - 1, j] = "M";  // 현재 위치를 "M"으로 변경
            }
        }
    }

}