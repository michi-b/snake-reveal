using Game.Enums;
using Game.Enums.Extensions;
using Random = UnityEngine.Random;

namespace Game.Player.Controls
{
    public class MonkeyTestRandomInputPlayerActorControls : IPlayerActorControls
    {
        GridDirection IPlayerActorControls.GetDirectionChange(GridDirections availableDirections)
        {
            if (!IsEnabled)
            {
                return GridDirection.None;
            }

            var result = (GridDirection)Random.Range(1, 5); // 1 - 4, which is all directions except None
            return availableDirections.Contains(result) ? result : GridDirection.None;
        }

        public bool IsEnabled { get; set; }

        void IPlayerActorControls.Destroy()
        {
        }
    }
}