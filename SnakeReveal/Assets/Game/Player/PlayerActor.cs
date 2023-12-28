using Extensions;
using Game.Enums;
using Game.Lines;
using Game.Lines.Deprecated;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Player
{
    public class PlayerActor : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private PlayerActorRenderer _renderer;

        [SerializeField] private Vector2Int _position;

        [FormerlySerializedAs("_gridDirection")] [SerializeField]
        private GridDirection _direction = GridDirection.None;

        [SerializeField] private int _speed = 1;

        private PlayerActorControls _controls;

        [CanBeNull] private DeprecatedLine _currentLine;

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
            get => _position;
            set => _position = value;
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

        public void Step()
        {
            Position += Direction.ToInt2();
            Position = _grid.Clamp(Position);

            // todo: extrapolate grid position in Update() instead (this just applies the grid position to scene position for rendering
            ApplyPosition();
        }
    }
}