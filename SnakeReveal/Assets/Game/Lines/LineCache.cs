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
            line.Initialize(_grid);
            return line;
        }
    }
}