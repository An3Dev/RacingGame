using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class SliderText : MonoBehaviour
{
    Slider slider;
    TextMeshProUGUI text;
    private void Awake()
    {
        slider = transform.parent.GetComponentInChildren<Slider>();
        text = GetComponent<TextMeshProUGUI>();
        slider.onValueChanged.AddListener(delegate { OnSliderWasChanged(); });
        OnSliderWasChanged();
    }

    void OnSliderWasChanged()
    {
        text.text = slider.value.ToString();
    }

}
