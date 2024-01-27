using Game.Player.Controls;
using Game.Player.Simulation.States;

namespace Game.Player.Simulation
{
    public class PlayerSimulation
    {
        private readonly PlayerActor _actor;
        private readonly IPlayerActorControls _controls;

        // whether the player in shape travel mode is traveling in the same direction as the shape turn
        private IPlayerSimulationState _currentState;

        private readonly ShapeTravelState _shapeTravelState;

        public PlayerSimulation(PlayerActor actor, DrawnShape shape, DrawingChain drawing, bool monkeyTestPlayerSimulationWithRandomInputs)
        {
            _actor = actor;
            _controls = monkeyTestPlayerSimulationWithRandomInputs ? new MonkeyTestRandomInputPlayerActorControls() : PlayerActorControls.Create();
            _shapeTravelState = new ShapeTravelState(_actor, shape);
            var drawingState = new DrawingState(_actor, shape, drawing, _shapeTravelState);
            _currentState = _shapeTravelState.Initialize(drawingState);
        }

        public bool ControlsEnabled
        {
            set
            {
                if (value)
                {
                    _controls.Activate();
                }
                else
                {
                    _controls.Deactivate();
                }
            }
        }

        public int CoveredCellCount => _shapeTravelState.CoveredCellCount;

        public void Move()
        {
            for (int moveIndex = 0; moveIndex < _actor.Speed; moveIndex++)
            {
                _currentState = _currentState.Move(_controls.GetRequestedDirection());
            }

            // todo: apply grid position only once per frame instead (and extrapolate)
            _actor.ApplyPosition();
        }
    }
}