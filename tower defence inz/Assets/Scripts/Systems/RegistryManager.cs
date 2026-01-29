using System.Collections.Generic;
using System.Linq;
using TDPG.EffectSystem.ElementLogic;
using TDPG.EffectSystem.ElementRegistry;
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

                _instance = FindFirstObjectByType<RegistryManager>();
                
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
    

    void Start()
    {
        registry = new Registry();
        
        var slow = new TempSlowDown(0.75f,1f);
        var dmg = new HealthDown(3f);
        var dota = new HealthDrain(1, 3, 1);
        var DmgEffect = new HealthDown(1f);
        var FireEffects = new List<Effect> { dmg, dota };
        var WaterEffects = new List<Effect> { dmg, slow };
        var PoisonEffects = new List<Effect> { dota };

        var FireElement = new Element("Fire", 1, FireEffects);
        var WaterElement = new Element("Water", 2, WaterEffects);
        var PoisonElement =  new Element("Poison", 3, PoisonEffects);

        registry.PutPreMadeElement(new List<int> { 0 }, FireElement);
        registry.PutPreMadeElement(new List<int> { 0 }, WaterElement);
        registry.PutPreMadeElement(new List<int> { 0 }, PoisonElement);
    }

    public Element GetNewElementById(int elementId, string parentDNA)
    {
        Element element = registry.GetElement(elementId);
        if (element != null && elementId != 0)
        {
            return element;
        }
        
        int registrySize = registry.GetAllElements().Count();
        int firstParent = int.Parse(parentDNA.Substring(0, 1)) % ( registrySize - 1 ) + 1;
        int secondParent = int.Parse(parentDNA.Substring(1, 1)) % ( registrySize - 1 ) + 1;

        if (firstParent == secondParent)
        {
            secondParent = (secondParent + 1) % ( registrySize - 1 ) + 1;
        }
        
        List<int> parentList = new List<int> { firstParent, secondParent };

        if (!registry.HasElementWithSameNonRootParents(new HashSet<int>(parentList)))
        {
            return registry.GenerateChildElementFromParents_Recombine(parentList);
        }
        List<Element> elementList = new List<Element> { registry.GetElement(firstParent), registry.GetElement(secondParent) };
        return registry.GetElement(elementList);
    }

    private Element getNewElementFromParents(string parentDNA, int registrySize)
    {
        int firstParent = int.Parse(parentDNA.Substring(0, 1)) % ( registrySize - 1 ) + 1;
        int secondParent = int.Parse(parentDNA.Substring(1, 1)) % ( registrySize - 1 ) + 1;

        if (firstParent == secondParent)
        {
            secondParent = (secondParent + 1) % ( registrySize - 1 ) + 1;
        }
        
        List<int> parentList = new List<int> { firstParent, secondParent };
        if (registry.HasElementWithSameNonRootParents(new HashSet<int>(parentList)))
        {
            return registry.GenerateChildElementFromParents_Recombine(parentList);
        }
        return null;
    }

    public Registry GetRegistry()
    {
        return registry;
    }

    public void LoadRegistry(Registry newRegistry)
    {
        if (newRegistry == null)
        {
            Debug.LogError("[RegistryManager] Tried to load null registry!");
            return;
        }
        
        this.registry = newRegistry;
        Debug.Log("[RegistryManager] Registry overwritten from Save Data.");
    }
    
}
