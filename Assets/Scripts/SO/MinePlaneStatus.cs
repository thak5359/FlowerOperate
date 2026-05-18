using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MinePlaneStatus", menuName = "Plane/MinePlaneStatus")]
public class MinePlaneStatus : PlaneStatus
{
    [SerializeField] private  List<ChunkData> chunks = new(MAX_CHUNK_COUNT);

    [field: SerializeField] public static int MAX_CHUNK_COUNT { get;  } = 4;

}







//int rows = 15;
//int cols = 15;
//int totalCells = rows * cols; // 225칸
//int targetCount = (int)(totalCells * 0.8); // 80% = 180칸

//// 1. 모든 칸의 인덱스(0 ~ 224)를 리스트에 담습니다.
//List<int> cellIndices = new List<int>();
//for (int i = 0; i < totalCells; i++)
//{
//    cellIndices.Add(i);
//}

//// 2. Fisher-Yates 셔플 알고리즘으로 리스트를 무작위로 섞습니다.
//Random rand = new Random();
//for (int i = totalCells - 1; i > 0; i--)
//{
//    int j = rand.Next(i + 1);
//    // 두 원소의 위치를 바꿉니다 (Swap)
//    int temp = cellIndices[i];
//    cellIndices[i] = cellIndices[j];
//    cellIndices[j] = temp;
//}

//// 3. 앞에서부터 80%에 해당하는 180개만 가져옵니다.
//List<int> selectedIndices = cellIndices.GetRange(0, targetCount);

//// --- 아래는 파트너가 시각적으로 확인할 수 있도록 만든 출력 코드입니다 ---

//// 15x15 그리드에 선택 여부 표시 (true: 선택됨, false: 선택 안 됨)
//bool[,] grid = new bool[rows, cols];
//foreach (int index in selectedIndices)
//{
//    int r = index / cols; // 행 위치
//    int c = index % cols; // 열 위치
//    grid[r, c] = true;
//}

//// 콘솔에 그리드 출력
//int selectedCount = 0;
//for (int r = 0; r < rows; r++)
//{
//    for (int c = 0; c < cols; c++)
//    {
//        if (grid[r, c])
//        {
//            Console.Write("■ "); // 선택된 칸
//            selectedCount++;
//        }
//        else
//        {
//            Console.Write("□ "); // 선택되지 않은 칸
//        }
//    }
//    Console.WriteLine();
//}

//Console.WriteLine($"\n선택된 칸의 개수: {selectedCount}개 (전체 {totalCells}개 중 정확히 80%)");