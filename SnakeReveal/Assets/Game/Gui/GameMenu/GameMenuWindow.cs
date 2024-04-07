using Attributes;
using Game.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gui.GameMenu
{
    public class GameMenuWindow : MonoBehaviour
    {
        [SerializeField] private GamePlayerSettingsContainer _settingsContainer;
        [SerializeField] private Slider _swipeThreshold;

        private GamePlayerSettings _settings;

        protected void OnEnable()
        {
            _settings = _settingsContainer.Settings;
            Initialize();
        }

        protected void OnDisable()
        {
            _settings.SaveToPlayerPrefs();
            _settings = null;
        }

        private void Initialize()
        {
            _swipeThreshold.value = _settings.SwipeThreshold;
        }

        [UnityEventTarget]
        public void RevertSettingsToDefaults()
        {
            _settingsContainer.ResetToDefaults(!isActiveAndEnabled);
            Initialize();
        }

        [UnityEventTarget]
        public void RevertSettingsToSavedPreferences()
        {
            _settingsContainer.RevertToSavedPreferences(!isActiveAndEnabled);
            Initialize();
        }
    }
}