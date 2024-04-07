using Attributes;
using UnityEngine;
using Utility;

namespace Game.Settings
{
    [CreateAssetMenu(fileName = nameof(GamePlayerSettingsContainer), menuName = MenuUtility.Path + nameof(GamePlayerSettingsContainer), order = 0)]
    public class GamePlayerSettingsContainer : ScriptableObject
    {
        [SerializeField] private GamePlayerSettings _defaults;

        private GamePlayerSettings _loaded;

        public GamePlayerSettings Settings
        {
            get
            {
                EnsureIsLoaded();
                return _loaded;
            }
        }

        public void ResetToDefaults(bool saveToPreferences)
        {
            EnsureIsLoaded();

            _loaded.OverrideWithDefaults(_defaults);

            if (saveToPreferences)
            {
                _loaded.SaveToPlayerPrefs();
            }
        }

        public void RevertToSavedPreferences(bool saveToPreferences)
        {
            EnsureIsLoaded();

            _loaded.OverrideWithDefaults(_defaults);
            _loaded.OverrideWithPlayerPrefs();

            if (saveToPreferences)
            {
                _loaded.SaveToPlayerPrefs();
            }
        }

        private void EnsureIsLoaded()
        {
            _loaded ??= new GamePlayerSettings(_defaults);
        }

        public float SwipeThreshold
        {
            [UnityEventTarget]
            set
            {
                EnsureIsLoaded();
                _loaded.SwipeThreshold = value;
            }
        }
    }
}