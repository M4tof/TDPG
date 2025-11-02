using Assets.TDPG.Generators.Scalars;
using Assets.TDPG.Generators.AttackPatterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.TDPG.Generators.Interfaces
{
    public interface IAttackPatternGenerator : IGenerator<AttackPattern>
    {
        FloatGenerator DurationGenerator { get; set; }
        IntGenerator EventCountGenerator { get; set; }
        void Validate();
    }
}
