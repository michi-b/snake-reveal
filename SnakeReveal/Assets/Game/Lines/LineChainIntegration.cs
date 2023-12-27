using Unity.Mathematics;
using UnityEngine;

namespace Game.Lines
{
    public static class LineChainIntegration
    {
        public static void Integrate(LineChain loop, LineChain chain)
        {
            Debug.Assert(loop.Loop, "Loop must be a loop");
            Debug.Assert(!chain.Loop, "Chain must not be a loop");
            
            // int2 chainStartPosition = chain[0].Position;
            // int2 chainEndPosition = chain[^1].Position;
            
            // int 
        }
    }
}
