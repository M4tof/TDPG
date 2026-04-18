using TDPG.Templates.Turret;
using UnityEngine;

public class BuildingMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] [Tooltip("Root of Building Menu")] private GameObject buildingPanel;
    [SerializeField] [Tooltip("Panel where buttons show")] private GameObject buttonBuildingPanel;
    [SerializeField] [Tooltip("Used for upgrades")] private CardSelectionMenu cardSelectionMenu;
    [SerializeField] [Tooltip("Component used to spawn turrets")] TurretSpawner turretSpawner;
    
    [Header("Turret Selection")]
    [SerializeField] [Tooltip("List of paramaters of turrets")] private TurretData[] turretData;
    [SerializeField] [Tooltip("Prefab of button")] private GameObject turretButtonPrefab;
    [SerializeField] [Tooltip("Position of first button")] private Vector2 startPosition = new Vector2(-400, 0);
    [SerializeField] [Tooltip("Space between positions of buttons")] private float diffrenceBetweenButtons = 200;
    
    
    private int turretAmount = 0;
    private bool isActive = false;
    
    void Start()
    {
        buildingPanel.SetActive(false);

        foreach (TurretData data in turretData)
        {
            AddNewTurret(data);
        }
        
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

    public void AddNewTurret(TurretData data)
    {
        //Create Button
        GameObject turretButton = Instantiate(turretButtonPrefab, buttonBuildingPanel.transform);
        turretButton.GetComponent<RectTransform>().anchoredPosition = startPosition + new Vector2(turretAmount * diffrenceBetweenButtons,0);
        turretAmount += 1;
        
        //Set parameters
        TurretSelection turret = turretButton.GetComponent<TurretSelection>();
        if (turret != null)
        {
            turret.SetTurretSpawner(turretSpawner);
            turret.SetCardSelectionMenu(cardSelectionMenu);
            turret.SetTurretName(data.TurretID);
            turret.SetCost(data.Cost);
            if (data.CrystalSprite != null)
            {
                turret.SetImage(data.CrystalSprite);
            }
            else
            {
                turret.SetImage(data.BaseSprite);
            }
        }
    }
    
    void OnValidate()
    {
        if (turretSpawner == null)
        {
            Debug.LogWarning("Turret Spawner is not assigned", this);
        }
        if (cardSelectionMenu == null)
        {
            Debug.LogWarning("card Selection Menu is null", this);
        }

        if (turretData == null || turretData.Length == 0)
        {
            Debug.LogWarning("turret Data is null or empty", this);
        }
    }
}
