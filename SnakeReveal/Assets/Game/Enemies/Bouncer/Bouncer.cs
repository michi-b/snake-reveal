using System;
using System.Collections.Generic;
using Attributes;
using Extensions;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Enemies.Bouncer
{
    [RequireComponent(typeof(Renderer)), RequireComponent(typeof(Rigidbody2D))]
    public class Bouncer : MonoBehaviour
    {
        [SerializeField] private Material _regularMaterial;
        [SerializeField] private Material _inDrawnShapeMaterial;
        [SerializeField] private LayerMask _inDrawnShapeExcludedCollidersMask;

        private Rigidbody2D _rigidbody2D;
        private Rigidbody2D Rigidbody2D => _rigidbody2D ??= GetComponent<Rigidbody2D>();

        private Renderer _renderer;
        private Renderer Renderer => _renderer ??= GetComponent<Renderer>();

        [UnityEventTarget]
        public void OnIsCapturedInDrawnShapeChanged(bool isCaptured)
        {
            Renderer.sharedMaterial = isCaptured ? _inDrawnShapeMaterial : _regularMaterial;
            Rigidbody2D.excludeLayers = isCaptured ? _inDrawnShapeExcludedCollidersMask : new LayerMask();
        }
    }
}