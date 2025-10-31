using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class UIManager : MonoBehaviour
{
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
        SceneManager.LoadScene("InitializeNewGame");
    }
    
    public void OnLoadPress(LoadButton caller)
    {
        Debug.Log($"Load Pressed, loading {caller.SavePath}");
        // TODO: Implement loading logic here
        SceneManager.LoadScene("MainGame");
    }
}
