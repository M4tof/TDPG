using System.Collections.Generic;
using TDPG.Generators.AttackPatterns;
using TDPG.Generators.Scalars;

namespace TDPG.Generators.Interfaces
{
    public interface IAttackPatternLayout
    {
        List<AttackEvent> GenerateEvents(IRandomSource source, int eventCount, float duration, IGenerator<List<float>> directionGenerator = null, FloatGenerator timeOffsetGenerator = null, FloatGenerator speedGenerator = null, IntGenerator damageGenerator = null, FloatGenerator spreadAngleGenerator = null);
    }
}
