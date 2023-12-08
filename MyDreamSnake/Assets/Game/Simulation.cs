using System;
using Extensions;
using Game.Lines;
using Game.Player;
using UnityEngine;

namespace Game
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private Grid _grid;
        [SerializeField] private PlayerActor _playerActor;
        [SerializeField] private LineContainer _lineContainer;
        [SerializeField] private LineCache _lineCache;

        protected virtual void Start()
        {
            
        }

        protected virtual void FixedUpdate()
        {
            if (_playerActor.Direction != GridDirection.None)
            {
                for (var moveIndex = 0; moveIndex < _playerActor.Speed; moveIndex++)
                {
                    _playerActor.GridPosition += _playerActor.Direction.ToInt2();
                    _playerActor.GridPosition = _grid.Clamp(_playerActor.GridPosition);
                }
                _playerActor.transform.SetLocalPosition(_grid.GetScenePosition(_playerActor.GridPosition));
            }
        }
    }
}