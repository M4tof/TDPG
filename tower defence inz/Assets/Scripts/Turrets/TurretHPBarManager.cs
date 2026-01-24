using TDPG.Templates.Turret;
using UnityEngine;

[RequireComponent(typeof(Turret))]
public class TurretHPBarManager : MonoBehaviour
{
    [SerializeField] private HPBarVisualisation hpBarVisualiation;
    private Turret turret;

    void Start()
    {
        turret = GetComponent<Turret>();
        hpBarVisualiation.Init(turret.GetCurrentHealth());    
        turret.HealthChanged.AddListener(OnHealthChanged);
    }

    private void OnHealthChanged()
    {
        hpBarVisualiation.SetValue(turret.GetCurrentHealth()); 
    }
}
