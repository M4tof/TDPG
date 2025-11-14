using UnityEngine;

public class BuildingMenu : MonoBehaviour
{
    [SerializeField] private GameObject buildingPanel;
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
        isActive = false;
    }
    
    public void SwitchBuildingPanel()
    {
        isActive = !isActive;
        buildingPanel.SetActive(isActive);
    }

    public bool GetIsActive()
    {
        return isActive;
    }
    
}
