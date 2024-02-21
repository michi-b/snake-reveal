using UnityEngine;

namespace Game.Simulation.Grid.Bounds
{
    public class GridBounds : MonoBehaviour
    {
        [SerializeField] private BoundsPaddingContainer _padding;
        [SerializeField] private BoundsColliderContainer _colliders;

        public BoundsColliderContainer Colliders => _colliders;

        public BoundsPaddingContainer Padding => _padding;
    }
}