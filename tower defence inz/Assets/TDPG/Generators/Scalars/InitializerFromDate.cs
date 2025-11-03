using System;

namespace TDPG.Generators.Scalars
{
    public static class InitializerFromDate
    {
        public static ulong QuickGenerate(int slotNum)
        {
            DateTime now = DateTime.Now;
            // This will always produce a valid number in range: 0 to 93,707,877 (for slotNum 1-3)
            // // Daily range per slot: 0 to 31,235,959
            return (ulong)(slotNum*(now.Day * 1000000 + now.Hour * 10000 + now.Minute * 100 + now.Second + now.Millisecond*10));
        }
    }
}