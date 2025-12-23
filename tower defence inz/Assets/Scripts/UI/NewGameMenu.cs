using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming TextMeshPro for inputs
using TDPG.Templates.Grid.MapGen;
using System;
using System.Linq;
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
        if (mapTypeDropdown != null)
        {
            mapTypeDropdown.ClearOptions();

            // Get all names
            var allTypes = Enum.GetNames(typeof(MapTypes));

            // Filter out "Static"
            var options = allTypes
                .Where(name => name != MapTypes.Static.ToString())
                .ToList();

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
            // Get the string text currently shown on the dropdown
            string selectedText = mapTypeDropdown.options[mapTypeDropdown.value].text;

            // Convert string back to Enum
            type = (MapTypes)Enum.Parse(typeof(MapTypes), selectedText);
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
        w = Mathf.Max(w, 20);
        h = Mathf.Max(h, 20);
        s = Mathf.Max(s, 1);
        water = Mathf.Clamp(water, -1.0f, -0.001f);
        wall = Mathf.Clamp(wall, 0.001f, 1f);
        m = Mathf.Max(m, 1);
        e = Mathf.Max(e, 1);

        
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
        Debug.Log($"[NewGameMenu]: \n{Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented)}");
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