using Extensions;
using Game.Simulation.Grid;
using UnityEngine;

namespace Game.Lines
{
    public class LineCache : SimpleCache<Line>
    {
        [SerializeField] private SimulationGrid _grid;

        public override Line Get()
        {
            Line line = base.Get();
            Transform lineTransform = line.transform;
            lineTransform.SetParent(transform);
            lineTransform.SetLocalPositionZ(0f);
            line.Initialize(_grid);
            return line;
        }
    }
}