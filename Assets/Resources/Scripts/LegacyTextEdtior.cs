using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LegacyTextEdtior : MonoBehaviour
{
    // Start is called before the first frame update
    public Text targetText;

    public void changeTextPuncPause(float value)
    {
        targetText.text = ((value+1)*0.05f).ToString("F2");
    }

    public void changeTextValueInt(float value)
    {
        targetText.text = ((int)value).ToString();
    }


}
