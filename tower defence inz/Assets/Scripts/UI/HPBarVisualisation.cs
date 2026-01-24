using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HPBarVisualisation : MonoBehaviour
{
    private float minValue = 0;
    private float maxValue = 100;
    private Slider slider;
    void Start()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
        slider.minValue = 0;
    }

    public void Init(float value)
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
        SetMaxValue(value);
        SetValue(value);
        ShowBar(false);
    }

    public void SetMaxValue(float value)
    {
        maxValue = value;
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
        slider.maxValue = maxValue;
    }

    public void SetValue(float value)
    {
        slider.value = value;
        if (value < maxValue)
        {
            ShowBar(true);
        }
    }

    private void ShowBar(bool show)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(show);
        }
    }
}
