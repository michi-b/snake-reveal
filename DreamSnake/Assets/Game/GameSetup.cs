using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class GameSetup : MonoBehaviour
    {
        [SerializeField] private GameArguments _arguments;

        [SerializeField] private Button _playButton;

        protected void OnEnable()
        {
            if (_arguments.PlayOnEnable)
            {
                _playButton.onClick.Invoke();
            }
        }
    }
}
