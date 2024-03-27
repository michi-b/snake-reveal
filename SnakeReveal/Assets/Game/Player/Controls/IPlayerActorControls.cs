using Game.Enums;

namespace Game.Player.Controls
{
    public interface IPlayerActorControls
    {
        void Activate();
        void Deactivate();
        public GridDirection EvaluateRequestedDirection();
    }
}