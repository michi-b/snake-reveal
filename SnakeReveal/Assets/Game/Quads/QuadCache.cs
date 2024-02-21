using Game.Simulation.Grid;
using UnityEngine;

namespace Game.Quads
{
    public class QuadCache : SimpleCache<Quad>
    {
        [SerializeField] private SimulationGrid _grid;

        protected override Quad Instantiate()
        {
            Quad instance = base.Instantiate();
            instance.Initialize(_grid);
            return instance;
        }
    }
}