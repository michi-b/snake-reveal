using System;
using Attributes;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace Game.UI.GameMenu
{
    [RequireComponent(typeof(Animator))]
    public class GameMenu : MonoBehaviour
    {
        [SerializeField] private string _isOpenAnimatorPropertyName = "IsOpen";
        [SerializeField] private Button _gameMenuButton;


        private int _isOpenAnimatorPropertyId;
        private bool _wantsToBeOpen;

        private Animator _animator;

        private Animator Animator => _animator ??= GetComponent<Animator>();

        public AnimatorControlledState AnimatorState { get; private set; }

        protected void Awake()
        {
            _isOpenAnimatorPropertyId = Animator.StringToHash(_isOpenAnimatorPropertyName);
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

        public bool IsOpenButtonEnabled
        {
            set => _gameMenuButton.interactable = value;
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