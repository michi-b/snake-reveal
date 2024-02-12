using System;
using Attributes;
using UnityEngine;

namespace Game.Gui.GameMenu
{
    [RequireComponent(typeof(Animator))]
    public class GameMenu : MonoBehaviour
    {
        protected void Awake()
        {
            _isOpenAnimatorPropertyId = Animator.StringToHash(_isOpenAnimatorPropertyName);
        }

        [UnityEventTarget("Game menu toggle button")]
        public void Toggle()
        {
            SetOpen(!_wantsToBeOpen);
        }

        private void SetOpen(bool value)
        {
            _wantsToBeOpen = value;
            Animator.SetBool(_isOpenAnimatorPropertyId, _wantsToBeOpen);
        }

        [UnityAnimationEventTarget]
        public void SetAnimatorState(AnimatorControlledState state)
        {
            AnimatorState = state;
        }

        private Animator _animator;

        private Animator Animator => _animator ??= GetComponent<Animator>();


        [SerializeField] private string _isOpenAnimatorPropertyName = "IsOpen";

        private int _isOpenAnimatorPropertyId;

        private bool _wantsToBeOpen;

        public AnimatorControlledState AnimatorState { get; private set; }

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