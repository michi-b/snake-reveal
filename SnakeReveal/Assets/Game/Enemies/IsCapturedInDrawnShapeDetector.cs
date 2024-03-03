using System;
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
        [SerializeField] private LayerMask _captureEnterLayers;
        [SerializeField] private LayerMask _captureMaintainLayers;

        private int _captureEnterContactCount;
        private int _captureMaintainContactCount;
        private bool _isCaptured;

        protected void Reset()
        {
            _captureEnterLayers = LayerMask.GetMask("ShapeQuads");
            _captureMaintainLayers = LayerMask.GetMask("ShapeQuads", "Enemies");
        }

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
            bool isCaptureEnterContact = IsCapturingContact(other);
            if (isCaptureEnterContact)
            {
                _captureEnterContactCount++;
            }

            if (IsCaptureMaintainingContact(other))
            {
                _captureMaintainContactCount++;
            }

            if (!IsCaptured && isCaptureEnterContact && CapturesPivot(other))
            {
                IsCaptured = true;
            }
        }

        protected void OnTriggerExit2D(Collider2D other)
        {
            if (IsCapturingContact(other))
            {
                _captureEnterContactCount--;
            }

            if (IsCaptureMaintainingContact(other))
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
            if (!IsCaptured && IsCapturingContact(contact) && CapturesPivot(contact))
            {
                IsCaptured = true;
            }
        }

        private bool IsCapturingContact(Component other) => _captureEnterLayers.Contains(other.gameObject.layer);
        private bool IsCaptureMaintainingContact(Component other) => _captureMaintainLayers.Contains(other.gameObject.layer);
        private bool CapturesPivot(Collider2D captureContact) => captureContact.OverlapPoint(transform.position);
    }
}