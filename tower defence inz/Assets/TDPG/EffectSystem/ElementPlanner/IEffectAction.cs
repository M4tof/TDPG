using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDPG.EffectSystem.ElementPlanner
{
    public interface IEffectAction
    {
        string Name { get; }
        float Intensity { get; }
        void Execute(EffectContext context);
    }
}
