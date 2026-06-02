using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit 사용을 위해 필수
using System;
using VContainer;
using R3; // DateTime 사용을 위해 필요

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
        if (_moneyLabel == null)
        {
            Debug.LogError("[InfoUIController] MoneyLabel을 UXML에서 찾을 수 없습니다!");
        }
        else
        {
            Debug.Log("[InfoUIController] MoneyLabel을 성공적으로 찾았습니다.");
        }
        // 초기값 설정
        UpdateDateTime();

        // 이벤트 구독
        _ownItemManager.InventoryRevisionChanged.Subscribe(_ => UpdateMoney()).AddTo(this);
    }

    private void UpdateDateTime()
    {
        if (_yearLabel == null || _dateLabel == null)
        {
            Debug.LogError("[InfoUIController] YearLabel 또는 DateLabelDay가 존재하지 않습니다.");
            return;
        }

        // 4. 데이터 적용 (text 속성 변경)
        _yearLabel.text = $"{ProgressManager.getYear()}년";
        _dateLabel.text = $"{ProgressManager.getMonth():D2}월 {ProgressManager.getDay():D2}일";
        
        //요일과 시간 정보도 추가로 업데이트 가능합니다.
        if (_dayOfWeekLabel != null)
            _dayOfWeekLabel.text = GetKoreanDayOfWeek(ProgressManager.getDay() % 7);
        UpdateMoney();
        //if (_timeLabel != null)
        //    _timeLabel.text = now.ToString("HH:mm");
    }

    private void UpdateMoney()
    {
        if (_moneyLabel != null)
        {
            int currentMoney = _ownItemManager.GetData.GetMoney;
            _moneyLabel.text = currentMoney.ToString() + "$";
            Debug.Log($"[InfoUIController] MoneyLabel 텍스트를 '{_moneyLabel.text}'로 설정했습니다. (보유 금액: {currentMoney})");
        }
        else
        {
            Debug.LogWarning("[InfoUIController] UpdateMoney가 호출되었으나 _moneyLabel이 null입니다.");
        }
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