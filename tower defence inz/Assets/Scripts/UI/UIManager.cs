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
        // Singleton pattern to ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            // If another GameManager already exists, destroy this one
            Destroy(gameObject);
            Debug.LogWarning("Duplicate UIManager destroyed. Only one instance allowed.");
        }
        else
        {
            // If this is the first GameManager, make it the instance
            Instance = this;
            // Prevents the GameObject from being destroyed when reloading a scene
            DontDestroyOnLoad(gameObject);
            Debug.Log("UIManager created and set to not destroy on load.");
            GM= GameManager.Instance;
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnNewGamePress()
    {
        Debug.Log("New Game Pressed");
        Debug.Break();
        SceneManager.LoadScene("InitializeNewGame");
    }

    public void OnLoadPress(SaveLoadButton caller)
    {
        Debug.Log($"Load Pressed, loading {caller.SavePath}");
        // TODO: Implement loading logic here
        GM.LoadGame(caller.SavePath);
        GM.SetSlot(caller.SlotNumber);
        Debug.Break();
        SceneManager.LoadScene("MainGame");
    }

    public void OnSavePress(SaveLoadButton caller)
    {
        Debug.Log($"Save Pressed, saving to {caller.SavePath}");
        GM.SetSlot(caller.SlotNumber);
        GM.SaveGame(caller.SavePath);
        // TODO: Implement saving logic here
    }
}
