using Attributes;
using TextDisplay.Abstractions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI.GameInfo.LevelCompleteMessage
{
    public class LevelCompleteMessage : MessageBox.MessageBox
    {
        [SerializeField] private FloatDisplay _secondsDisplay;
        [SerializeField] private FloatDisplay _coverageDisplay;

        [UnityEventTarget]
        public void OnRestartButtonClicked()
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            IsVisible = false;
        }

        [UnityEventTarget]
        public void OnCloseAppButtonClicked()
        {
            IsVisible = false;
            Application.Quit(ExitCodes.Success);
        }

        public void Show(float seconds, float coverage)
        {
            _secondsDisplay.Value = seconds;
            _coverageDisplay.Value = coverage;
            IsVisible = true;
        }
    }
}