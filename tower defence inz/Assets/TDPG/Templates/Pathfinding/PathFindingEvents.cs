namespace TDPG.Templates.Pathfinding
{
    /// <summary>
    /// A static event bus responsible for broadcasting changes in the navigation grid.
    /// <br/>
    /// Pathfinding agents should subscribe to these events to recalculate paths dynamically 
    /// when the environment changes (e.g., a wall is placed or destroyed).
    /// </summary>

    public static class PathfindingEvents
    {
        /// <summary>
        /// Triggered whenever the Grid topology is modified (e.g., a wall is built, a door opens).
        /// <br/>
        /// <b>Usage:</b> Listeners should clear their path cache and re-query the pathfinder when this fires.
        /// </summary>
        public static System.Action OnGridChanged;
    
        /// <summary>
        /// Safely invokes the <see cref="OnGridChanged"/> event.
        /// </summary>
        public static void TriggerGridChanged()
        {
            OnGridChanged?.Invoke();
        }
        
    }
}