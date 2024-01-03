using System.Linq;
using UnityEngine;

namespace Game.Lines
{
    public class LineChain : LineContainer
    {
        public const string LastFieldName = nameof(_last);

        [SerializeField, HideInInspector] private Line _last;

        public new Line Start => base.Start;
        public Line Last => _last;

        protected override Color GizmosColor => new(1f, 0.5f, 0f);

        protected override Line ExclusiveEnd => null;

        protected override void PostProcessLineChanges()
        {
            _last = this.Last();
        }
    }
}