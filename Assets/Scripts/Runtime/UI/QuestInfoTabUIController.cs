using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using VContainer;
using R3;

public class QuestInfoTabUIController : MonoBehaviour
{
    [Header("UI Document Handle")]
    [SerializeField] private UIDocument _uiDocument;

    [Header("Templates")]
    [SerializeField] private VisualTreeAsset _questContentTemplate;
    [SerializeField] private VisualTreeAsset _questObjectiveTemplate;

    [Inject] private QuestManager _questManager;

    private VisualElement _root;
    private ScrollView _scrollView;
    private readonly CompositeDisposable _disposables = new();
    private readonly List<IDisposable> _runtimeDisposables = new();

    private void Awake()
    {
        if (_uiDocument == null)
            _uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (_uiDocument == null) return;
        _root = _uiDocument.rootVisualElement;

        _scrollView = _root.Q<ScrollView>();
        if (_scrollView == null)
        {
            _scrollView = _root.Q<ScrollView>(className: "unity-scroll-view");
        }

        if (_questManager != null)
        {
            // 진행 중인 퀘스트 리스트 변경 구독
            _questManager.OnQuestListChanged
                .Subscribe(_ => RefreshQuestList())
                .AddTo(_disposables);

            RefreshQuestList();
        }
    }

    private void OnDisable()
    {
        ClearRuntimeBindings();
        _disposables.Clear();
    }

    private void ClearRuntimeBindings()
    {
        foreach (var disposable in _runtimeDisposables)
        {
            disposable.Dispose();
        }
        _runtimeDisposables.Clear();
    }

    private void RefreshQuestList()
    {
        ClearRuntimeBindings();
        _scrollView.Clear();

        var progressingQuests = _questManager.ProgressingQuests;
        if (progressingQuests == null || progressingQuests.Count == 0)
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
            return;
        }

        if (_root != null)
        {
            _root.style.display = DisplayStyle.Flex;
        }

        foreach (var quest in progressingQuests)
        {
            if (quest == null) continue;

            // 1. 퀘스트 영역 생성
            VisualElement questContentElem = _questContentTemplate.Instantiate();
            Label titleLabel = questContentElem.Q<Label>("QuestTitleLabel");

            // 2. 퀘스트 제목 할당
            QuestContent questContent = default;
            if (_questManager.TryGetQuestContent(quest.QuestID, out questContent))
            {
                if (titleLabel != null) titleLabel.text = questContent.QuestTitle;
            }
            else
            {
                if (titleLabel != null) titleLabel.text = $"퀘스트 {quest.QuestID}";
            }

            // 3. 디폴트로 템플릿에 들어있는 정적 인스턴스 제거 및 컨테이너 획득
            VisualElement defaultObjective = questContentElem.Q<VisualElement>("QuestObjective");
            VisualElement container = titleLabel != null ? titleLabel.parent : questContentElem;

            if (defaultObjective != null && defaultObjective.parent != null)
            {
                defaultObjective.parent.Remove(defaultObjective);
            }

            // 4. 개별 목표 생성 및 바인딩
            if (quest.QuestObjectives != null)
            {
                for (int i = 0; i < quest.QuestObjectives.Length; i++)
                {
                    var objectiveProgress = quest.QuestObjectives[i];
                    if (objectiveProgress == null) continue;

                    // 해당 인덱스의 베이스 데이터 매칭
                    QuestObjective baseObjective = default;
                    if (questContent.QuestObjectives != null && i < questContent.QuestObjectives.Length)
                    {
                        baseObjective = questContent.QuestObjectives[i];
                    }

                    VisualElement objectiveElem = _questObjectiveTemplate.Instantiate();
                    Label descLabel = objectiveElem.Q<Label>("QuestDescriptionLabel");
                    Label progressLabel = objectiveElem.Q<Label>("QuestProgressStringLabel");

                    // 수동 갱신 및 구독 바인딩
                    var binding = new QuestObjectiveBinding(descLabel, progressLabel, objectiveProgress, baseObjective);
                    _runtimeDisposables.Add(binding);

                    if (container != null)
                    {
                        container.Add(objectiveElem);
                    }
                    else
                    {
                        questContentElem.Add(objectiveElem);
                    }
                }
            }

            _scrollView.Add(questContentElem);
        }
    }
}

/// <summary>
/// 개별 퀘스트 목표의 상태 변화를 UI에 매핑해주는 바인딩 헬퍼 클래스
/// </summary>
public class QuestObjectiveBinding : IDisposable
{
    private readonly Label _descLabel;
    private readonly Label _progressLabel;
    private readonly QuestObjectiveInProgress _objectiveProgress;
    private readonly IDisposable _subscription;

