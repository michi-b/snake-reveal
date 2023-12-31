using Game.Grid;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Lines.Colliders
{
    /// <summary>
    ///     Utility class for testing collisions with this transform in editor.
    ///     Not meant to be used at runtime, as it's not optimized at all.
    /// </summary>
    [RequireComponent(typeof(SimulationGridTransform))]
    public class CollisionTest : MonoBehaviour
    {
        [SerializeField] private SimulationGridTransform _transform;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private Collider2D[] _overlappingColliders;


        public LayerMask LayerMask
        {
            get => _layerMask;
            set => _layerMask = value;
        }

        private void Reset()
        {
            _transform = GetComponent<SimulationGridTransform>();
        }

        [PublicAPI("Targeted by UnityEvents")]
        public void Test()
        {
            _overlappingColliders = Physics2D.OverlapPointAll(transform.position, LayerMask);
        }
    }
}