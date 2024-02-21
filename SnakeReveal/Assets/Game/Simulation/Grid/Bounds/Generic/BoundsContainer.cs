using Game.Enums;
using Generic;
using UnityEngine;

namespace Game.Simulation.Grid.Bounds.Generic
{
    public class BoundsContainer<TQuad> : MonoBehaviour where TQuad : BoundsQuad
    {
        [SerializeField] private EnumIndexed<GridSide, TQuad> _quads;

        public TQuad this[GridSide direction] => _quads[direction];

        public void SetSize(Vector2 holeSize, float thickness)
        {
            Vector2 centerDistance = (holeSize + new Vector2(thickness, thickness)) * 0.5f;

            Vector2 topCenter = new(0f, centerDistance.y);
            Vector2 rightCenter = new(centerDistance.x, 0f);
            Vector2 bottomCenter = new(0f, -centerDistance.y);
            Vector2 leftCenter = new(-centerDistance.x, 0f);

            Vector2 verticalQuadSize = new(thickness, holeSize.y);
            Vector2 horizontalQuadSize = new(holeSize.x + thickness * 2f, thickness);

            _quads[GridSide.Bottom].Place(bottomCenter, horizontalQuadSize);
            _quads[GridSide.Left].Place(leftCenter, verticalQuadSize);
            _quads[GridSide.Top].Place(topCenter, horizontalQuadSize);
            _quads[GridSide.Right].Place(rightCenter, verticalQuadSize);
        }
    }
}