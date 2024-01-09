using Game.Enums;

namespace Game.Player.Controls
{
    public class MonkeyTestRandomInputPlayerActorControls : IPlayerActorControls
    {
        bool enabled = false;
        
        public void Activate()
        {
            enabled = true;
        }

        public void Deactivate()
        {
            enabled = false;
        }

        public GridDirection GetRequestedDirection()
        {
            if (!enabled)
            {
                return GridDirection.None;
            }
            
            return (GridDirection) UnityEngine.Random.Range(1, 5); // 1 - 4, which is all directions except None
        }
    }
}