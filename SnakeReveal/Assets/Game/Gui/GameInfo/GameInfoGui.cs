using TextDisplay.Abstractions;
using UnityEngine;

namespace Game.Gui.GameInfo
{
    public class GameInfoGui : MonoBehaviour
    {
        [SerializeField] private FloatDisplay _percentCompletionDisplay;
    
        public float PercentCompletion
        {
            set => _percentCompletionDisplay.Value = value;
        }
    }
}
