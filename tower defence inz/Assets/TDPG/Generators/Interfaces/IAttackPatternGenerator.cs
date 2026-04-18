using TDPG.Generators.AttackPatterns;
using TDPG.Generators.Scalars;

namespace TDPG.Generators.Interfaces
{
    /// <summary>
    /// A generator blueprint responsible for creating complete <see cref="AttackPattern"/> objects.
    /// <br/>
    /// It manages the high-level properties (Duration, Count) and delegates the event arrangement to an <see cref="IAttackPatternLayout"/>.
    /// </summary>
    public interface IAttackPatternGenerator : IGenerator<AttackPattern>
    {
        /// <summary>
        /// Generator responsible for determining the total duration (in seconds) of the attack pattern.
        /// </summary>
        FloatGenerator DurationGenerator { get; set; }
        
        /// <summary>
        /// Generator responsible for determining how many individual events (projectiles) spawn during the pattern.
        /// </summary>
        IntGenerator EventCountGenerator { get; set; }
        
        /// <summary>
        /// Checks if all required sub-generators are assigned and valid.
        /// <br/>
        /// Should throw an <see cref="System.InvalidOperationException"/> if configuration is missing.
        /// </summary>
        void Validate();
        
    }
}
