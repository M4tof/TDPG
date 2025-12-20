using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDPG.EffectSystem.ElementPlanner
{
    /// <summary>
    /// Defines the mathematical strategy used when multiple effects modify the same parameter.
    /// </summary>
    public enum StackMode
    {
        /// <summary>
        /// Values are summed together.
        /// <br/>Example: Effect A (-10 HP) and Effect B (-20 HP) result in -30 HP.
        /// </summary>
        Additive, 
        
        /// <summary>
        /// The most recent value replaces all previous ones (Last-Write-Wins).
        /// <br/>Example: Setting a "State" where only the latest command matters.
        /// </summary>
        Override
    }
}
