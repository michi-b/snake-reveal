using System.Collections.Generic;
using UnityEngine;

namespace Game.Lines
{
    public abstract class LineChainRenderer : MonoBehaviour
    {
        /// <summary>
        ///     Ditch current rendering objects and rebuild them from scratch.
        /// </summary>
        /// <param name="lines">
        ///     All lines that need to be rendered,
        ///     including the loop closing line if it is looping.
        /// </param>
        public abstract void EditModeRebuild(SimulationGrid grid, IReadOnlyList<Line> lines);
    }
}