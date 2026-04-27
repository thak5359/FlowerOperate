using Cysharp.Threading.Tasks;
using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class TitleMenuController : MonoBehaviour
{
    public Button startButton;
    public Button loadButton;
    public Button settingButton;
    public Button endButton;
    protected TitleSettingMenuController _settingMenuManger;
    
    
    
    [Inject] 
    void Construct(TitleSettingMenuController input_settingMenuManager)
    {
        _settingMenuManger = input_settingMenuManager;
        if(_settingMenuManger == null)
            Debug.LogAssertion("SettingMenuManager이 할당되지 않았습니다.");
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
        _settingMenuManger.OpenSettingMenu().Forget();
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
