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
        panel.SetActive(true);
        WriteMessage();
    }
    
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    private void WriteMessage()
    {
        int currentWave = WaveManager.Instance.GetCurrentWave();
        descriptionText.text = "You survived: " + currentWave + " waves.";
    }
}
