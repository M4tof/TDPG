using System;

namespace TDPG.Generators.Scalars
{
    
    /// <summary>
    /// A utility class for generating non-deterministic initial seed values based on the system clock.
    /// <br/>
    /// Useful for bootstrapping RNGs (creating a "random" starting point) or adding time-based 'salt' to existing seeds.
    /// </summary>
    public static class InitializerFromDate
    {
        /// <summary>
        /// Generates a <see cref="ulong"/> value derived from the current Date and Time (down to the millisecond).
        /// </summary>
        /// <param name="slotNum">
        /// A multiplier index (e.g., Save Slot ID).
        /// <br/>
        /// Used to differentiate the result if multiple generators are initialized at the exact same moment.
        /// </param>
        /// <returns>
        /// A time-dependent value.
        /// <br/>
        /// <b>Range:</b> Approximately 0 to 93,707,877 (assuming slotNum is between 1 and 3).
        /// </returns>
        public static ulong QuickGenerate(int slotNum)
        {
            DateTime now = DateTime.Now;
            // This will always produce a valid number in range: 0 to 93,707,877 (for slotNum 1-3)
            // Daily range per slot: 0 to 31,235,959
            return (ulong)(slotNum*(now.Day * 1000000 + now.Hour * 10000 + now.Minute * 100 + now.Second + now.Millisecond*10));
        }
    }
}