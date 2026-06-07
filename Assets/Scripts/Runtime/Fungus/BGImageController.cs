using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Fungus;

public class SaveSlotItem : MonoBehaviour
{
    public Image thumbnailImage;
    public Text sceneNameText;
    public Text blockNameText;
    public Text saveTimeText;

    public void UpdateUI(string slotKey)
    {
        var saveManager = FungusManager.Instance.SaveManager;
        string path = System.IO.Path.Combine(SaveManager.STORAGE_DIRECTORY, slotKey);

        if (saveManager.SaveDataExists(slotKey))
        {
            // 1. 썸네일 로드
            if (File.Exists(path + ".png"))
            {
                byte[] bytes = File.ReadAllBytes(path + ".png");
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                thumbnailImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }

            // 2. 데이터 정보 추출 (JSON 파싱을 통해 Scene/Block 정보 가져오기 가능)
            // 간단하게 구현하려면 세이브 시점에 별도의 메타데이터를 저장하거나 
            // SavePoint의 Description을 활용합니다.
            sceneNameText.text = "저장된 데이터 있음";
            saveTimeText.text = File.GetLastWriteTime(path + ".json").ToString("yyyy-MM-dd HH:mm");
        }
        else
        {
            thumbnailImage.sprite = null; // 빈 슬롯 표시
            sceneNameText.text = "Empty Slot";
            blockNameText.text = "-";
            saveTimeText.text = "-";
        }
    }
}