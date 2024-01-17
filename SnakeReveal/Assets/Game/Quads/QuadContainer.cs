using System.Collections.Generic;
using UnityEngine;

namespace Game.Quads
{
    public class QuadContainer : MonoBehaviour
    {
        [SerializeField] private QuadCache _cache;
        [SerializeField] private List<Quad> _quads;
    }
}