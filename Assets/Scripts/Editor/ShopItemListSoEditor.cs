using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;

[CustomEditor(typeof(ShopItemListSO))]
public class ShopItemListSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 UI 그리기
        base.OnInspectorGUI();

        ShopItemListSO so = (ShopItemListSO)target;

        GUILayout.Space(20);

        // CSV 불러오기 버튼 생성
        if (GUILayout.Button("CSV 데이터 불러오기 (씨앗 상점)", GUILayout.Height(30)))
        {
            string path = EditorUtility.OpenFilePanel("CSV 파일 선택", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                LoadCSVData(so, path);
            }
        }
    }

    private void LoadCSVData(ShopItemListSO so, string path)
    {
        string[] lines = File.ReadAllLines(path);
        List<ProductData> dataList = new List<ProductData>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');

            // 파트너가 주신 CSV 구조 확인: 
            // values[3] = ProductNo, values[4] = Name, values[5] = Cost, values[6] = Date
            if (values.Length >= 7 && int.TryParse(values[3], out int productNO))
            {
                string productName = values[4];
                if (int.TryParse(values[5], out int cost))
                {
                    string dateString = values[6];
                    int unlockDay = ConvertDateToDay(dateString);

                    ProductData newData = new ProductData(productNO, productName, cost, unlockDay);
                    dataList.Add(newData);
                }
            }
        }

        // ShopItemListSO의 private 배열에 리플렉션으로 데이터 할당
        FieldInfo field = typeof(ShopItemListSO).GetField("products", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(so, dataList.ToArray());

            // 변경 사항을 Unity에 알리고 저장
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
            Debug.Log($"[상점 데이터 갱신 완료] 총 {dataList.Count}개의 씨앗 데이터가 성공적으로 등록되었어요!");
        }
        else
        {
            Debug.LogError("ShopItemListSO에서 'products' 필드를 찾을 수 없어요. 변수명을 확인해 주세요.");
        }
    }

    // ProgressManager의 역산 알고리즘 적용
    private int ConvertDateToDay(string dateStr)
    {
        try
        {
            // "2월 8일" 형식에서 숫자만 추출
            string[] parts = dateStr.Split(new char[] { '월', '일' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                int month = int.Parse(parts[0].Trim());
                int day = int.Parse(parts[1].Trim());

                // 역산 식: Total Day = (Month - 1) * 28 + DayInMonth
                return (month - 1) * 28 + day;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"날짜 파싱 중 오류가 발생했어요: {dateStr} -> {e.Message}");
        }

        // 파싱 실패 시 기본값 1 반환
        return 1;
    }
}