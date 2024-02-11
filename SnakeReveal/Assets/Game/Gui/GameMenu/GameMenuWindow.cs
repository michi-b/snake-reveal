using System;
using Attributes;
using UnityEngine;

namespace Game.Gui.GameMenu
{
    [RequireComponent(typeof(Animator))]
    public class GameMenuWindow : MonoBehaviour
    {
        protected void Awake()
        {
            _isOpenAnimatorPropertyId = Animator.StringToHash(_isOpenAnimatorPropertyName);
        }

        [UnityAnimationEventTarget]
        public void ToggleOpen()
        {
            Animator.SetBool(_isOpenAnimatorPropertyId, !IsOpen);
        }

        private bool IsOpen
        {
            get => Animator.GetBool(_isOpenAnimatorPropertyId);
            set => Animator.SetBool(_isOpenAnimatorPropertyId, value);
        }

        private Animator _animator;
        private Animator Animator => _animator ??= GetComponent<Animator>();


        [SerializeField] private string _isOpenAnimatorPropertyName = "IsOpen";
        private int _isOpenAnimatorPropertyId;
    }
}