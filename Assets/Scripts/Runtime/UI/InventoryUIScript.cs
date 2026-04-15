using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;

    private List<Button> buttons = new List<Button>();
    VisualElement root;

    private Button closeButton;

    private void Awake()
    {

        if (_uiDocument != null)
            _uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnabled()
    {
        root = _uiDocument.rootVisualElement;

        buttons.Clear();

        buttons = root.Query<Button>("slot-button").ToList(); // 리스트에 집어넣기

        closeButton = root.Query<Button>("CloseButton");
        closeButton.clicked += closeInventory;

    }
    private void OnDisabled()
    {
        buttons.Clear();
    }

    public void closeInventory()
    {
        root.visible = false;
    }

    public void openInventory()
    {
        root.visible = true;
    }

}
