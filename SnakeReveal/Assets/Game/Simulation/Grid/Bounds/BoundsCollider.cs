using JetBrains.Annotations;
using UnityEngine;

namespace Game.Simulation.Grid.Bounds
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(BoxCollider2D))]
    public class BoundsCollider : BoundsQuad
    {
        [PublicAPI] public BoxCollider2D Collider => _collider ??= GetComponent<BoxCollider2D>();

        private BoxCollider2D _collider;
    }
}