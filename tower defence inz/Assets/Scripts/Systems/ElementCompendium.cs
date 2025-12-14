using QuikGraph;
using System.Collections.Generic;
using System.Linq;
using TDPG.EffectSystem.ElementLogic;
using TDPG.EffectSystem.ElementRegistry;
using UnityEngine;

public class ElementCompendium : MonoBehaviour
{
    public static ElementCompendium Instance { get; private set; }

    private Registry registry;
    private List<Element> cachedElements;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Start()
    {
        registry = RegistryManager.Instance.GetRegistry();
        RefreshCache();
    }

    public void RefreshCache()
    {
        if (registry == null)
        {
            Debug.LogWarning("Registry is null in ElementCompendium!");
            return;
        }

        cachedElements = registry.GetAllElements().ToList();
    }

    public List<Element> GetAllElements()
        => cachedElements;

    public Element GetElement(int id)
        => registry.GetElement(id);

    public Element GetElement(string name)
        => cachedElements.FirstOrDefault(e => e.Name == name);

    // --- NOWE: to czego potrzebuje GraphMenu ---
    public IEnumerable<Element> GetAllNodes()
        => registry.GetAllElements(); // bez cache!

    public IEnumerable<Edge<Element>> GetAllEdges()
        => registry.GetEdges();
}
