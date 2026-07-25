// 수정 위치: 에디터 테스트 아이템도 비동기 로드 완료 후 추가해요.
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// VContainer 의존성 주입을 사용하는 에디터 윈도우 예제입니다.
/// </summary>
public class ItemDataDebugWindow : EditorWindow
{
    // [Inject] 속성을 통해 주입받을 필드를 정의합니다.
    [Inject] private PlayerOwnItemDataManager _itemDataManager;

    private int _testItemId = 1;
    private int _testItemCount = 10;

    [MenuItem("Tools/Item Data Debug Window")]
    public static void ShowWindow()
    {
        var window = GetWindow<ItemDataDebugWindow>();
        window.titleContent = new GUIContent("Item Debugger");
    }

    private void OnGUI()
    {
        GUILayout.Label("VContainer Dependency Injection Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("플레이 모드에서만 의존성 주입이 가능합니다.", MessageType.Info);
            return;
        }

        // 주입 버튼
        if (GUILayout.Button("의존성 주입 받기 (Inject)", GUILayout.Height(30)))
        {
            TryInject();
        }

        EditorGUILayout.Space(10);

        // 주입된 데이터 표시
        if (_itemDataManager != null)
        {
            DrawItemDataInfo();
        }
        else
        {
            EditorGUILayout.HelpBox("아직 의존성이 주입되지 않았습니다. 위 버튼을 눌러주세요.", MessageType.Warning);
        }
    }

    private void TryInject()
    {
        // 1. 현재 씬에서 활성화된 LifetimeScope를 찾습니다.
        var scope = FindObjectOfType<LifetimeScope>();

        if (scope != null && scope.Container != null)
        {
            // 2. 해당 컨테이너를 통해 현재 Window 객체에 주입(Field Injection)을 실행합니다.
            scope.Container.Inject(this);
            Debug.Log("<color=green>[VContainer]</color> ItemDataDebugWindow: 의존성 주입 성공!");
        }
        else
        {
            Debug.LogError("[VContainer] 활성화된 LifetimeScope 또는 Container를 찾을 수 없습니다.");
        }
    }

    private void DrawItemDataInfo()
    {
        ref var data = ref _itemDataManager.GetData;

        EditorGUILayout.BeginVertical("box");
        {
            GUILayout.Label("Player Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Money", data.GetMoney.ToString() + " G");
            EditorGUILayout.LabelField("Reputation", data.GetReputation.ToString());
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Money +1000 G"))
            {
                data.AddMoney(1000);
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // --- Selling Box Test Section ---
        EditorGUILayout.BeginVertical("box");
        {
            GUILayout.Label("Selling Box Test", EditorStyles.boldLabel);
            _testItemId = EditorGUILayout.IntField("Test Item ID", _testItemId);
            _testItemCount = EditorGUILayout.IntField("Test Item Count", _testItemCount);

            if (GUILayout.Button("Add Item to Selling Box", GUILayout.Height(25)))
            {
                // PlayerOwnItemDataManager를 통해 아이템 추가
                AddTestItemAsync().Forget();
                Debug.Log($"<color=cyan>[Debugger]</color> SellingBox에 아이템 추가: ID {_testItemId} (x{_testItemCount})");
            }

            EditorGUILayout.Space(5);
            GUILayout.Label("Current Selling Box Items:", EditorStyles.miniLabel);
            var sellingItems = data.GetItemList(ContainerType.SELLING);
            if (sellingItems != null)
            {
                foreach (var item in sellingItems)
                {
                    if (item != null && item.Id > 0)
                    {
                        EditorGUILayout.LabelField($"- ID: {item.Id} (x{item.Count})");
                    }
                }
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        GUILayout.Label("Inventory Quick View (Top 5 Slots)", EditorStyles.boldLabel);
        var inven = data.GetItemList(ContainerType.INVENTORY);
        if (inven != null)
        {
            int displayCount = Mathf.Min(5, inven.Count);
            for (int i = 0; i < displayCount; i++)
            {
                var item = inven[i];
                string itemName = (item != null && item.Id > 0) ? $"ID: {item.Id} (x{item.Count})" : "Empty";
                EditorGUILayout.LabelField($"Slot {i}", itemName);
            }
        }
    }

    // 수정 위치: 판매 상자에 완전히 로드된 테스트 아이템만 추가해요.
    private async UniTask AddTestItemAsync()
    {
        GameItem item = await ItemFactory.CreateItemAsync(_testItemId, _testItemCount);
        if (item != null)
            _itemDataManager.AddItem(ContainerType.SELLING, item);
    }
}
