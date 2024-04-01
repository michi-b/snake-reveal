using Game.Player.Controls;
using Game.Player.Controls.Touch.Extensions;
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
        [SerializeField] private TextRenderer _gameState;
        [SerializeField] private TextRenderer _simulationState;
        [SerializeField] private IntDisplay _simulationTicks;
        [SerializeField] private FloatDisplay _simulationTime;
        [SerializeField] private IntDisplay _totalCellCount;
        [SerializeField] private IntDisplay _coveredCellCount;
        [SerializeField] private FloatDisplay _coveredCellPercentage;
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

        protected void FixedUpdate()
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

        public string GameState
        {
            set
            {
                AssertIsDebug();
                _gameState.Text = value;
            }
        }

        public string SimulationState
        {
            set
            {
                AssertIsDebug();
                _simulationState.Text = value;
            }
        }

        public float SimulationTime
        {
            set
            {
                AssertIsDebug();
                _simulationTime.Value = value;
            }
        }

        public int SimulationTicks
        {
            set
            {
                AssertIsDebug();
                _simulationTicks.Value = value;
            }
        }

        public int TotalCellCount
        {
            set
            {
                AssertIsDebug();
                _totalCellCount.Value = value;
                UpdateCoveredCellPercentage();
            }
        }

        public int CoveredCellCount
        {
            set
            {
                AssertIsDebug();
                _coveredCellCount.Value = value;
                UpdateCoveredCellPercentage();
            }
        }

        public int TargetCellCount
        {
            set
            {
                AssertIsDebug();
                _targetCellCount.Value = value;
            }
        }
        
        public Vector2 Swipe0Start
        {
            set
            {
                AssertIsDebug();
                _swipe0Start.Value = value;
            }
        }

        private void UpdateCoveredCellPercentage()
        {
            _coveredCellPercentage.Value = (float)_coveredCellCount.Value / _totalCellCount.Value;
        }

        private static void AssertIsDebug()
        {
#if !DEBUG
            throw new InvalidOperationException("Debug info gui should bot be used in release code.");
#endif
        }
    }
}