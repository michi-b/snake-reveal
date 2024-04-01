using System;
using Game.Enums;
using Game.Enums.Extensions;
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

        public GridDirection GetDirectionChange(GridDirections availableDirections)
        {
            if (!_enabled)
            {
                return GridDirection.None;
            }

            var result = (GridDirection)Random.Range(1, 5); // 1 - 4, which is all directions except None
            return availableDirections.Contains(result) ? result : GridDirection.None;
        }

        void IDisposable.Dispose()
        {
        }
    }
}