using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TDPG.Templates.Turret; // For TurretBase

public class HoverManager : MonoBehaviour
{
    private Camera _cam;
    private GameObject _currentTarget;

    // Filter to avoid hitting walls/background
    [SerializeField] private LayerMask hoverLayerMask;

    [Header("Visuals")]
    [Tooltip("Assign a GameObject with a SpriteRenderer (Circle) here.")]
    [SerializeField] private GameObject rangeIndicatorPrefab;

    private Transform _currentRangeIndicator;


    void Start()
    {
        _cam = Camera.main;
        if (hoverLayerMask.value == 0) hoverLayerMask = LayerMask.GetMask("Default", "Enemy");
        if (rangeIndicatorPrefab != null)
        {
            GameObject obj = Instantiate(rangeIndicatorPrefab, transform); // Child of Camera/Manager
            obj.name = "RangeIndicator_Runtime";
            _currentRangeIndicator = obj.transform;
            obj.SetActive(false);
        }
    }

    void Update()
    {
        if (StatPanelUI.Instance == null) return;

        // Check for UI Blocking (Canvas)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            StatPanelUI.Instance.Hide();
            HideRangeIndicator();
            _currentTarget = null;
            return;
        }

        // Physics Raycast
        Vector2 mousePos = Vector2.zero;
        if (Mouse.current != null) mousePos = Mouse.current.position.ReadValue();
        else if (Input.mousePresent) mousePos = Input.mousePosition; // Fallback

        Vector2 worldPoint = _cam.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, hoverLayerMask);

        TurretBase turret = null;

        if (hit.collider != null)
        {
            GameObject hitObj = hit.collider.gameObject;

            // Identify and Update UI
            if (hitObj.TryGetComponent(out EnemyBehavior enemy))
            {
                ShowEnemyStats(enemy);
                HideRangeIndicator();
            }
            else if ((turret = hitObj.GetComponentInParent<TurretBase>()) != null)
            {
                ShowTurretStats(turret);
                ShowRangeIndicator(turret);
            }
            else
            {
                // Hit a wall or non-interactive object
                StatPanelUI.Instance.Hide();
                HideRangeIndicator();
            }
        }
        else
        {
            // Hit nothing
            StatPanelUI.Instance.Hide();
            HideRangeIndicator();
            _currentTarget = null;
        }
    }

    private void ShowEnemyStats(EnemyBehavior enemy)
    {
        if (enemy.Logic == null || enemy.Logic.Data == null) return;

        var data = enemy.Logic.Data;
        var logic = enemy.Logic;

        StatPanelUI.Instance.Show();
        StatPanelUI.Instance.UpdateStats(
            $"{logic.GeneratedName} the {data.EnemyName}",
            $"{Mathf.CeilToInt(logic.CurrentHealth)} / {Mathf.CeilToInt(logic.DynamicMaxHealth)}",
            logic.CurrentSpeed.ToString("F1"),
            data.Damage.ToString(),
            data.AttackSpeed.ToString("F1")
        );
    }

    private void ShowTurretStats(TurretBase turret)
    {
        if (turret.Data == null) return;

        var data = turret.Data;

        StatPanelUI.Instance.Show();
        StatPanelUI.Instance.UpdateStats(
            data.TurretID,
            $"{data.MaxHP}",
            "-",
            data.Damage.ToString(),
            data.FireRate.ToString("F1")
        );
    }

    private void ShowRangeIndicator(TurretBase turret)
    {
        if (_currentRangeIndicator == null) return;
        if (turret == null || turret.Data == null) return;

        SpriteRenderer baseSprite = turret.GetComponentInChildren<SpriteRenderer>();

        _currentRangeIndicator.gameObject.SetActive(true);
        // _currentRangeIndicator.position = turret.transform.position;


        if (baseSprite != null)
        {
            _currentRangeIndicator.position = baseSprite.transform.position;
        }
        else
        {
            _currentRangeIndicator.position = turret.transform.position;
        }

        // Get Factors
        float rangeInGridTiles = turret.Data.Range;
        float cellSize = 1.0f;

        // Safety check for GridManager
        if (TDPG.Templates.Grid.GridManager.Instance != null)
        {
            cellSize = TDPG.Templates.Grid.GridManager.Instance.CellSize;
        }

        // Calculate Target Diameter (World Space)
        float worldDiameter = rangeInGridTiles * cellSize * 2f;

        // Normalize Sprite Size
        float currentSpriteSize = 1f;
        var sr = _currentRangeIndicator.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            currentSpriteSize = sr.sprite.bounds.size.x;
        }

        // Desired / Actual = Scalar
        float finalScale = worldDiameter / currentSpriteSize;

        _currentRangeIndicator.localScale = new Vector3(finalScale, finalScale, 1f);
    }

    private void HideRangeIndicator()
    {
        if (_currentRangeIndicator != null)
        {
            _currentRangeIndicator.gameObject.SetActive(false);
        }
    }
}