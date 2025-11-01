using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    
    [SerializeField] private GameObject firstPage;
    [SerializeField] private GameObject[] pageList;
    
    private bool menuActive = false;

    void Start()
    {
        Close();
    }
    public void OpenFirstPage()
    {
        menuActive = true;
        firstPage.SetActive(true);
        foreach (GameObject page in pageList)
        {
            page.SetActive(false);
        }
    }

    public void Close()
    {
        menuActive = false;
        firstPage.SetActive(false);
        foreach (GameObject page in pageList)
        {
            page.SetActive(false);
        }
    }
    
    public void SwitchMenu()
    {
        if (menuActive)
        {
            Close();
            return;
        }
        OpenFirstPage();
    }
}
