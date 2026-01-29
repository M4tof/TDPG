namespace TDPG.Templates.Grid.MapGen
{
    /// <summary>
    /// Defines the noise algorithm and terrain characteristics used during generation.
    /// </summary>
    public enum MapTypes
    {
        /// <summary>
        /// Standard generation using OpenSimplex2 noise. 
        /// <br/>Creates rolling, organic terrain suitable for standard maps.
        /// </summary>
        Smooth,
        
        /// <summary>
        /// Uses Ridged Fractal noise. 
        /// <br/>Creates sharp contrasts, resulting in narrow corridors, distinct islands, or cave-like structures.
        /// </summary>
        Mountainous,
        
        /// <summary>
        /// Uses PingPong noise with high frequency. 
        /// <br/>Creates scattered, noisy terrain with many small obstacles and water pockets.
        /// </summary>
        Chaotic,
        
        /// <summary>
        /// Bypasses procedural noise and loads a hardcoded matrix from <see cref="DeterministicMap"/>.
        /// <br/>Useful for unit testing pathfinding or consistent debugging.
        /// </summary>
        Static
    }
}