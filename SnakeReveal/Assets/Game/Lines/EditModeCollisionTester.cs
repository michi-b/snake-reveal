using Game.Simulation.Grid;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Lines
{
    public class EditModeCollisionTester : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;

        // ReSharper disable once NotAccessedField.Local
        // this serialized referenced is just abused to display overlaps in the inspector
        [SerializeField] private Collider2D[] _overlappingColliders;

        public LayerMask LayerMask
        {
            get => _layerMask;
            set => _layerMask = value;
        }

        private void Reset()
        {
            GetComponent<GridPlacement>();
        }

        [PublicAPI("Targeted by UnityEvents")]
        public void EditModeTest()
        {
            Reset();
            // ReSharper disable once Unity.PreferNonAllocApi
            // This is currently only used for debugging in editor
            _overlappingColliders = Physics2D.OverlapPointAll(transform.position, LayerMask);
        }
    }
}