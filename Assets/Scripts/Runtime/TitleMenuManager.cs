using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class TitleMenuManager : MonoBehaviour
{
    public Button startButton;
    public Button loadButton;
    public Button settingButton;
    public Button endButton;
    protected SettingMenuManager _settingMenuManger;
    
    
    
    [Inject] 
    void Construct(SettingMenuManager input_settingMenuManager)
    {
        _settingMenuManger = input_settingMenuManager;
        Debug.Log($"세팅 메뉴의 할당 값은 {_settingMenuManger == null} 입니다");
    }

    void Start()
    {
        startButton.onClick.AddListener(() => OnClickStartButton());
        loadButton.onClick.AddListener(() =>  OnClickLoadButton());
        settingButton.onClick.AddListener(() => OnClickSettingButton());
        endButton.onClick.AddListener(() => OnClickGameEndButton());
    }
    private void OnClickStartButton()
    {
        SceneLoader.LoadScene("SampleScene", null);
    }
    private void OnClickLoadButton()
    {
        Debug.LogAssertion("아무것도 없어요");
    }
    private void OnClickSettingButton()
    {
        _settingMenuManger.OnClickSettingOpen();
    }
    private void OnClickGameEndButton()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
