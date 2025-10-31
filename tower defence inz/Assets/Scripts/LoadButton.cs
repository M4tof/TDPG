using UnityEngine;
using TMPro;

public class LoadButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TextMeshProUGUI tmpText = GetComponentInChildren<TextMeshProUGUI>();

        if (tmpText != null)
        {
            tmpText.text = $"Load slot {_slotNumber}";

        }
        else
        {
            Debug.LogWarning($"TextMeshProUGUI component not found in children of {gameObject.name}. " +
                             "Ensure your button has a Text (TMP) child.", this);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    [SerializeField] private string _savePath;
    [SerializeField] private int _slotNumber;

    public string SavePath => _savePath;
    public int SlotNumber => _slotNumber;

    public void SetData(string path, int number)
    {
        _savePath = path;
        _slotNumber = number;
    }

}
