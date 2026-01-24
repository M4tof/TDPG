using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance { get; private set; }

    private GameObject Caller {get; set; }

    void Start()
    {

    }

    void Update()
    {

    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Duplicate GameManager destroyed. Only one instance allowed.");
        }
        else
        {
            Instance = this;
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
