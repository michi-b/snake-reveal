using Extensions;
using UnityEngine;
using Utility;

namespace Game.Enemies
{
    public class SetStartVelocity : MonoBehaviour
    {
        [SerializeField] private Vector2 _startVelocity;

        public Vector2 StartVelocity
        {
            get => _startVelocity;
            set => _startVelocity = value;
        }

        protected virtual void Start()
        {
            GetComponent<Rigidbody2D>().velocity = StartVelocity;
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
    }
}