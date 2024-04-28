using Attributes;
using Game.Settings;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utility;

namespace Game.Gui.GameMenu
{
    public class GameMenuWindow : MonoBehaviour
    {
        [SerializeField] private GameSettingsContainer _settingsContainer;

        [FormerlySerializedAs("_swipeThreshold"), SerializeField]
        private Slider _swipeThresholdSlider;

        [SerializeField] private Toggle _displayDebugInfoToggle;


        private GameSettings _settings;

        protected void OnEnable()
        {
            _settings = _settingsContainer.Settings;
            Initialize();
        }

        protected void OnDisable()
        {
            _settings.Save();
            _settings = null;
        }

        private void Initialize()
        {
            _swipeThresholdSlider.value = _settings.SwipeThreshold;
            _displayDebugInfoToggle.isOn = _settings.DisplayDebugInfo;
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

        [UnityEventTarget]
        public void OnDisplayDebugInfoToggleChanged(bool value)
        {
            if (isActiveAndEnabled)
            {
                _settings.DisplayDebugInfo = value;
            }
        }

        [UnityEventTarget]
        public void AbortLevel()
        {
            ApplicationUtility.Quit(ExitCodes.Success);
        }
    }
}