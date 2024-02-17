using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class IsCapturedInDrawnShapeDetector : MonoBehaviour
    {
        [SerializeField] private UnityEvent<bool> _onIsCapturedChanged;
        private int _contactCount;
        private bool _isCaptured;

        [PublicAPI]
        public bool IsCaptured
        {
            get => _isCaptured;
            set
            {
                if (_isCaptured != value)
                {
                    _isCaptured = value;
                    _onIsCapturedChanged?.Invoke(value);
                }
            }
        }

        protected void OnTriggerEnter2D(Collider2D other)
        {
            if (!GetIsDrawnShape(other))
            {
                return;
            }

            _contactCount++;
            CheckIsCaptured(other);
        }

        protected void OnTriggerExit2D(Collider2D other)
        {
            if (!GetIsDrawnShape(other))
            {
                return;
            }

            _contactCount--;
            Debug.Assert(_contactCount >= 0);
            if (_contactCount == 0)
            {
                IsCaptured = false;
            }
        }

        protected void OnTriggerStay2D(Collider2D contact)
        {
            if (!GetIsDrawnShape(contact))
            {
                return;
            }

            CheckIsCaptured(contact);
        }

        private static bool GetIsDrawnShape(Component other) => other.gameObject.layer == GameSettings.instance.IsCapturedInDrawnShapeCheckLayer;

        private void CheckIsCaptured(Collider2D contact)
        {
            if (!IsCaptured && contact.OverlapPoint(transform.position))
            {
                IsCaptured = true;
            }
        }
    }
}