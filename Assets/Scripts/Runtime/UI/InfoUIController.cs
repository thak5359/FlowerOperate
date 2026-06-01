using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit 사용을 위해 필수
using System;
using VContainer; // DateTime 사용을 위해 필요

public class InfoUIController : MonoBehaviour
{
    private UIDocument _uiDocument;
    [Inject] PlayerOwnItemDataManager _ownItemManager;

    // UXML에서 설정한 Label 요소들을 담을 변수
    private Label _yearLabel;
    private Label _dateLabel;
    private Label _dayOfWeekLabel;
    private Label _timeLabel;
    private Label _moneyLabel;


    private void Awake()
    {
        // 1. UIDocument 컴포넌트 가져오기
        _uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {

        // 2. rootVisualElement 가져오기 (모든 UI 요소의 최상위 부모)
        var root = _uiDocument.rootVisualElement;

        // 3. Q<T>("이름") 메서드로 UXML에 정의된 요소 찾기
        _yearLabel = root.Q<Label>("YearLabel");
        _dateLabel = root.Q<Label>("DateLabelDay");
        _dayOfWeekLabel = root.Q<Label>("DayOfWeekLabel");
        _timeLabel = root.Q<Label>("TimeLabel");
        _moneyLabel = root.Q<Label>("MoneyLabel");
        // 초기값 설정
        UpdateDateTime();
    }

    private void UpdateDateTime()
    {
        if (_yearLabel == null || _dateLabel == null)
        {
            Debug.Log("레이블이 존재하지 않음");
            return;
        }

        // 4. 데이터 적용 (text 속성 변경)
        _yearLabel.text = $"{ProgressManager.getYear()}년";
        _dateLabel.text = $"{ProgressManager.getMonth():D2}월 {ProgressManager.getDay():D2}일";
        
        //요일과 시간 정보도 추가로 업데이트 가능합니다.
        if (_dayOfWeekLabel != null)
            _dayOfWeekLabel.text = GetKoreanDayOfWeek(ProgressManager.getDay() % 7);
        if(_moneyLabel != null)
            _moneyLabel.text = _ownItemManager.GetData.GetMoney.ToString();
        //if (_timeLabel != null)
        //    _timeLabel.text = now.ToString("HH:mm");
    }


    public void UpdateDateTime(string Year)
    {

    }

    private string GetKoreanDayOfWeek(int day)
    {
        return day switch
        {
            0 => "일요일",
            1 => "월요일",
            2 => "화요일",
            3 => "수요일",
            4 => "목요일",
            5 => "금요일",
            6 => "토요일",
            _ => ""
        };
    }
}