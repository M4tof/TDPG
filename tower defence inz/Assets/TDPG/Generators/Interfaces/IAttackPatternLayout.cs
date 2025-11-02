using Assets.TDPG.Generators.AttackPatterns;
using Assets.TDPG.Generators.Scalars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.TDPG.Generators.Interfaces
{
    public interface IAttackPatternLayout
    {
        List<AttackEvent> GenerateEvents(IRandomSource source, int eventCount, float duration, IGenerator<List<float>> directionGenerator = null, FloatGenerator timeOffsetGenerator = null, FloatGenerator speedGenerator = null, IntGenerator damageGenerator = null, FloatGenerator spreadAngleGenerator = null);
    }
}
