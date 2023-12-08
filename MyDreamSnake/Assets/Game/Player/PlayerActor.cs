using Game.Lines;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Player
{
    public class PlayerActor : MonoBehaviour
    {
        [SerializeField] private PlayerActorRenderer _renderer;

        [SerializeField] private int2 _gridPosition;

        [FormerlySerializedAs("_gridDirection")] [SerializeField]
        private GridDirection _direction = GridDirection.None;

        [SerializeField] private int _speed = 1;

        private PlayerActorControls _controls;

        [CanBeNull] private Line _currentLine;

        public GridDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                UpdateRendererDirection();
            }
        }

        public int Speed => _speed;

        public int2 GridPosition
        {
            get => _gridPosition;
            set => _gridPosition = value;
        }

        protected virtual void Awake()
        {
            _controls = new PlayerActorControls(this);
        }

        protected virtual void OnEnable()
        {
            _controls.Enable();
        }

        protected virtual void OnDisable()
        {
            _controls.Disable();
        }

        protected void OnValidate()
        {
            UpdateRendererDirection();
        }

        private void UpdateRendererDirection()
        {
            _renderer.ApplyDirection(Direction);
        }
    }
}