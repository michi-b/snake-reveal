namespace Game.Settings
{
    public interface IGameSettingsObserver
    {
        void OnGameSettingsChanged(GameSettings settings);
    }
}