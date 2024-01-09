using Extensions;
using Game.Grid;
using Game.Lines;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.Polygon
{
    public class DrawnShapePolygon : MonoBehaviour
    {
        [SerializeField] private SpriteShapeController _shape;

        [SerializeField, ContextMenuItem("EvaluateInverseScale", nameof(EvaluateInverseScale))]
        private float _inverseScale;

        public void Apply(SimulationGrid grid, LineSpan lines)
        {
            _shape.spline.Clear();

            int currentIndex = 0;
            foreach (Line line in lines)
            {
                Vector2 scenePosition = line.Start.GetScenePosition(grid);
                Vector2 localPosition = scenePosition * _inverseScale;
                _shape.spline.InsertPointAt(currentIndex++, localPosition);
            }
        }

        private void EvaluateInverseScale()
        {
            _inverseScale = 1f / transform.localScale.x;
        }
    }
}