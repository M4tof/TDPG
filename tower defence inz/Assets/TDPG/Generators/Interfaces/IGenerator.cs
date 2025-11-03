namespace TDPG.Generators.Interfaces
{
    public interface IGenerator<T>
    {
        T Generate(IRandomSource source);
        T Generate(Seed.Seed seed, string context = null);
        T Preview(ulong pseudo);
    }
}
