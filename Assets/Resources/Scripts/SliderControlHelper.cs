using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderControlHelper : MonoBehaviour
{

    public Slider targetSlider;

    public virtual void ChangeSliderValue(float num)
    {
        targetSlider.value += num;
    }


}

