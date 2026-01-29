using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatPanelUI : MonoBehaviour
{
    public static StatPanelUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject panelObject;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI fireRateText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Hide(); // Hide by default
    }

    public void Show()
    {
        panelObject.SetActive(true);
    }

    public void Hide()
    {
        panelObject.SetActive(false);
    }

    public void UpdateStats(string name, string hp, string speed, string dmg, string rate)
    {
        nameText.text = name;
        healthText.text = $"HP: {hp}";
        speedText.text = $"SPD: {speed}";
        damageText.text = $"DMG: {dmg}";
        fireRateText.text = $"ROF: {rate}";
    }
}