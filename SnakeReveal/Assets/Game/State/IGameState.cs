namespace Game.State
{
    public interface IGameState
    {
        IGameState FixedUpdate();
        string Name { get; }
    }
}