using System;
using UnityEngine;

namespace Game.Lines.Colliders
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LineCollider : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D _collider;

        [SerializeField] private LineContainer _container;
        [SerializeField] private int _index = -1;
        
        
        protected void Reset()
        {
            _collider = GetComponent<BoxCollider2D>();
        }
    }
}
