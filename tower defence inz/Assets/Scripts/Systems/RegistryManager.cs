using System.Collections.Generic;
using TDPG.EffectSystem.ElementLogic;
using TDPG.EffectSystem.ElementRegistry;
using TDPG.Generators.Seed;
using UnityEngine;

public class RegistryManager : MonoBehaviour
{
    
    [SerializeField] private Registry registry;
    
    private static RegistryManager _instance;
    
    public static RegistryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RegistryManager>();
                
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("RegistryManager");
                    _instance = singletonObject.AddComponent<RegistryManager>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        registry = new Registry();
        
        var slow = new TempSlowDown(0.75f,1f);
        var burn = new HealthDown(3f);
        var effects = new List<Effect> { slow, burn };

        var element = new Element("Fire", 1, effects);
        var other_element = new Element("Water", 2, effects);

        registry.PutPreMadeElement(new List<int> { 0 }, element);
        registry.PutPreMadeElement(new List<int> { 1 }, other_element);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Registry GetRegistry()
    {
        return registry;
    }

    
}
