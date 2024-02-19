using Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class IsCapturedInDrawnShapeDetector : MonoBehaviour
    {
        [SerializeField] private UnityEvent<bool> _onIsCapturedChanged;
        private int _captureEnterContactCount;
        private int _captureMaintainContactCount;
        private bool _isCaptured;

        [PublicAPI]
        public bool IsCaptured
        {
            get => _isCaptured;
            private set
            {
                Debug.Assert(_isCaptured != value);
                _isCaptured = value;
                _onIsCapturedChanged?.Invoke(value);
            }
        }

        protected void OnTriggerEnter2D(Collider2D other)
        {
            bool entersCapture = ContactCaptures(other);
            if (entersCapture)
            {
                _captureEnterContactCount++;
            }

            if (ContactMaintainsCapture(other))
            {
                _captureMaintainContactCount++;
            }

            if (!IsCaptured && entersCapture && CapturesPivot(other))
            {
                IsCaptured = true;
            }
        }

        protected void OnTriggerExit2D(Collider2D other)
        {
            if (ContactCaptures(other))
            {
                _captureEnterContactCount--;
            }

            if (ContactMaintainsCapture(other))
            {
                _captureMaintainContactCount--;
            }

            if (IsCaptured && _captureEnterContactCount == 0 && _captureMaintainContactCount == 0)
            {
                IsCaptured = false;
            }
        }

        protected void OnTriggerStay2D(Collider2D contact)
        {
            if (!IsCaptured && ContactCaptures(contact) && CapturesPivot(contact))
            {
                IsCaptured = true;
            }
        }

        private static bool ContactCaptures(Component other) => GameSettings.instance.IsCapturedInDrawnCheckEnterLayers.Contains(other.gameObject.layer);
        private static bool ContactMaintainsCapture(Component other) => GameSettings.instance.CaptureMaintainLayers.Contains(other.gameObject.layer);

        private bool CapturesPivot(Collider2D captureContact) => captureContact.OverlapPoint(transform.position);
    }
}