using UnityEngine;

namespace Game.Grid.Bounds
{
    public class GridBounds : MonoBehaviour
    {
        [SerializeField] private BoundsPaddingContainer _padding;
        [SerializeField] private BoundsColliderContainer _colliders;
    }
}