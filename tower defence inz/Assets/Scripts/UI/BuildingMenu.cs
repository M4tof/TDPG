using UnityEngine;

public class BuildingMenu : MonoBehaviour
{
    [SerializeField] private GameObject buildingPanel;
    [SerializeField] private CardSelectionMenu cardSelectionMenu;
    private bool isActive = false;
    
    void Start()
    {
        buildingPanel.SetActive(false);
    }

    public void OpenBuildingPanel()
    {
        buildingPanel.SetActive(true);
        isActive = true;
    }

    public void CloseBuildingPanel()
    {
        buildingPanel.SetActive(false);
        cardSelectionMenu.Close();
        isActive = false;
    }
    
    public void SwitchBuildingPanel()
    {
        isActive = !isActive;
        buildingPanel.SetActive(isActive);
        cardSelectionMenu.Close();
    }

    public bool GetIsActive()
    {
        return isActive;
    }
    
}
