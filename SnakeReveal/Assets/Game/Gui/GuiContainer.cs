using UnityEngine;

namespace Game.Gui
{
    public class GuiContainer : MonoBehaviour
    {
        [SerializeField] private GameInfo.GameInfoGui _gameInfo;
        [SerializeField] private GameMenu.GameMenu _gameMenu;
        [SerializeField] private AvailableDirectionsIndication.AvailableDirectionsIndication _availableDirectionsIndication;
        [SerializeField] private DebugInfo.DebugInfoGui _debugInfo;

        public GameInfo.GameInfoGui GameInfo => _gameInfo;
        public GameMenu.GameMenu GameMenu => _gameMenu;

        public AvailableDirectionsIndication.AvailableDirectionsIndication AvailableDirectionsIndication => _availableDirectionsIndication;
        public DebugInfo.DebugInfoGui DebugInfo => _debugInfo;
    }
}