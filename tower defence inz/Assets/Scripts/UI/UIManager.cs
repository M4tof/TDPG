using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public GameManager GM;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Duplicate UIManager destroyed. Only one instance allowed.");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("UIManager created and set to not destroy on load.");
        }
    }
    
    void Start()
    {
        GM = GameManager.Instance;
    }

    void Update()
    {
        
    }

    public void OnNewGamePress()
    {
        Debug.Log("New Game Pressed");
        SceneManager.LoadScene("InitializeNewGame");
    }

    public void OnLoadPress(SaveLoadButton caller)
    {
        Debug.Log($"Load Pressed, loading {caller.SavePath}");
        GM.PendingLoadPath = caller.SavePath;
        GM.PendingLoadSlot = caller.SlotNumber;
        SceneManager.LoadScene("MainGame");

    }

    public void OnSavePress(SaveLoadButton caller)
    {
        Debug.Log($"Save Pressed, saving to {caller.SavePath}");
        GM.SetSlot(caller.SlotNumber);
        GM.SaveGame(caller.SavePath);
    }
}
