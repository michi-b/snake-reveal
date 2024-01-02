using UnityEngine;

namespace Game.Lines
{
    public class LineChain : LineContainer 
    {
        protected override bool Loop => false;
        protected override Color GizmosColor => new Color(1f, 0.5f, 0f);
    }
}