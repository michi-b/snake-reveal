using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Lines
{
    public class LineChain : LineContainer
    {
        public const string LastFieldName = nameof(_end);

        [FormerlySerializedAs("_last"), SerializeField, HideInInspector]
        private Line _end;

        public override bool Loop => false;
        public new Line Start => base.Start;
        public override Line End => _end;

        protected override Color GizmosColor => new(1f, 0.5f, 0f);

        protected override void PostProcessEditModeLineChanges()
        {
            Line current = Start;
            while (current.Next != null)
            {
                current = current.Next;
            }
            _end = current;
            base.PostProcessEditModeLineChanges();
        }

        public LineSpan AsSpan(bool excludeLast)
        {
            return excludeLast
                ? new LineSpan(Start, End)
                : AsSpan();
        }
    }
}