using Game.Player.Controls;
using Game.Player.Simulation.States;

namespace Game.Player.Simulation
{
    public class PlayerSimulation
    {
        private readonly PlayerActor _actor;
        private readonly PlayerActorControls _controls;

        // whether the player in shape travel mode is traveling in the same direction as the shape turn
        private IPlayerSimulationState _currentState;

        public PlayerSimulation(PlayerActor actor, DrawnShape shape, DrawingChain drawing)
        {
            _actor = actor;
            DrawnShape shape1 = shape;
            _controls = PlayerActorControls.Create();
            var shapeTravelState = new ShapeTravelState(_actor, shape1);
            var drawingState = new DrawingState(_actor, shape1, drawing, shapeTravelState);
            _currentState = shapeTravelState.Initialize(drawingState);
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