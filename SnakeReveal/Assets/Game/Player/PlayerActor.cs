using Extensions;
using Game.Enums;
using Game.Grid;
using Game.Lines;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(SimulationGridTransform))]
    public class PlayerActor : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private SimulationGridTransform _transform;
        [SerializeField] private PlayerActorRenderer _renderer;
        [SerializeField] private GridDirection _direction = GridDirection.None;
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

        public Vector2Int Position
        {
            get => _transform.Position;
            set => _transform.Position = value;
        }

        protected void Reset()
        {
            _transform = GetComponent<SimulationGridTransform>();
        }

        protected void OnValidate()
        {
            ApplyRendererDirection();
        }

        public void ApplyPosition()
        {
            transform.SetLocalPositionXY(_grid.GetScenePosition(Position));
        }

        private void ApplyRendererDirection()
        {
            _renderer.ApplyDirection(Direction);
        }

        public void Move()
        {
            Position += Direction.ToVector2Int();
            
            // todo: no need for clamping once grid edge travelling is implemented
            Position = _grid.Clamp(Position);

            // todo: extrapolate grid position in Update() instead (this just applies the grid position to scene position for rendering
            ApplyPosition();
        }
    }
}