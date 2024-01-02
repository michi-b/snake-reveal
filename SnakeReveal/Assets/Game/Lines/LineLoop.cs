using JetBrains.Annotations;
using UnityEngine;

namespace Game.Lines
{
    public class LineLoop : LineContainer
    {
        protected override bool Loop => true;
        
        protected override Color GizmosColor => new(0f, 0.5f, 1f);
        
        [PublicAPI("Allocation-free enumeration of all lines.")]
        public override LineEnumerator GetEnumerator()
        {
            return new LineEnumerator(Start, Start);
        }
    }
}