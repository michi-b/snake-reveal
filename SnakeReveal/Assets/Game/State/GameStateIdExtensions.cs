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
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
    }
}