using Attributes;
using UnityEngine;
using Utility;

namespace Game.Settings
{
    [CreateAssetMenu(fileName = nameof(GameSettingsContainer), menuName = MenuUtility.Path + nameof(GameSettingsContainer), order = 0)]
    public class GameSettingsContainer : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private GameSettings _defaults;

        private GameSettings _settings;

        public GameSettings Settings
        {
            get
            {
                EnsureIsLoaded();
                return _settings;
            }
        }

        public void ResetToDefaults(bool saveToPreferences)
        {
            EnsureIsLoaded();

            _settings.OverrideWithDefaults(_defaults);

            if (saveToPreferences)
            {
                _settings.SaveToPlayerPrefs();
            }
        }

        public void RevertToSavedPreferences(bool saveToPreferences)
        {
            EnsureIsLoaded();

            _settings.OverrideWithDefaults(_defaults);
            _settings.OverrideWithPlayerPrefs();

            if (saveToPreferences)
            {
                _settings.SaveToPlayerPrefs();
            }
        }

        private void EnsureIsLoaded()
        {
            _settings ??= new GameSettings(_defaults);
        }

        public float SwipeThreshold
        {
            [UnityEventTarget]
            set
            {
                EnsureIsLoaded();
                _settings.SwipeThreshold = value;
            }
        }

        public bool DisplayDebugInfo
        {
            [UnityEventTarget]
            set
            {
                EnsureIsLoaded();
                _settings.DisplayDebugInfo = value;
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _settings = null;
        }
    }
}