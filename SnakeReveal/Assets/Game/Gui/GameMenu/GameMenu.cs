using System;
using Attributes;
using Gam.Gui.GameMenu;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace Game.Gui.GameMenu
{
    [RequireComponent(typeof(Animator))]
    public class GameMenu : MonoBehaviour
    {
        [SerializeField] private string _isOpenAnimatorPropertyName = "IsOpen";
        [SerializeField] private Button _gameMenuButton;

        private int _isOpenAnimatorPropertyId;
        private bool _wantsToBeOpen;

        private Animator _animator;
        private GameMenuControls _keyboardControls;

        private Animator Animator => _animator ??= GetComponent<Animator>();

        public AnimatorControlledState AnimatorState { get; private set; }

        protected void Awake()
        {
            _isOpenAnimatorPropertyId = Animator.StringToHash(_isOpenAnimatorPropertyName);
            _keyboardControls = new GameMenuControls();
            _keyboardControls.GameMenu.Toggle.performed += _ => Toggle();
            _keyboardControls.Enable();
        }

        [UnityEventTarget("Game menu toggle button")]
        public void Toggle()
        {
            SetOpen(!_wantsToBeOpen);
        }

        [UnityEventTarget("Abort Level Button")]
        public void AbortLevel()
        {
            ApplicationUtility.Quit(ExitCodes.Success);
        }

        [UnityAnimationEventTarget]
        public void SetAnimatorState(AnimatorControlledState state)
        {
            AnimatorState = state;
        }

        public bool IsToggleEnabled
        {
            set
            {
                if (value)
                {
                    _keyboardControls.Enable();
                }
                else
                {
                    _keyboardControls.Disable();
                }

                _gameMenuButton.interactable = value;
            }
        }

        private void SetOpen(bool value)
        {
            _wantsToBeOpen = value;
            Animator.SetBool(_isOpenAnimatorPropertyId, _wantsToBeOpen);
        }

        [Serializable]
        public struct AnimatorControlledState
        {
            [SerializeField] private bool _isSomewhatOpen;
            [SerializeField] private bool _isFullyOpen;

            public bool IsSomewhatOpen => _isSomewhatOpen;

            public bool IsFullyOpen => _isFullyOpen;
        }
    }
}