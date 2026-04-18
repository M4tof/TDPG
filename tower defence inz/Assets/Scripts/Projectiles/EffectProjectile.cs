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
    
    new void Start()
    {
        base.Start();
        if (GetPlanner() == null)
        {
            SetPlanner(new ElementPlanner(RegistryManager.Instance.GetRegistry()));
        }   
        _audioController = GetComponent<ProceduralAudioController>();
        if (_audioController != null)
        {
            _audioController.Play();
        }
    }
    
    public override void OnTriggerEnter2D(Collider2D other)
    {
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
    }
    
    public override void AddElement(string ElementName)
    {
        if (GetPlanner() == null)
        {
            SetPlanner(new ElementPlanner(RegistryManager.Instance.GetRegistry()));
        }
        base.AddElement(ElementName);
        
    }
}
