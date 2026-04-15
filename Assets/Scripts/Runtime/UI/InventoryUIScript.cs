using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;

    private List<Button> buttons = new List<Button>();
    VisualElement root;

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



    }
    private void OnDisabled()
    {
        buttons.Clear();
    }

    public void CloseBTN()
    {

    }




}
