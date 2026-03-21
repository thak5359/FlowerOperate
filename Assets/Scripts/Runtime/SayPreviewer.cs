using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;

public class SayPreviewer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI previewText;

    [Header("Sample Content")]
    [TextArea(3, 10)]
    public List<string> firstMeetLines = new List<string>(); // 처음 켰을 때 
    [TextArea(3, 10)]
    public List<string> randomLines = new List<string>();    // 일반 랜덤 대사
    [TextArea(3, 10)]
    public List<string> hiddenLines = new List<string>();    // 히든 대사 (출력 후 카운트 초기화)

    [Header("Settings")]
    [Range(0f, 100f)]
    public float hiddenChance = 5f; // 히든 대사가 나올 확률 (5%)

    private bool isFirstMeet = true;
    private StringBuilder sb = new StringBuilder();
    private Coroutine typingCoroutine;

    private enum LineType { None, First, Random, Hidden }
    private LineType lastType = LineType.None;
    private int lastIndex = -1;

    private void OnEnable()
    {
        StopAllCoroutines();
        isFirstMeet = true; // 옵션창 켤 때마다 초기화
        StartCoroutine(WaitAndStart());
    }

    private IEnumerator WaitAndStart()
    {
        
        yield return null; // 1프레임 대기 (OptionManager.Instance가 생성될 시간을 줌)
        typingCoroutine = StartCoroutine(PlayPreview());
    }


    private IEnumerator PlayPreview()
    {
       
        // 리스트가 비어있을 경우를 대비한 방어 코드
        if (firstMeetLines.Count == 0 && randomLines.Count == 0)
        {
            previewText.text = "출력할 대사가 없습니다.";
            yield break;
        }

        while (true)
        {

            if (OptionManager.Instance == null)
            {
                yield return null;
                continue;
            }
            string line = SelectNextLine();

            sb.Clear();
            previewText.text = "";

            // 타이핑 효과 시작
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                sb.Append(c);
                previewText.text = sb.ToString();

                float speed = // OptionManager.Instance.settings.writingSpeed;
                    30;
                float puncPause = //(OptionManager.Instance.settings.puncSliderRaw + 1)*0.05f;
                    0.2f;
                float charDelay = 1f / Mathf.Max(speed, 1f);

                // 구두점 체크
                bool isPunctuation = IsPunctuation(c);

                yield return new WaitForSeconds(charDelay);

                if (isPunctuation)
                {
                    yield return new WaitForSeconds(puncPause);
                }
            }

            float finalWaitTime = 1.5f; // 기본값

            if (OptionManager.Instance != null //&& OptionManager.Instance.settings.isAutoMode == true)
                )
            {
                // 오토 모드라면 슬라이더로 설정한 값을 가져옴
               // finalWaitTime = OptionManager.Instance.settings.autoWaitTime;
            }

            // 문장 종료 후 대기 시간 (다음 문장 넘어가기 전)
            yield return new WaitForSeconds(finalWaitTime);


            float elapsed = 0f;
            while (true)
            {
                float currentTargetWait = 0.2f; //OptionManager.Instance.settings.autoWaitTime;
                   

                elapsed += Time.deltaTime;
                if (elapsed >= currentTargetWait) break;

                yield return null; // 매 프레임 체크하여 슬라이더 조절 시 즉각 반영되게 함
            }

        }
    }

    private string SelectNextLine()
    {
        LineType currentType = LineType.None;
        int currentIndex = -1;
        string selectedLine = "";

        // 1. 처음 만남 단계 (sayCount 0)
        if (isFirstMeet == true && firstMeetLines.Count > 0)
        {
            currentType = LineType.First;
            currentIndex = Random.Range(0, firstMeetLines.Count);
            selectedLine = firstMeetLines[currentIndex];
            isFirstMeet = false; // 다음부터는 일반 랜덤/히든 대사로 넘어감
        }
        else
        {
            float roll = Random.Range(0f, 100f);

            // 2. 히든 대사 결정
            if (hiddenLines.Count > 0 && roll <= hiddenChance)
            {
                currentType = LineType.Hidden;
                // 리스트가 2개 이상일 때만 중복 체크
                do
                {
                    currentIndex = Random.Range(0, hiddenLines.Count);
                } while (hiddenLines.Count > 1 && currentType == lastType && currentIndex == lastIndex);

                selectedLine = hiddenLines[currentIndex];
            }
            // 3. 일반 랜덤 대사 결정
            else if (randomLines.Count > 0)
            {
                currentType = LineType.Random;
                do
                {
                    currentIndex = Random.Range(0, randomLines.Count);
                } while (randomLines.Count > 1 && currentType == lastType && currentIndex == lastIndex);

                selectedLine = randomLines[currentIndex];
            }
            else
            {
                // 예외 처리
                selectedLine = firstMeetLines[Random.Range(0, firstMeetLines.Count)];
            }
        }

        // 마지막 상태 업데이트 (인덱스와 타입만 저장)
        lastType = currentType;
        lastIndex = currentIndex;

        return selectedLine;
    }
    private bool IsPunctuation(char c)
    {
        return c == '.' || c == ',' || c == '!' || c == '?' ||
               c == ':' || c == ';' || c == '·' || c == '♪' ||
               c == ')' || c == 'ㅤ';
    }
}