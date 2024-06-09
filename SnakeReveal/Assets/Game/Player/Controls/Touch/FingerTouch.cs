using UnityEngine;

namespace Game.Player.Controls.Touch
{
    public class FingerTouch
    {
        public bool HasSwiped { get; private set; }

        public void OnFingerUp()
        {
            HasSwiped = false;
        }

        public void ConsumeSwipe()
        {
            HasSwiped = true;
        }

        public void Reset()
        {
            HasSwiped = false;
        }
    }
}