    public QuestObjectiveBinding(Label descLabel, Label progressLabel, QuestObjectiveInProgress objectiveProgress, QuestObjective baseObjective)
    {
        _descLabel = descLabel;
        _progressLabel = progressLabel;
        _objectiveProgress = objectiveProgress;

        // 1. 목표 종류에 따른 한글 문구 바인딩
        if (_descLabel != null)
        {
            _descLabel.text = GetDescriptionKorean(baseObjective);
        }

        // 2. 실시간 진척도 텍스트 갱신 설정
        UpdateProgressText();
        _subscription = _objectiveProgress.OnProgressChanged.Subscribe(_ => UpdateProgressText());
    }

    private void UpdateProgressText()
    {
        if (_progressLabel != null && _objectiveProgress != null)
        {
            _progressLabel.text = _objectiveProgress.ProgressString;
        }
    }

    private string GetDescriptionKorean(QuestObjective baseObjective)
    {
        string itemName = "";
        if (baseObjective.TargetID != 0)
        {
            try
            {
                itemName = GlobalItemDB.GetItemName(baseObjective.TargetID).ToString();
            }
            catch
            {
                itemName = $"아이템 {baseObjective.TargetID}";
            }
        }

        switch (baseObjective.ContentType)
        {
            case QuestContentType.Chat:
                return "NPC와 대화하기";
            case QuestContentType.OwnItemSpecific:
                return string.IsNullOrEmpty(itemName) ? "아이템 보유하기" : $"{itemName} 보유하기";
            case QuestContentType.SubmissionItem:
                return string.IsNullOrEmpty(itemName) ? "아이템 납품하기" : $"{itemName} 납품하기";
            case QuestContentType.SellAnything:
                return "아이템 판매하기";
            case QuestContentType.SellItem:
                return string.IsNullOrEmpty(itemName) ? "아이템 판매하기" : $"{itemName} 판매하기";
            case QuestContentType.BuyAnything:
                return "아이템 구매하기";
            case QuestContentType.BuyItem:
                return string.IsNullOrEmpty(itemName) ? "아이템 구매하기" : $"{itemName} 구매하기";
            case QuestContentType.PlotSowing:
                return string.IsNullOrEmpty(itemName) ? "씨앗 심기" : $"{itemName} 씨앗 심기";
            case QuestContentType.PlotWatering:
                return string.IsNullOrEmpty(itemName) ? "밭에 물 주기" : $"{itemName}에 물 주기";
            case QuestContentType.PlotHammeringPlot:
                return "해머로 밭 다듬기";
            case QuestContentType.PlotHammeringFlower:
                return "해머로 꽃 다듬기";
            case QuestContentType.PlotFertilizer:
                return "밭에 비료 주기";
            case QuestContentType.PlotBountfyFertilizer:
                return "밭에 성장 비료 주기";
            case QuestContentType.PlotQualityFertilizer:
                return "밭에 품질 비료 주기";
            case QuestContentType.PlotReaping:
                return string.IsNullOrEmpty(itemName) ? "작물 수확하기" : $"{itemName} 수확하기";
            case QuestContentType.PlotReapingSpecific:
                return string.IsNullOrEmpty(itemName) ? "특정 작물 수확하기" : $"{itemName} 수확하기";
            case QuestContentType.OreRuiningAnything:
                return "광석 캐기";
            case QuestContentType.OreRuiningSpecific:
                return string.IsNullOrEmpty(itemName) ? "광석 캐기" : $"{itemName} 광석 캐기";
            case QuestContentType.TreeRuiningAnything:
                return "벌목하기";
            case QuestContentType.TreeRuiningSpecific:
                return string.IsNullOrEmpty(itemName) ? "벌목하기" : $"{itemName} 벌목하기";
            case QuestContentType.GrassRuiningAnything:
                return "풀 베기";
            case QuestContentType.GrassRuininingSpecific:
                return string.IsNullOrEmpty(itemName) ? "풀 베기" : $"{itemName} 풀 베기";
            case QuestContentType.PlowPlot:
                return "밭 갈기";
            case QuestContentType.BloomAnything:
                return "꽃 피우기";
            case QuestContentType.BloomFlower:
                return string.IsNullOrEmpty(itemName) ? "꽃 피우기" : $"{itemName} 꽃 피우기";
            case QuestContentType.Unknown:
            default:
                return "퀘스트 목표 달성";
        }
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}
