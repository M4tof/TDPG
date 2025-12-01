using TDPG.EffectSystem.ElementLogic;
using TDPG.EffectSystem.ElementRegistry;
using UnityEngine;
using UnityEngine.UI;
using QuikGraph;
using System.Linq;

public class RegistryMenu : MonoBehaviour
{
    [Header("UI Pages")]
    [SerializeField] private GameObject graphPage;    // g³ówny panel grafu
    [SerializeField] private GameObject detailsPage;  // panel z opisem wybranego node'a

    [Header("Graph Drawing Handle")]
    [SerializeField] private RectTransform graphContainer;
    // Tu rysujemy graf: linie, ikonki, nazwy

    [Header("Prefabs")]
    public GameObject nodePrefab;       // przycisk/ikonka reprezentuj¹ca element
    public GameObject edgePrefab;       // najprostsza linia (UI Image)

    private bool menuActive = false;
    private Registry registry;

    private void Start()
    {
        Close();
        registry = new Registry();
    }

    // ---------------------------
    // PUBLIC MENU CONTROLS
    // ---------------------------
    public void Open()
    {
        menuActive = true;
        graphPage.SetActive(true);
        detailsPage.SetActive(false);

        RedrawGraph();
    }

    public void Close()
    {
        menuActive = false;
        graphPage.SetActive(false);
        detailsPage.SetActive(false);

        ClearGraphVisuals();
    }

    public void ToggleMenu()
    {
        if (menuActive) Close();
        else Open();
    }

    public bool IsOpen => menuActive;

    // ---------------------------
    // GRAPH RENDERING
    // ---------------------------
    private void RedrawGraph()
    {
        if (registry == null)
        {
            Debug.LogError("GraphMenu cannot find Registry.");
            return;
        }

        ClearGraphVisuals();

        foreach (var element in registry.GetAllElements())
        {
            CreateNode(element);
        }

        foreach (var edge in registry.GetEdges())
        {
            CreateEdge(edge);
        }
    }

    private void ClearGraphVisuals()
    {
        if (graphContainer == null) return;

        foreach (Transform child in graphContainer)
            Destroy(child.gameObject);
    }

    // ---------------------------
    // NODE CREATION
    // ---------------------------
    private void CreateNode(Element element)
    {
        if (nodePrefab == null || graphContainer == null) return;

        GameObject go = Instantiate(nodePrefab, graphContainer);
        go.name = $"Node_{element.Name}";

        // set text if nodePrefab has UI.Text or TMP
        var label = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (label != null)
            label.text = element.Name;

        // random simple placement
        go.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(Random.Range(-300, 300), Random.Range(-200, 200));

        // Add button to open description
        var button = go.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => ShowDetails(element));
        }
    }

    // ---------------------------
    // EDGE CREATION
    // ---------------------------
    private void CreateEdge(Edge<Element> edge)
    {
        if (edgePrefab == null || graphContainer == null) return;

        GameObject line = Instantiate(edgePrefab, graphContainer);
        line.name = $"Edge_{edge.Source.Name}_to_{edge.Target.Name}";

        // find the node objects by name
        RectTransform a = graphContainer.Find($"Node_{edge.Source.Name}") as RectTransform;
        RectTransform b = graphContainer.Find($"Node_{edge.Target.Name}") as RectTransform;

        if (a == null || b == null)
        {
            Debug.LogWarning("Could not connect nodes: missing node UI.");
            Destroy(line);
            return;
        }

        RectTransform lineRect = line.GetComponent<RectTransform>();

        Vector2 dir = b.anchoredPosition - a.anchoredPosition;
        float dist = dir.magnitude;

        lineRect.sizeDelta = new Vector2(dist, 3f);
        lineRect.anchoredPosition = a.anchoredPosition + dir / 2f;
        lineRect.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    // ---------------------------
    // DETAILS PANEL
    // ---------------------------
    private void ShowDetails(Element element)
    {
        if (detailsPage == null) return;

        detailsPage.SetActive(true);

        var txt = detailsPage.GetComponentInChildren<TMPro.TextMeshProUGUI>();

        if (txt != null)
        {
            string effectsText = string.Join("\n", element.GetEffects().Select(e => $"- {e.Name}: {e.Description}"));
            txt.text =
                $"<b>{element.Name}</b>\n\n" +
                $"DNA: {element.GetDna()}\n\n" +
                $"<b>Effects:</b>\n{effectsText}";
        }
    }
}