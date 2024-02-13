using System;
using Game.Enums;
using Game.Enums.Extensions;
using Game.Enums.Utility;
using UnityEngine;
using UnityEngine.UI;
using UndoUtility = Utility.UndoUtility;

namespace Game.Gui.AvailableDirectionsIndication
{
    public class AvailableDirectionsIndication : MonoBehaviour
    {
        [ContextMenuItem("Apply", nameof(ApplyDirectionsViaContextMenuEntry)), SerializeField]
        private GridDirections _directions;

        public GridDirections Directions
        {
            get => _directions;
            set
            {
                if (value != _directions)
                {
                    _directions = value;
                    ApplyDirections();
                }
            }
        }

        protected void Awake()
        {
            _isVisibleParameterId = Animator.StringToHash(_isVisibleParameterName);
        }

        public void SetVisible(bool visible)
        {
            Animator.SetBool(_isVisibleParameterId, visible);
        }

        private void ApplyDirectionsViaContextMenuEntry()
        {
            UndoUtility.RecordFullGameObjectHierarchy(this, nameof(ApplyDirectionsViaContextMenuEntry));
            ApplyDirections();
        }

        private void ApplyDirections()
        {
            foreach (GridDirection direction in GridDirectionUtility.ActualDirections)
            {
                GetImage(direction).gameObject.SetActive(Directions.Contains(direction));
            }
        }

        private Image GetImage(GridDirection direction)
        {
            return direction switch
            {
                GridDirection.Right => _rightArrow,
                GridDirection.Up => _upArrow,
                GridDirection.Left => _leftArrow,
                GridDirection.Down => _downArrow,
                GridDirection.None => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        private Animator _animator;
        private Animator Animator => _animator ??= GetComponent<Animator>();

        [SerializeField] private Image _upArrow;
        [SerializeField] private Image _rightArrow;
        [SerializeField] private Image _downArrow;
        [SerializeField] private Image _leftArrow;

        [SerializeField] private string _isVisibleParameterName = "IsVisible";
        private int _isVisibleParameterId;
    }
}