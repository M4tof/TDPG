using TDPG.Generators.AttackPatterns;
using TDPG.Generators.Scalars;

namespace TDPG.Generators.Interfaces
{
    public interface IAttackPatternGenerator : IGenerator<AttackPattern>
    {
        FloatGenerator DurationGenerator { get; set; }
        IntGenerator EventCountGenerator { get; set; }
        void Validate();
    }
}
