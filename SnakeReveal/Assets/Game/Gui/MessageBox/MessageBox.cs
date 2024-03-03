using Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Gui.MessageBox
{
    [RequireComponent(typeof(Animator))]
    public class MessageBox : MonoBehaviour
    {
        [SerializeField] private bool _isVisible;

        private Animator _animator;

        private Animator Animator => _animator = _animator == null ? GetComponent<Animator>() : _animator;


        [UnityEventTarget("On Okay Button Clicked")]
        public virtual bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    ApplyIsVisible();
                }
            }
        }

        protected virtual void ApplyIsVisible()
        {
            Animator.SetBool(IsVisibleAnimatorPropertyId, _isVisible);
        }

        protected void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplyIsVisible();
            }
        }

        [PublicAPI] protected static readonly int IsVisibleAnimatorPropertyId = Animator.StringToHash(IsVisibleAnimatorPropertyName);

        private const string IsVisibleAnimatorPropertyName = "IsVisible";
    }
}