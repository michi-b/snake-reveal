using Game.Enums;

namespace Game.Simulation
{
    public interface IPlayerSimulationState
    {
        /// <summary>
        /// Name of the state for the debug info gui
        /// </summary>
        string Name { get; }

        /// <summary>Tick the simulation (once per fixed update, while this state is active)</summary>
        /// <param name="result"></param>
        /// <returns>The new current player simulation state, or just this if it did not change</returns>
        public IPlayerSimulationState Update(ref SimulationUpdateResult result);

        /// <summary>
        /// Evaluates the currently available directions for the player to move in.
        /// This is used by <see cref="Game.WaitingForSimulationInputState"/> to know in which direction continuation is possible.
        /// </summary>
        /// <returns>Currently available directions for the player to move in</returns>
        public GridDirections GetAvailableDirections();

        void Resume()
        {
        }
    }
}