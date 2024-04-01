using System;
using Game.Enums;
using Random = UnityEngine.Random;

namespace Game.Player.Controls
{
    public class MonkeyTestRandomInputPlayerActorControls : IPlayerActorControls
    {
        private bool _enabled;

        public void Activate()
        {
            _enabled = true;
        }

        public void Deactivate()
        {
            _enabled = false;
        }

        public GridDirection EvaluateRequestedDirection()
        {
            if (!_enabled)
            {
                return GridDirection.None;
            }

            return (GridDirection)Random.Range(1, 5); // 1 - 4, which is all directions except None
        }

        void IDisposable.Dispose()
        {
        }
    }
}