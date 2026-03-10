using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using Fungus;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MultiSaveController : MonoBehaviour
{

    private bool isSaveMode = false; // 현재 세이브 모드인지 로드 모드인지 구분
    private static int SLOT_PER_PAGE = 8; // 페이지당 슬롯 수

    [Header("UI Panels")]
    public CanvasGroup KioskUI;          // 숨겨야 할 기존 SaveMenu UI
    public GameObject SavePanel;        // 세이브/로드 UI 파츠
    public GameObject OptionPanel;      // 옵션 UI 파츠

    public Text PageText;

    public RectTransform mainUIPanel; // 이동시킬 최상위 부모 패널
    public Vector2 showPos;          // 화면 안 위치 (예: 0,525)
    public Vector2 hidePos;          // 화면 밖 위치 (예: 0, -525)
    public float duration = 0.5f;    // 이동 시간

    [Header("Slot References")]
    public SaveSlotItem[] slotItems; // 8개의 슬롯 객체를 담는 배열

    


    // 1. 세이브/로드 버튼을 눌렀을 때 호출
    public void OpenSavePanel(bool Toggle)
    {
        if (Toggle == true)
        {
            if (SavePanel.activeSelf == false) // 세이브/로드 패널 활성화
            { SavePanel.SetActive(true); }


            if (OptionPanel.activeSelf == true) // 옵션 패널 비활성화
            { OptionPanel.SetActive(false); }

            isSaveMode = true; // 세이브 모드로 설정

            RefreshAllSlots(); // 슬롯 내용 갱신
        }
    }

    public void OpenLoadPanel(bool Toggle)
    {
        if (Toggle)
        {
            if (SavePanel.activeSelf == false) // 세이브/로드 패널 활성화
            { SavePanel.SetActive(true); }


            if (OptionPanel.activeSelf == true) // 옵션 패널 비활성화
            { OptionPanel.SetActive(false); }


            isSaveMode = false; 
            RefreshAllSlots();

            Debug.Log($" 로드모드 활성화 : {isSaveMode}");
        }
    }

    public void OpenOptionPanel(bool Toggle)
    {
        if (Toggle == true)
        {
            if (OptionPanel.activeSelf == false) // 옵션 패널 활성화
            { OptionPanel.SetActive(true); }

            if (SavePanel.activeSelf == true) // 세이브/로드 패널 비활성화
            { SavePanel.SetActive(false); }
        }
        else { OptionPanel.SetActive(false); }
    }


    private Coroutine moveCoroutine;

    //키오스크 숨기기/보이기 (이동 연출 포함) 
    public void ShowKiosk(bool Toggle)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        if (Toggle == true)
        {
            moveCoroutine = StartCoroutine(MoveRoutine(showPos));
            // 켜질 때 슬롯 갱신 등 기존 로직 수행
            RefreshAllSlots();
        }
        else
        {

            moveCoroutine = StartCoroutine(MoveRoutine(hidePos));
        }

    }

    private IEnumerator MoveRoutine(Vector2 targetPos)
    {
        Vector2 startPos = mainUIPanel.anchoredPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // 부드러운 가감속을 위해 SmoothStep 적용
            t = t * t * (3f - 2f * t);

            mainUIPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        mainUIPanel.anchoredPosition = targetPos;
        if (targetPos == hidePos && OptionPanel.activeSelf == true)
        {
           OptionPanel.SetActive(false);
        }

    }

    // 모든 슬롯의 텍스트와 썸네일 업데이트
    public void RefreshAllSlots()
    {
        if (slotItems == null || slotItems.Length == 0)
        {
            Debug.LogWarning("SlotItems 배열이 할당되지 않았거나 비어 있습니다.");
            return;
        }
        int currentPage = int.Parse(PageText.text);

        for (int i = 0; i < slotItems.Length; i++)
        {

            int slotID = (currentPage - 1) * SLOT_PER_PAGE + (i + 1);
            string slotKey = "Slot_" + slotID;

            slotItems[i].UpdateUI(slotKey);
        }
    }

    // 슬롯 클릭 시 호출 (인스펙터에서 1~10번 설정)
    public void OnSlotClicked(int slotNumber)
    {
        if (PageText == null)
        {
            Debug.LogError("PageText가 할당되지 않았습니다.");
            return;
        }


        int currentPage = int.Parse(PageText.text);
        int slotID = (currentPage - 1) * SLOT_PER_PAGE + slotNumber;
        string slotKey = "Slot_" + slotID;

        Debug.Log($"슬롯 클릭됨: {slotKey}, 세이브 모드: {isSaveMode}");

        if (isSaveMode)
        { StartCoroutine(CaptureAndSave(slotKey)); }
        else
            LoadGameLogic(slotKey);
    }


    public void PrevSaveSlots() // 이전 세이브 페이지로
    {
        Debug.Log("Prev 버튼 클릭됨!"); // 이 로그가 콘솔에 찍히는지 확인하세요.
        int CurPageNum = int.Parse(PageText.text);

        if (CurPageNum == 1)
            return;
        else
        {
            PageText.text = (CurPageNum - 1).ToString();
            RefreshAllSlots();
        }
    }

    public void NextSaveSlots() // 다음 세이브 페이지로
    {
        int CurPageNum = int.Parse(PageText.text);

        if (CurPageNum == 8)
            return;
        else
        {
            PageText.text = (CurPageNum + 1).ToString();
            RefreshAllSlots();
        }
    }





    private void LoadGameLogic(string slotKey)
    {
        string savedScene = PlayerPrefs.GetString(slotKey + "_SceneName");
        if (string.IsNullOrEmpty(savedScene)) return;

        // 중복 호출 제거하고 한 번만 실행
        FungusManager.Instance.SaveManager.Load(slotKey);

        // 이동 연출과 함께 닫기
        ShowKiosk(false);
    }

    //  화면 캡처 및 저장 로직
    private IEnumerator CaptureAndSave(string slotKey)
    {
        // UI 숨기기 (캡처에 포함되지 않도록)
        SavePanel.SetActive(false);
        if (KioskUI != null) KioskUI.alpha = 0;

        yield return new WaitForEndOfFrame();


        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(slotKey + "_SceneName", currentScene); // 현재 씬 이름 저장

        // 스크린샷 캡처
        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        // PNG 파일 저장
        byte[] bytes = ss.EncodeToPNG();
        string path = SaveManager.STORAGE_DIRECTORY + slotKey + ".png";
        if (!Directory.Exists(SaveManager.STORAGE_DIRECTORY)) Directory.CreateDirectory(SaveManager.STORAGE_DIRECTORY);
        File.WriteAllBytes(path, bytes);

        Destroy(ss);

        // UI 다시 보이기
        if (KioskUI != null) KioskUI.alpha = 1;
        SavePanel.SetActive(true);

        // Fungus 세이브 실행
        FungusManager.Instance.SaveManager.Save(slotKey);

        // UI 즉시 갱신
        RefreshAllSlots();
    }




    private void Start()
    {
        PageText.text = "1";
        RefreshAllSlots();
    }
}