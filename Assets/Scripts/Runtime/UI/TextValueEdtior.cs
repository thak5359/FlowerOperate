using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextValueEdtior : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI targetText;

    public void changeTextPuncPause(float value)
    {
        targetText.text = ((value+1)*0.05f).ToString("F2");
    }

    public void changeTextValueInt(float value)
    {
        targetText.text = (((int)value).ToString()+ "%");
    }


}
