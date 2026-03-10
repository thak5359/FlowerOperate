using Fungus;
using System.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class ShowState : MonoBehaviour
{
    // Start is called before the first frame update

    [Serializable]
    public struct StateData
    {
        public string name;
        [TextArea(2,5)]
        public string description;
        public Sprite icon;
        public Toggle Toggle;
    }

    [Header("State Data List")]
    public List<StateData> stateList;

    [Header("UI Referenece")]
    public Text stateNameText;
    public Text stateDescText;
    public Image stateIconImage;



    public void ChangeState(int index)
    {
        // 리스트 범위를 벗어나는지 안전 검사
        if (index < 0 || index >= stateList.Count)
        {
            Debug.LogWarning($"Index {index} 가 리스트 범위를 벗어났습니다!");
            return;
        }

        StateData data = stateList[index];

        // UI 업데이트
        if (stateNameText != null) stateNameText.text = data.name;
        if (stateDescText != null) stateDescText.text = data.description;
        if (stateIconImage != null && data.icon != null)
        {
            stateIconImage.sprite = data.icon;
        }
    }

    private void Start()
    {
        for (int i = 0; i < stateList.Count; i++)
        {
            int index = i; 
            if (stateList[i].Toggle != null)
            {
                stateList[i].Toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        ChangeState(index);
                    }
                });

                if (stateList[i].Toggle.isOn)
                {
                    ChangeState(index);
                }
            }
        }
    }

}
