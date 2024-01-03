using System;
using System.Linq;
using Game.Enums;
using UnityEngine;

namespace Game.Lines
{
    public class LineLoop : LineContainer
    {
        public const string ClockwiseTurnWeightFieldName = nameof(_clockwiseTurnWeight);
        public const string TurnFieldName = nameof(_turn);

        [SerializeField, HideInInspector] private int _clockwiseTurnWeight;

        [SerializeField, HideInInspector] private Turn _turn;

        protected override Color GizmosColor => new(0f, 0.5f, 1f);
        protected override Line ExclusiveEnd => Start;

        protected override void PostProcessLineChanges()
        {
            _clockwiseTurnWeight = this.Sum(line => line.Direction.GetTurn(line.Next!.Direction).GetClockwiseWeight());
            _turn = _clockwiseTurnWeight == 0 ? Turn.None : _clockwiseTurnWeight > 0 ? Turn.Right : Turn.Left;
        }

        public void EditModeInsert(LineChain insertTarget)
        {
            throw new NotImplementedException();
        }
    }
}