using QuikGraph;
using System.Collections.Generic;
using System.Linq;
using TDPG.EffectSystem.ElementLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphMenu : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private GameObject graphPage;
    [SerializeField] private GameObject detailsPage;

    [Header("Drawing container")]
    [SerializeField] private RectTransform graphContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject edgePrefab;

    [Header("Registry")]
    [SerializeField] private ElementCompendium registryHolder;

    private bool menuActive = false;

    private void Start()
    {
        graphPage.SetActive(false);
        detailsPage.SetActive(false);
    }

    public void Open()
    {
        menuActive = true;
        graphPage.SetActive(true);
        RedrawGraph();
    }

    public void Close()
    {
        menuActive = false;
        graphPage.SetActive(false);
        detailsPage.SetActive(false);
        ClearGraphVisuals();
    }

    public void Toggle()
    {
        if (menuActive) Close();
        else Open();
    }

    private void RedrawGraph()
    {
        ClearGraphVisuals();

        if (registryHolder == null)
        {
            Debug.LogError("GraphMenu: registryHolder is NULL");
            return;
        }

        IEnumerable<Element> nodes = registryHolder.GetAllNodes();
        IEnumerable<Edge<Element>> edges = registryHolder.GetAllEdges();

        // Create node UI
        var nodeMap = new Dictionary<Element, RectTransform>();
        int i = 0;
        int total = nodes != null ? nodes.Count() : 0;

        foreach (var element in nodes)
        {
            var nodeGO = CreateNode(element, i, total);
            nodeMap[element] = nodeGO.GetComponent<RectTransform>();
            i++;
        }

        // Create edges
        if (edges != null)
        {
            foreach (var edge in edges)
            {
                if (nodeMap.ContainsKey(edge.Source) && nodeMap.ContainsKey(edge.Target))
                {
                    CreateEdge(nodeMap[edge.Source], nodeMap[edge.Target]);
                }
            }
        }
    }

    private GameObject CreateNode(Element element, int index, int total)
    {
        GameObject go = Instantiate(nodePrefab, graphContainer);
        go.name = $"Node_{element.Name}_{element.Id}";

        var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp) tmp.text = element.Name;

        float angle = index * Mathf.PI * 2f / Mathf.Max(total, 1);
        float radius = Mathf.Min(graphContainer.rect.width, graphContainer.rect.height) * 0.35f;

        Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;

        // Hook node click
        var button = go.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => ShowDetails(element));
        }

        return go;
    }

    private void CreateEdge(RectTransform a, RectTransform b)
    {
        GameObject line = Instantiate(edgePrefab, graphContainer);
        var lineRect = line.GetComponent<RectTransform>();

        Vector2 dir = b.anchoredPosition - a.anchoredPosition;
        float dist = dir.magnitude;

        lineRect.sizeDelta = new Vector2(dist, lineRect.sizeDelta.y);
        lineRect.anchoredPosition = a.anchoredPosition + dir * 0.5f;
        lineRect.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    private void ClearGraphVisuals()
    {
        for (int i = graphContainer.childCount - 1; i >= 0; i--)
            Destroy(graphContainer.GetChild(i).gameObject);
    }

    private void ShowDetails(Element element)
    {
        detailsPage.SetActive(true);

        var text = detailsPage.GetComponentInChildren<TextMeshProUGUI>();
        if (!text) return;

        var effects = element.GetEffects();
        string effectsText = effects != null
            ? string.Join("\n", effects.Select(e => $"- {e.Name}: {e.Description}"))
            : "None";

        text.text =
            $"<b>{element.Name}</b>\n\n" +
            $"DNA: {element.GetDna()}\n\n" +
            $"<b>Effects</b>\n{effectsText}";
    }
}
