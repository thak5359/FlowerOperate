using Unity.Entities;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class BlobDataViewer : EditorWindow
{
    private string blobFolderPath = "Assets/StreamingAssets/Blobs";

    [MenuItem("Tools/Blob Data Viewer")]
    public static void ShowWindow() => GetWindow<BlobDataViewer>("Blob Viewer");

    private void OnGUI()
    {
        GUILayout.Label("Blob Data Explorer (Natural Language)", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        blobFolderPath = EditorGUILayout.TextField("Blob 폴더 경로", blobFolderPath);

        if (GUILayout.Button("모든 Blob 파일 해석해서 로그로 보기", GUILayout.Height(40)))
        {
            ReadAllBlobs();
        }
    }

    private void ReadAllBlobs()
    {
        if (!Directory.Exists(blobFolderPath))
        {
            Debug.LogError($"경로를 찾을 수 없습니다: {blobFolderPath}");
            return;
        }

        string[] files = Directory.GetFiles(blobFolderPath, "*.blob");
        if (files.Length == 0)
        {
            Debug.LogWarning("해당 경로에 .blob 파일이 없습니다.");
            return;
        }

        Debug.Log($"<color=cyan><b>=== 총 {files.Length}개의 Blob 파일 해석 시작 ===</b></color>");

        foreach (string path in files)
        {
            string fileName = Path.GetFileName(path);
            
            // 파일명을 기반으로 타입을 추측하여 읽기 시도
            if (fileName.Contains("Detail"))
            {
                if (fileName.Contains("Flower")) ReadFlowerDetail(path);
                else if (fileName.Contains("Usable")) ReadUsableDetail(path);
                else Debug.LogWarning($"[{fileName}] 지원하지 않는 Detail 타입입니다.");
            }
            else
            {
                // ID 데이터류 (FlowerIdData, UsableIdData 등)
                // 파일명에 'Usable'이 포함되어 있으면 UsableItemBlobDatas로 먼저 시도
                if (fileName.Contains("Flower")) ReadFlowerItems(path);
                else if (fileName.Contains("Usable")) ReadUsableItems(path);
                else ReadCommonItems(path);
            }
        }
    }

    private void ReadFlowerItems(string path)
    {
        if (BlobAssetReference<FlowerItemBlobDatas>.TryRead(path, 1, out var blobRef))
        {
            Debug.Log($"<color=yellow>[꽃 아이템 데이터] 파일: {Path.GetFileName(path)} (총 {blobRef.Value.Items.Length}개)</color>");
            for (int i = 0; i < blobRef.Value.Items.Length; i++)
            {
                ref var item = ref blobRef.Value.Items[i];
                Debug.Log($"  ▶ <b>[{item.ItemName.ToString()}]</b> ID: {item.ItemId} | " +
                          $"품종Idx: {item.speciesIndex}, 색상Idx: {item.colorIndex}, 꽃말Idx: {item.floroIndex}/{item.floroIndex2} | " +
                          $"성장시간: {item.growthDuration}일, 수확량: {item.harvestAmount}개 | " +
                          $"설명: {item.Description.ToString()}");
            }
            blobRef.Dispose();
        }
    }

    private void ReadUsableItems(string path)
    {
        if (BlobAssetReference<UsableItemBlobDatas>.TryRead(path, 1, out var blobRef))
        {
            Debug.Log($"<color=orange>[소모품 아이템 데이터] 파일: {Path.GetFileName(path)} (총 {blobRef.Value.Items.Length}개)</color>");
            for (int i = 0; i < blobRef.Value.Items.Length; i++)
            {
                ref var item = ref blobRef.Value.Items[i];
                Debug.Log($"  ▶ <b>[{item.ItemName.ToString()}]</b> ID: {item.ItemId} | " +
                          $"지속시간Idx: {item.durationIndex}, 파워Idx: {item.powerIndex}, 차지Idx: {item.chargeIndex} | " +
                          $"설명: {item.Description.ToString()}");
            }
            blobRef.Dispose();
        }
        else
        {
            // 만약 UsableItemBlobDatas로 읽기 실패 시 일반 ItemBlobDatas로 재시도
            ReadCommonItems(path);
        }
    }

    private void ReadCommonItems(string path)
    {
        if (BlobAssetReference<ItemBlobDatas>.TryRead(path, 1, out var blobRef))
        {
            Debug.Log($"<color=white>[일반 아이템 데이터] 파일: {Path.GetFileName(path)} (총 {blobRef.Value.Items.Length}개)</color>");
            for (int i = 0; i < blobRef.Value.Items.Length; i++)
            {
                ref var item = ref blobRef.Value.Items[i];
                Debug.Log($"  ▶ <b>[{item.ItemName.ToString()}]</b> ID: {item.ItemId} | 가격: {item.Price}G | 설명: {item.Description.ToString()}");
            }
            blobRef.Dispose();
        }
    }

    private void ReadFlowerDetail(string path)
    {
        if (BlobAssetReference<FlowerDetailBlobDatas>.TryRead(path, 1, out var blobRef))
        {
            Debug.Log($"<color=green>[꽃 상세 정의 데이터] 파일: {Path.GetFileName(path)} (총 {blobRef.Value.flowerDetails.Length}개)</color>");
            for (int i = 0; i < blobRef.Value.flowerDetails.Length; i++)
            {
                ref var item = ref blobRef.Value.flowerDetails[i];
                Debug.Log($"  ▶ <b>정의[{i}]</b> 품종: {item.species.ToString()} | 색상: {item.color.ToString()} | 꽃말: {item.floro.ToString()} / {item.floro2.ToString()}");
            }
            blobRef.Dispose();
        }
    }

    private void ReadUsableDetail(string path)
    {
        if (BlobAssetReference<UsableDetailBlobDatas>.TryRead(path, 1, out var blobRef))
        {
            Debug.Log($"<color=lightblue>[소모품 상세 정의 데이터] 파일: {Path.GetFileName(path)} (총 {blobRef.Value.usableDetails.Length}개)</color>");
            for (int i = 0; i < blobRef.Value.usableDetails.Length; i++)
            {
                ref var item = ref blobRef.Value.usableDetails[i];
                Debug.Log($"  ▶ <b>정의[{item.index}]</b> 지속시간: {item.duration}s | 파워: {item.power} | 차지시간: {item.chargeInfo.ChargeTime}s, 최대차지: {item.chargeInfo.maxChargeCount}");
            }
            blobRef.Dispose();
        }
    }
}
