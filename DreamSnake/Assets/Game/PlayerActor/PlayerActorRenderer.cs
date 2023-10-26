using Game.Simulation.Grid;
using UnityEngine;

namespace Game.PlayerActor
{
    public class PlayerActorRenderer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _idle;
        [SerializeField] private SpriteRenderer _moving;
        

        public void ApplyDirection(GridDirection direction)
        {
            bool isIdle = direction == GridDirection.None;
            _idle.enabled = isIdle;
            _moving.enabled = !isIdle;
            if (!isIdle)
            {
                UpdateRotation(direction);
            }
        }

        private void UpdateRotation(GridDirection direction)
        {
            transform.rotation = direction.ToRotation();
        }
    }
}
