using Extensions;
using UnityEngine;
using Utility;

namespace Game.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SetStartVelocity : MonoBehaviour
    {
        [SerializeField] private Vector2 _startVelocity;

        public const string StartVelocityPropertyName = nameof(_startVelocity);

        public Vector2 StartVelocity
        {
            get => _startVelocity;
            set => _startVelocity = value;
        }

        protected virtual void Start()
        {
            Rigidbody.velocity = StartVelocity;
        }

        protected void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Color originalGizmosColor = Gizmos.color;
                Gizmos.color = Color.red;

                Vector3 position = transform.position;
                GizmosUtility.DrawArrow(position, position + _startVelocity.ToVector3(0f));

                Gizmos.color = originalGizmosColor;
            }
        }

        private Rigidbody2D _rigidbody;
        protected Rigidbody2D Rigidbody => _rigidbody ??= GetComponent<Rigidbody2D>();
    }
}