using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TitleMenuManager : MonoBehaviour
{
    public Button startButton;
    public Button loadButton;
    public Button settingButton;
    public Button endButton;

    [SerializeField] protected SettingMenuManager SMM;

    
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
        SMM.showUI();
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
