using UnityEngine;

namespace Game.Lines
{
    public class LineLoop : LineContainer
    {
        protected override bool Loop => true;
        protected override Color GizmosColor => new(0f, 0.5f, 1f);
    }
}