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
        ItemFactory.CreateItemPrefab(itemManager.CreateItem(201001, 1), new Vector3(1, 1, 20)); //낡은 괭이
        ItemFactory.CreateItemPrefab(itemManager.CreateItem(201009, 1), new Vector3(1, 1, 20)); // 낡은 물뿌리개
        ItemFactory.CreateItemPrefab(itemManager.CreateItem(201017, 1), new Vector3(1, 1, 20)); // 낡은 망치
        ItemFactory.CreateItemPrefab(itemManager.CreateItem(201025, 1), new Vector3(1, 1, 20)); // 낡은 낫
        ItemFactory.CreateItemPrefab(itemManager.CreateItem(201033, 1), new Vector3(1, 1, 20)); // 낡은 도끼
        this.gameObject.SetActive(false);
    }
}