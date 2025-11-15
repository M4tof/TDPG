using System;
using static TDPG.Generators.Seed.Genetic;

namespace TDPG.Generators.Seed
{
    public interface ISeed
    {
        ulong GetBaseValue();
        string GetName();
        public static Seed operator *(ISeed a, ISeed b) // explicit way to make a new seed, can use Seed and GlobalSeed as parents
        {
            string parentName = "_ChildOf:" + a.GetName() + b.GetName();
            int id = -1; // -1 is the standard we agree on to represent a seed that was made outside GlobalSeed functions (out of wedlock xd)
            
            byte[] aByte = BitConverter.GetBytes(a.GetBaseValue());
            byte[] bByte = BitConverter.GetBytes(b.GetBaseValue());
            
            ulong value = ByteCrossover(aByte, bByte);
            
            return new Seed(value,id,parentName);
        }
    }
}