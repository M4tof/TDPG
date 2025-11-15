using UnityEngine;
using TMPro;
public class ManaDisplay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    TextMeshProUGUI tmpText;
    void Start()
    {

        tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = $"Mana: {GameManager.Instance.RSInstance.mana.Value:F2}";

        }
        else
        {
            Debug.LogWarning($"TextMeshProUGUI component not found in children of {gameObject.name}. " +
                             "Ensure your object has a Text (TMP) child.", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = $"Mana: {GameManager.Instance.RSInstance.mana.Value:F2}";

        }
        else
        {
            Debug.LogWarning($"TextMeshProUGUI component not found in children of {gameObject.name}. " +
                             "Ensure your object has a Text (TMP) child.", this);
        }
    }
}
