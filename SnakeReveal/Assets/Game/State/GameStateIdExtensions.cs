using System;

namespace Game.State
{
    public static class GameStateIdExtensions
    {
        public static string GetDisplayName(this GameStateId state)
        {
            return state switch
            {
                GameStateId.GameMenu => "Game Menu",
                GameStateId.SimulationRunning => "Simulation Running",
                GameStateId.WaitingForSimulationInput => "Waiting For Simulation Input",
                GameStateId.LevelComplete => "Level Complete",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        public static bool GetIsGameMenuAvailable(this GameStateId state)
        {
            return state switch
            {
                GameStateId.GameMenu => true,
                GameStateId.SimulationRunning => true,
                GameStateId.WaitingForSimulationInput => true,
                GameStateId.LevelComplete => false,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
    }
}