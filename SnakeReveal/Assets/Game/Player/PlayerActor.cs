using Extensions;
using Game.Enums;
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
                ApplyRendererDirection();
            }
        }

        public int Speed => _speed;

        public int2 GridPosition
        {
            get => _gridPosition;
            set => _gridPosition = value;
        }

        protected void OnValidate()
        {
            ApplyRendererDirection();
        }

        public void ApplyGridPosition(SimulationGrid grid)
        {
            transform.SetLocalPositionXY(grid.GetScenePosition(GridPosition));
        }

        private void ApplyRendererDirection()
        {
            _renderer.ApplyDirection(Direction);
        }
    }
}