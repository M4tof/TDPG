using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming TextMeshPro for inputs
using TDPG.Templates.Grid.MapGen;
using System;
using System.Collections.Generic;
public class NewGameMenu : MonoBehaviour
{
    [Header("UI Inputs")]
    [SerializeField] private TMP_Dropdown mapTypeDropdown;
    [SerializeField] private TMP_InputField widthInput;
    [SerializeField] private TMP_InputField heightInput;
    [SerializeField] private TMP_InputField spawnersInput;
    [SerializeField] private TMP_InputField waterLevelInput;
    [SerializeField] private TMP_InputField wallLevelInput;
    [SerializeField] private TMP_InputField minimalDistanceInput;
    [SerializeField] private Toggle canSwimToggle;
    [SerializeField] private TMP_InputField emptyCellsInput;

    [Header("Configuration")]
    [SerializeField] private int selectedSlot = 1;

    void Start()
    {
        // Auto-fill Enum Dropdown
        if (mapTypeDropdown != null)
        {
            mapTypeDropdown.ClearOptions();
            var options = new List<string>(Enum.GetNames(typeof(MapTypes)));
            mapTypeDropdown.AddOptions(options);
        }
    }

    // Call this from your "Start Game" Button
    public void OnClickStartGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager missing in scene!");
            return;
        }

        // 1. Validate & Parse Inputs
        // (Using defaults if empty or invalid)

        MapTypes type = MapTypes.Mountainous; // Default
        if (mapTypeDropdown != null)
        {
            type = (MapTypes)mapTypeDropdown.value;
        }
        int w = ParseInt(widthInput.text, 50);
        int h = ParseInt(heightInput.text, 50);
        int s = ParseInt(spawnersInput.text, 3);
        float water = ParseFloat(waterLevelInput.text, -0.356f);
        float wall = ParseFloat(wallLevelInput.text, 0.4f);
        int m = ParseInt(minimalDistanceInput.text, 3);
        bool a = canSwimToggle != null && canSwimToggle.isOn;
        int e = ParseInt(emptyCellsInput.text, 2);
        // Limit constraints to prevent crashes
        w = Mathf.Clamp(w, 20, 100);
        h = Mathf.Clamp(h, 20, 100);
        s = Mathf.Clamp(s, 1, 5);
        water = Mathf.Clamp(water, -1f, 1f);
        wall = Mathf.Clamp(wall, -1f, 1f);
        m = Mathf.Clamp(m, 1, 10);
        e = Mathf.Clamp(e, 1, 10);


        // 2. Build Config
        MapGenConfig config = new MapGenConfig
        {
            MapType = MapTypes.Mountainous,
            Width = w,
            Height = h,
            SpawnerCount = s,
            WaterLevel = water,
            WallLevel = wall,
            MinimalDistance = m,
            AssumeCanSwim = a,
            EmptyCellsAroundPoints = e
        };

        // 3. Launch
        GameManager.Instance.StartNewGame(selectedSlot, config);
    }

    // Helper for safe parsing
    private int ParseInt(string text, int fallback)
    {
        if (string.IsNullOrEmpty(text)) return fallback;
        if (int.TryParse(text, out int result)) return result;
        return fallback;
    }

    private float ParseFloat(string text, float fallback)
    {
        if (string.IsNullOrEmpty(text)) return fallback;
        if (float.TryParse(text, out float result)) return result;
        return fallback;
    }

    // Link this to Slot Selection Buttons (if any)
    public void SetSlot(int slot)
    {
        selectedSlot = slot;
        Debug.Log($"Selected Save Slot: {slot}");
    }
}