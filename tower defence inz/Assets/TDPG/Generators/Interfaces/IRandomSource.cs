namespace TDPG.Generators.Interfaces
{
    public interface IRandomSource
    {
        ulong NextUInt64();
        double NextFloat();
        IRandomSource Clone();
    }
}
