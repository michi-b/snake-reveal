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

        public void Clear()
        {
            if (_start != null)
            {
                foreach (Line line in this)
                {
                    Cache.Return(line);
                }

                _start = _end = null;
            }
        }

        public void Set(LineData lineData)
        {
            Clear();
            Line line = GetNewLine(lineData);
            _start = _end = line;
        }

        /// <returns>whether a new line was added (a turn was needed)</returns>
        public bool Extend(Vector2Int actorPosition)
        {
            bool isTurn = !End.TryExtend(actorPosition);
            if (isTurn)
            {
                Line newEnd = GetNewLine(End.End, actorPosition);

                newEnd.Previous = _end;
                End.Next = newEnd;

                _end = newEnd;
                End.Validate();
            }

            return isTurn;
        }
    }
}