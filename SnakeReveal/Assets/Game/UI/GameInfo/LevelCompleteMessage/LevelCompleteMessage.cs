using Attributes;
using TextDisplay.Abstractions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI.GameInfo.LevelCompleteMessage
{
    public class LevelCompleteMessage : MessageBox.MessageBox
    {
        [SerializeField] private FloatDisplay _seconds;
        [SerializeField] private FloatDisplay _coverage;

        public float Seconds
        {
            set => _seconds.Value = value;
        }

        public float Coverage
        {
            set => _coverage.Value = value;
        }

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
    }
}