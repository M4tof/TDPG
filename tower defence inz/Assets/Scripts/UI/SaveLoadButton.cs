using UnityEngine;
using TMPro;

public class SaveLoadButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TextMeshProUGUI tmpText = GetComponentInChildren<TextMeshProUGUI>();

        if (tmpText != null)
        {
            tmpText.text = $"{_slotType} slot {_slotNumber}";

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

    [SerializeField] private string _slotType;

    public string SavePath => _savePath;
    public int SlotNumber => _slotNumber;

    public string SlotType => _slotType;
    public void SetData(string path, int number, string type)
    {
        _savePath = path;
        _slotNumber = number;
        _slotType = type;
    }

}
