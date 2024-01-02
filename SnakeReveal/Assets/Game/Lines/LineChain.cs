using JetBrains.Annotations;
using UnityEngine;

namespace Game.Lines
{
    public class LineChain : LineContainer
    {
        protected override bool Loop => false;
        protected override Color GizmosColor => new(1f, 0.5f, 0f);

        [PublicAPI("Allocation-free enumeration of all lines.")]
        public override LineEnumerator GetEnumerator()
        {
            return new LineEnumerator(Start, null);
        }
    }
}