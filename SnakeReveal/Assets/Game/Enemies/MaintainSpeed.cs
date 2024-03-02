using UnityEngine;

namespace Game.Enemies
{
    public class MaintainSpeed : SetStartVelocity
    {
        [SerializeField] private float _speed;
        [SerializeField] private float _enforceSpeedThreshold = 0.01f;

        public const string SpeedPropertyName = nameof(_speed);
        public const string EnforceSpeedThresholdPropertyName = nameof(_enforceSpeedThreshold);

        public float EnforceSpeedThreshold
        {
            get => _enforceSpeedThreshold;
            set => _enforceSpeedThreshold = value;
        }

        public float Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                StartVelocity = StartVelocity.normalized * _speed;
            }
        }

        protected virtual void Reset()
        {
            Speed = StartVelocity.magnitude;
        }

        protected virtual void FixedUpdate()
        {
            float currentSpeed = Rigidbody.velocity.magnitude;
            if (currentSpeed == 0)
            {
                currentSpeed = float.Epsilon;
            }

            if (Mathf.Abs(currentSpeed - Speed) > EnforceSpeedThreshold)
            {
                Rigidbody.velocity = Rigidbody.velocity / currentSpeed * Speed;
            }
        }
    }
}