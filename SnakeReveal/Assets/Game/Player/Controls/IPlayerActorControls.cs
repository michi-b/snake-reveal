using System;
using Game.Enums;

namespace Game.Player.Controls
{
    public interface IPlayerActorControls : IDisposable
    {
        void Activate();
        public GridDirection GetDirectionChange(GridDirections availableDirections);
    }
}