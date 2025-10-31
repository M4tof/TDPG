using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance { get; private set; }

    private GameObject Caller {get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            // If another GameManager already exists, destroy this one
            Destroy(gameObject);
            Debug.LogWarning("Duplicate GameManager destroyed. Only one instance allowed.");
        }
        else
        {
            // If this is the first GameManager, make it the instance
            Instance = this;
            // Prevents the GameObject from being destroyed when reloading a scene
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager created and set to not destroy on load.");
        }
    }


    public void OpenMenu(GameObject? caller)
    {
        if (caller != null)
        {
            this.Caller = caller;
            caller.SetActive(false);
        }
        gameObject.SetActive(true);
    }
    
    public void CloseMenu()
    {
        gameObject.SetActive(false);
        if (Caller != null)
        {
            Caller.SetActive(true);
            Caller = null;
        }
    }
}
