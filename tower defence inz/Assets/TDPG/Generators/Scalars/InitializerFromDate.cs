using System;

namespace TDPG.Generators.Scalars
{
    public static class InitializerFromDate
    {
        public static ulong QuickGenerate()
        {
            DateTime now = DateTime.Now;
            // This will always produce a valid number in range: 00000000 to 31235959
            return (ulong)(now.Day * 1000000 + now.Hour * 10000 + now.Minute * 100 + now.Second + now.Millisecond*10);
        }
    }
}