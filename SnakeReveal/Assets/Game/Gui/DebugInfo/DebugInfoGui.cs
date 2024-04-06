using System;
using Game.Player.Controls;
using Game.Player.Controls.Touch;
using Game.Player.Controls.Touch.Extensions;
using Game.Simulation;
using TextDisplay.Abstractions;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

#if !DEBUG
using System; // InvalidOperationException
#endif

namespace Game.Gui.DebugInfo
{
    public class DebugInfoGui : MonoBehaviour
    {
        [SerializeField] private Game _game;
        
        [SerializeField] private TextRenderer _gameState;
        [SerializeField] private TextRenderer _simulationState;
        [SerializeField] private IntDisplay _simulationTicks;
        [SerializeField] private FloatDisplay _simulationTime;
        [SerializeField] private IntDisplay _totalCellCount;
        [SerializeField] private IntDisplay _coveredCellCount;
        [SerializeField] private IntDisplay _targetCellCount;
        [SerializeField] private BoolDisplay _touch0Active;
        [SerializeField] private Vector2Display _touch0Start;
        [SerializeField] private Vector2Display _swipe0Start;
        [SerializeField] private Vector2Display _touch0Current;


        protected virtual void Awake()
        {
#if DEBUG
            gameObject.SetActive(true);
#else
            gameObject.SetActive(false);
#endif
        }

        protected virtual void FixedUpdate()
        {
            if (EnhancedTouchSupport.enabled && Touch.fingers.Count > 0)
            {
                Finger finger = Touch.fingers[0];

                FingerTouchInteraction touchInteraction = finger.GetTouchInteraction();

                _touch0Active.Value = (touchInteraction & FingerTouchInteraction.IsTouching) != 0;
                _touch0Start.Value = finger.GetLatestScreenPosition();
                _touch0Current.Value = finger.GetLatestStartScreenPosition();
            }
            else
            {
                _touch0Start.Value = default;
                _touch0Current.Value = default;
                _touch0Active.Value = default;
            }
        }

        protected virtual void Update()
        {
            _gameState.Text = _game.State.Name;
            
            GameSimulation gameSimulation = _game.Simulation;
            
            _simulationState.Text = gameSimulation.PlayerSimulation.CurrentState.Name;
            
            _simulationTicks.Value = gameSimulation.Ticks;
            _simulationTime.Value = gameSimulation.GetSimulationTime();
            
            _targetCellCount.Value = gameSimulation.TargetCellCount;
            _totalCellCount.Value = gameSimulation.Grid.GetCellCount();
            _coveredCellCount.Value = gameSimulation.CoveredCellCount;
            
            PlayerSimulation playerSimulation = gameSimulation.PlayerSimulation;
            IPlayerActorControls controls = playerSimulation.Controls;
            if(controls is PlayerActorControls playerActorControls)
            {
                SwipeEvaluation swipeEvaluation = playerActorControls.SwipeEvaluation;
                _swipe0Start.Value = swipeEvaluation.GetSwipeStart(0);
            }
        }

        private static void AssertIsDebug()
        {
#if !DEBUG
            throw new InvalidOperationException("Debug info gui should bot be used in release code.");
#endif
        }
    }
}