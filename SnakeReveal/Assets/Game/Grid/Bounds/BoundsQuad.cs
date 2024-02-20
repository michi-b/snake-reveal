using JetBrains.Annotations;
using UnityEngine;

namespace Game.Grid.Bounds
{
    public class BoundsQuad : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        [PublicAPI] public MeshFilter MeshFilter => _meshFilter ??= GetComponent<MeshFilter>();
        [PublicAPI] public MeshRenderer Renderer => _meshRenderer ??= GetComponent<MeshRenderer>();
    }
}