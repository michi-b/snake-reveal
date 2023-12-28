using System.Collections.Generic;
using UnityEngine;

namespace Game.Lines
{
    public abstract class LineChainRenderer : MonoBehaviour
    {
        public const int InitialLineCapacity = 1000;
        public abstract void EditModeRebuild(IList<Vector2> points, bool loop);
    }
}