using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDPG.Templates.Turret
{
    [RequireComponent(typeof(Image))]
    public class TurretSelection : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField] [Tooltip("Turret id")] private string turretToSpawn;
        [SerializeField] [Tooltip("For Debugging")] private List<CardData> turretCards;
        [Header("References")]
        [SerializeField] [Tooltip("Component used to spawn turrets")] TurretSpawner turretSpawner;
        [SerializeField] [Tooltip("Menu which show cards to select")] CardSelectionMenu cardSelectionMenu;

        [Header("UI")] 
        [SerializeField] [Tooltip("Component used to spawn turrets")] private TMP_Text upgradeCount;
        
        

        public void SelectTurret()
        {
            if (turretToSpawn != null)
            {
                turretSpawner.SetTurretToSpawn(turretToSpawn,turretCards);
            }
        }

        public void AddUpgrade(CardData upgrade)
        {
            turretCards.Add(upgrade);
            upgradeCount.text = GetTurretUpgrades().ToString();
        }

        public void OpenCardSelectionMenu()
        {
            cardSelectionMenu.ShowPlayersCards(this);
        }

        public int GetTurretUpgrades()
        {
            return turretCards.Count;
        }

        public string GetTurretName()
        {
            return turretToSpawn;
        }

        public void SetTurretName(string turretToSpawn)
        {
            this.turretToSpawn = turretToSpawn;
        }

        public void SetTurretSpawner(TurretSpawner turretSpawner)
        {
            this.turretSpawner = turretSpawner;
        }

        public void SetCardSelectionMenu(CardSelectionMenu cardSelectionMenu)
        {
            this.cardSelectionMenu = cardSelectionMenu;
        }

        public void SetImage(Sprite sprite)
        {
            GetComponent<Image>().sprite = sprite;
        }
    
        void OnValidate()
        {
            if (turretSpawner == null)
            {
                Debug.LogWarning("Turret Spawner is not assigned", this);
            }
            if (turretToSpawn == null)
            {
                Debug.LogWarning("Turret to spawn is null", this);
            }

            if (cardSelectionMenu == null)
            {
                Debug.LogWarning("Card selection menu is null", this);
            }

            if (upgradeCount == null)
            {
                Debug.LogWarning("Upgrade count is null", this);
            }
        }
    }
}
