using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{

    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI descriptionText;

    private void Start()
    {
        panel.SetActive(false);
    }
    
    public void OpenGameOverMessage()
    {
        Time.timeScale = 0f;
        panel.SetActive(true);
        WriteMessage();
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        ResourceSystem.Instance.ResetResources();
    }
    private void WriteMessage()
    {
        int currentWave = WaveManager.Instance.GetCurrentWave();
        descriptionText.text = "You survived: " + currentWave + " waves.";
    }
}
