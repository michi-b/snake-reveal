using Game.Enums;
using UnityEngine;

namespace Game.Player
{
    public class PlayerActorRenderer : MonoBehaviour
    {
        // [SerializeField] private SpriteRenderer _dot;
        [SerializeField] private SpriteRenderer _arrow;


        public void ApplyDirection(GridDirection direction)
        {
            bool isIdle = direction == GridDirection.None;
            _arrow.enabled = !isIdle;
            if (!isIdle)
            {
                UpdateRotation(direction);
            }
        }

        private void UpdateRotation(GridDirection direction)
        {
            transform.rotation = direction.GetRotation();
        }
    }
}