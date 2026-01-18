using System.Collections.Generic;
using TDPG.AudioModulation;
using TDPG.EffectSystem.ElementLogic;
using TDPG.EffectSystem.ElementPlanner;
using UnityEngine;

public class EffectProjectile : BasicProjectile
{
    private EffectContext effectContext;
    private ProceduralAudioController _audioController; 
    private bool hasHit = false;
    
    void Start()
    {
        base.Start();
        //TODO Zmienić aby nie hard codować elementów :) :] 
        //elements.Add(RegistryManager.Instance.GetRegistry().GetElement("Fire"));
        if (GetPlanner() == null)
        {
            SetPlanner(new ElementPlanner(RegistryManager.Instance.GetRegistry()));
        }
        //planner.RegisterElement("Fire");
        //planner.BuildPlan();
        
        _audioController = GetComponent<ProceduralAudioController>();
        if (_audioController != null)
        {
            _audioController.Play();
        }
    }
    
    public override void OnTriggerEnter2D(Collider2D other)
    {
        /*public IReadOnlyList<IEffectAction> GetPlannedActions()
        {
            // Najbezpieczniej zwracać *read-only snapshot*
            return plannedActions.AsReadOnly();
        }*/
        Debug.Log("TRIGGER ENTER: EFFECT");
        if (GetPlanner().GetPlannedActions().Count == 0)
        {
            AddElement("Root");
        }
        if (hasHit) return;
        if (other.GetComponent<EnemyBehavior>() != null)
        {
            hasHit = true;
            
            Collider2D projectileCollider = GetComponent<Collider2D>();
            if (projectileCollider != null)
            {
                projectileCollider.enabled = false;
            }
            effectContext = new EffectContext();
            effectContext.Target = other.gameObject;
            GetPlanner().ExecutePlan(effectContext);
        
            Destroy(gameObject);
        }
        /*EnemyBehavior enemyBehavior = other.gameObject.GetComponent<EnemyBehavior>();
        if (enemyBehavior != null)
        {
            //enemyBehavior.DealDamage(GetDamage());
            
            //Set Effect
            if (enemyBehavior.GetCurrentHealth() > 0)
            {
                Debug.Log(planner.GetPlannedActions());
                effectContext = new EffectContext();
                effectContext.Target = other.gameObject;
                planner.ExecutePlan(effectContext);
            }
             
        }
        Destroy(gameObject);*/
    }
    
    public override void AddElement(string ElementName)
    {
        if (GetPlanner() == null)
        {
            SetPlanner(new ElementPlanner(RegistryManager.Instance.GetRegistry()));
        }
        base.AddElement(ElementName);
        
    }

    /*public void addElement(string ElementName)
    {
        planner.RegisterElement(ElementName);
        planner.BuildPlan();
    }
    
    public void addElement(int id)
    {
        //TODO zrobić po ID
        //planner.RegisterElement(ElementName);
    }*/
}
