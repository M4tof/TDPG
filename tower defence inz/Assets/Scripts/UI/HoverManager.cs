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
        // Default mask if not set
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

        // 1. Check for UI Blocking (Canvas)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            StatPanelUI.Instance.Hide();
            HideRangeIndicator();
            _currentTarget = null;
            return;
        }

        // 2. Physics Raycast
        Vector2 mousePos = Vector2.zero;
        if (Mouse.current != null) mousePos = Mouse.current.position.ReadValue();
        else if (Input.mousePresent) mousePos = Input.mousePosition; // Fallback

        Vector2 worldPoint = _cam.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, hoverLayerMask);

        if (hit.collider != null)
        {
            GameObject hitObj = hit.collider.gameObject;
            
            // 3. Identify and Update UI
            if (hitObj.TryGetComponent(out EnemyBehavior enemy))
            {
                ShowEnemyStats(enemy);
                HideRangeIndicator();
            }
            else if (hitObj.TryGetComponent(out TurretBase turret))
            {
                ShowTurretStats(turret);
                ShowRangeIndicator(turret); // SHOW RING
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

        var data = enemy.Logic.Data; // SO
        var logic = enemy.Logic;     // Runtime

        StatPanelUI.Instance.Show();
        StatPanelUI.Instance.UpdateStats(
            data.EnemyName,
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
            $"{data.MaxHP}", // TODO: Add CurrentHP logic later
            "-", // Speed N/A
            data.Damage.ToString(),
            data.FireRate.ToString("F1")
        );
    }

    private void ShowRangeIndicator(TurretBase turret)
    {
        if (_currentRangeIndicator == null) return;

        _currentRangeIndicator.gameObject.SetActive(true);
        _currentRangeIndicator.position = turret.transform.position;

        // 1. Get Factors
        float rangeInGridTiles = turret.Data.Range;
        float cellSize = 1.0f;
        
        // Safety check for GridManager
        if (TDPG.Templates.Grid.GridManager.Instance != null)
        {
            cellSize = TDPG.Templates.Grid.GridManager.Instance.CellSize;
        }

        // 2. Calculate Target Diameter (World Space)
        // Range is Radius, so Diameter = Radius * 2
        float worldDiameter = rangeInGridTiles * cellSize * 2f;

        // 3. Normalize Sprite Size
        // If the sprite asset is 2.56 units wide (256px / 100ppu), we need to scale it down/up to match 1 unit first.
        // We get the unscaled bounds from the SpriteRenderer.
        float currentSpriteSize = 1f;
        var sr = _currentRangeIndicator.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            // bounds.size gives the size in world units at scale (1,1,1)
            currentSpriteSize = sr.sprite.bounds.size.x;
        }

        // 4. Final Scale Math
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