using Game.Enums;
using Generic;
using UnityEngine;

namespace Game.Grid.Bounds.Generic
{
    public class BoundsContainer<TQuad> : MonoBehaviour where TQuad : BoundsQuad
    {
        [SerializeField] private EnumIndexed<GridDirection, TQuad> _quads;

        public TQuad this[GridDirection direction] => _quads[direction];
    }
}