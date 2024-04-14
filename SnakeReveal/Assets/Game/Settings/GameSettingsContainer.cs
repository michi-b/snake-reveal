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

            _settings.ApplyDefaults(_defaults);

            if (saveToPreferences)
            {
                _settings.Save();
            }
        }

        public void RevertToSavedPreferences(bool saveToPreferences)
        {
            EnsureIsLoaded();

            _settings.ApplyDefaults(_defaults);
            _settings.ApplyPlayerPrefs();

            if (saveToPreferences)
            {
                _settings.Save();
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