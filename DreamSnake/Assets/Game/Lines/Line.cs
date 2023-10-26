using System;
using Extensions;
using Game.Simulation.Grid;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Lines
{
    [RequireComponent(typeof(Renderer))]
    public class Line : MonoBehaviour
    {
        [SerializeField] private Vector2 _spriteSize = new(0.02f, 0.02f);
        [SerializeField] private Vector2 _spriteSizeReciprocal = new(50f, 50f);

        private Renderer _renderer;

        public int2 Start { get; private set; }
        public int2 End { get; private set; }

        protected void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        protected void OnValidate()
        {
            _spriteSizeReciprocal = new Vector2(1f, 1f) / _spriteSize;
        }

        public void Place(SimulationGrid grid, int2 start, int2 end, float z)
        {
            Start = start;
            End = end;

            if (start.x == end.x && start.y == end.y)
            {
                Transform thisTransform = transform;
                transform.position = grid.GetWorldPosition(start, z);
                thisTransform.localScale = Vector3.one;
                _renderer.enabled = false;
                return;
            }

            Vector2 startScenePosition = grid.GetScenePosition(start);
            Vector2 endScenePosition = grid.GetScenePosition(end);
            Vector2 center = (startScenePosition + endScenePosition) * 0.5f;

            if (start.x == end.x)
            {
                float height = endScenePosition.y - startScenePosition.y;
                PlaceVertical(grid, center, height, z);
            }
            else if (start.y == end.y)
            {
                float width = endScenePosition.x - startScenePosition.x;
                PlaceHorizontal(grid, center, width, z);
            }
            else
            {
                throw new ArgumentException
                (
                    "Line must be either horizontal or vertical,"
                    + " but start and end positions are not aligned in neither x nor y."
                    + $" Start={start}; End={end}."
                );
            }

            _renderer.enabled = true;
        }

        private void PlaceHorizontal(SimulationGrid grid, Vector2 center, float width, float z)
        {
            Transform thisTransform = transform;
            thisTransform.localScale = new Vector3(width * _spriteSizeReciprocal.x, 1f, 1f);
            thisTransform.position = center.ToVector3(z);
        }

        private void PlaceVertical(SimulationGrid grid, Vector2 center, float height, float z)
        {
            Transform thisTransform = transform;
            thisTransform.localScale = new Vector3(1f, height * _spriteSizeReciprocal.y, 1f);
            thisTransform.position = center.ToVector3(z);
        }
    }
}