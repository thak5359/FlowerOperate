// 수정 위치: 테스트 드롭도 비동기 아이템 로드 완료 후 생성해요.
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class ItemGenTest : MonoBehaviour
{
    [Inject] private ItemManager itemManager;
    [Inject] private PlayerOwnItemDataManager _inven;
    private readonly int _hash = Animator.StringToHash("isOpen");
    public void OnClick()
    {
        ItemFactory.CreateItemPrefabAsync(201001, 1, new Vector3(1, 1, 20)).Forget(); //낡은 괭이
        ItemFactory.CreateItemPrefabAsync(201009, 1, new Vector3(1, 1, 20)).Forget(); // 낡은 물뿌리개
        ItemFactory.CreateItemPrefabAsync(201017, 1, new Vector3(1, 1, 20)).Forget(); // 낡은 망치
        ItemFactory.CreateItemPrefabAsync(201025, 1, new Vector3(1, 1, 20)).Forget(); // 낡은 낫
        ItemFactory.CreateItemPrefabAsync(201033, 1, new Vector3(1, 1, 20)).Forget(); // 낡은 도끼
        this.gameObject.SetActive(false);
    }
}
