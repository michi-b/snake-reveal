using Game.Enums;

namespace Game.Player.Controls
{
    public interface IPlayerActorControls
    {
        public GridDirection GetDirectionChange(GridDirections availableDirections);
        public bool IsEnabled { get; set; }
        void Destroy();
    }
}