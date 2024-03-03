using TextDisplay.Abstractions;
using UnityEngine;

namespace Game.Gui.GameInfo
{
    public class GameInfoGui : MonoBehaviour
    {
        [SerializeField] private FloatDisplay _percentCompletionDisplay;

        [SerializeField] private LevelCompleteMessage.LevelCompleteMessage _levelLevelCompleteMessage;

        public float PercentCompletion
        {
            set => _percentCompletionDisplay.Value = value;
        }

        public LevelCompleteMessage.LevelCompleteMessage LevelCompleteMessage => _levelLevelCompleteMessage;
    }
}