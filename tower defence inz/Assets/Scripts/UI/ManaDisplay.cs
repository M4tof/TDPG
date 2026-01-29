using UnityEngine;
using TMPro;
public class ManaDisplay : MonoBehaviour
{
    TextMeshProUGUI tmpText;
    void Start()
    {

        tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = $"Mana: {ResourceSystem.Instance.mana.Value:F2}";

        }
        else
        {
            Debug.LogWarning($"TextMeshProUGUI component not found in children of {gameObject.name}. " +
                             "Ensure your object has a Text (TMP) child.", this);
        }
    }

    void Update()
    {
        tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = $"Mana: {ResourceSystem.Instance.mana.Value:F2}";

        }
        else
        {
            Debug.LogWarning($"TextMeshProUGUI component not found in children of {gameObject.name}. " +
                             "Ensure your object has a Text (TMP) child.", this);
        }
    }
}
