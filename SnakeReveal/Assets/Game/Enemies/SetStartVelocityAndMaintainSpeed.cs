using UnityEngine;

namespace Game.Enemies
{
    public class SetStartVelocityAndMaintainSpeed : SetStartVelocity
    {
        [SerializeField] private Vector2 _startVelocity;

        private Rigidbody2D _rigidbody;


        private float _speed;
        private Rigidbody2D Rigidbody => _rigidbody ??= GetComponent<Rigidbody2D>();

        protected override void Start()
        {
            _speed = _startVelocity.magnitude;
            base.Start();
        }

        protected virtual void FixedUpdate()
        {
            Rigidbody2D thisRigidbody = Rigidbody;
            thisRigidbody.velocity = thisRigidbody.velocity.normalized * _speed;
        }
    }
}