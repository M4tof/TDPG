using UnityEngine;
using TDPG.Templates.Enemies;

/// <summary>
/// Abstract base class defining the visual and logical template for a status effect.
/// <br/>
/// Inherit from this to create concrete status assets (e.g., "Burning", "Frozen") that can be assigned 
/// via the Unity Inspector.
/// </summary>
public abstract class StatusComponent : ScriptableObject
{
    /// <summary>
    /// The display name or unique identifier for this status (e.g., "Poison").
    /// </summary>
    [Tooltip("The display name or unique identifier for this status.")]
    public string StatusName;
    
    /// <summary>
    /// The visual icon displayed in the UI (Health Bars / Status Panels) when this effect is active.
    /// </summary>
    [Tooltip("The visual icon displayed in the UI when this status is active.")]
    public Sprite StatusIcon;
    
    /// <summary>
    /// Executes the specific gameplay logic of this status on the target.
    /// <br/>
    /// <b>Example:</b> Reducing movement speed, applying a damage tick, or disabling attacks.
    /// </summary>
    /// <param name="enemy">The target entity receiving the status.</param>
    public abstract void Apply(EnemyBase enemy);
}