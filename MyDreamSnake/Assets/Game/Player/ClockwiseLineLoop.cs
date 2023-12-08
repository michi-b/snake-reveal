using Game.Lines;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Player
{
    
    public class ClockwiseLineLoop : MonoBehaviour
    {
        public Line Start { get; private set; }

        public void Set(SimulationGrid grid, LineCache lineCache, params int2[] positions)
        {
#if DEBUG
            Debug.Assert(Start == null);
            Debug.Assert(positions.Length >= 4);
#endif

            Line previous = null;
            for (int index = 0; index < positions.Length; index++)
            {
                int2 start = positions[index];
                int2 end = positions[(index + 1) % positions.Length];
                Line line = lineCache.Get();
                line.Place(grid, start, end);
                if (previous != null)
                {
                    previous.Next = line;
                    line.Previous = previous;
                }
                else
                {
                    Start = line;
                }

                previous = line;
            }

            Start.Previous = previous;
            // ReSharper disable once PossibleNullReferenceException because this is not possible due to assertion of positions.Length
            previous.Next = Start;
        }
    }
}