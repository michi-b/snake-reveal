using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Settings
{
    [Serializable]
    public class GameSettings
    {
        [SerializeField] private GameSettingsContainer _container;

        private const string KeyPrefix = nameof(GameSettings) + ".";

        private const string SwipeThresholdKey = KeyPrefix + nameof(SwipeThreshold);
        private const string DisplayDebugInfoKey = KeyPrefix + nameof(DisplayDebugInfo);

        [SerializeField] private float _swipeThreshold = 0.5f;
        [SerializeField] private bool _displayDebugInfo;
        private HashSet<IGameSettingsObserver> _observers;

        public float SwipeThreshold
        {
            get => _swipeThreshold;
            set
            {
                _swipeThreshold = value;
                NotifyObservers();
            }
        }

        public bool DisplayDebugInfo
        {
            get => _displayDebugInfo;
            set
            {
                _displayDebugInfo = value;
                NotifyObservers();
            }
        }

        public GameSettings(GameSettings defaults)
        {
            _observers = new HashSet<IGameSettingsObserver>();

            _container = defaults._container;

            OverrideWithDefaults(defaults);
            OverrideWithPlayerPrefs();
        }

        public void OverrideWithDefaults(GameSettings defaults)
        {
            SwipeThreshold = defaults.SwipeThreshold;
            DisplayDebugInfo = defaults.DisplayDebugInfo;
        }

        public void OverrideWithPlayerPrefs()
        {
            _swipeThreshold = PlayerPrefs.GetFloat(SwipeThresholdKey, _swipeThreshold);
            _displayDebugInfo = PlayerPrefs.GetInt(DisplayDebugInfoKey, _displayDebugInfo ? 1 : 0) == 1;
        }

        public void SaveToPlayerPrefs()
        {
            PlayerPrefs.SetFloat(SwipeThresholdKey, _swipeThreshold);
            PlayerPrefs.SetInt(DisplayDebugInfoKey, _displayDebugInfo ? 1 : 0);
        }

        public void Register(IGameSettingsObserver observer, bool notifyImmediately = false)
        {
            _observers.Add(observer);
            if (notifyImmediately)
            {
                observer.OnGameSettingsChanged(this);
            }
        }

        public void Deregister(IGameSettingsObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers()
        {
            foreach (IGameSettingsObserver observer in _observers)
            {
                observer.OnGameSettingsChanged(this);
            }
        }
    }
}