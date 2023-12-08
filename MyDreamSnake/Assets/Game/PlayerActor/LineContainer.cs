using Game.Lines;
using Game.Simulation.Grid;
using Unity.Mathematics;
using UnityEngine;

namespace Game.PlayerActor
{
    public class LineContainer : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;

        public void Place(Line line, int2 start, int2 end)
        {
            Transform thisTransform = transform;
            line.transform.parent = thisTransform;
            line.Place(_grid, start, end, thisTransform.position.z);
        }
    }
}
