﻿using Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Simulation.Grid.Bounds
{
    public class BoundsQuad : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        [PublicAPI] public MeshFilter MeshFilter => _meshFilter ??= GetComponent<MeshFilter>();
        [PublicAPI] public MeshRenderer Renderer => _meshRenderer ??= GetComponent<MeshRenderer>();

        public void Place(Vector2 center, Vector2 size)
        {
            Transform thisTransform = transform;
            thisTransform.SetWorldPositionXY(center);
            thisTransform.ClearLocalZ();
            thisTransform.localScale = new Vector3(size.x, size.y, 1f);
        }
    }
}