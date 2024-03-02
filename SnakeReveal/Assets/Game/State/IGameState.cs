namespace Game.State
{
    public interface IGameState
    {
        IGameState FixedUpdate();
        public GameStateId Id { get; }
    }
}