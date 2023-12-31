﻿using Game.Lines;
using Game.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private PlayerActor _playerActor;
        [SerializeField] private LineLoop _drawnShape;
        [SerializeField] private LineChain _drawingLineChain;
        [SerializeField] private Text _tickCounter;
        [SerializeField] private Text _timeCounter;

        private PlayerDrawingSimulation _playerDrawingSimulation;

        // with a fixed time step of 0.0083, this int will overflow after 206,2976188668982 days
        public int Ticks { get; private set; }

        protected virtual void Awake()
        {
            _playerDrawingSimulation = new PlayerDrawingSimulation(this, _playerActor, _drawnShape, _drawingLineChain);
        }

        protected virtual void FixedUpdate()
        {
            Ticks++;
            _tickCounter.text = Ticks.ToString();
            _timeCounter.text = (Ticks * 0.0083).ToString("F2");
            _playerDrawingSimulation.Tick();
        }

        protected virtual void OnEnable()
        {
            _playerDrawingSimulation.ControlsEnabled = true;
        }

        protected virtual void OnDisable()
        {
            _playerDrawingSimulation.ControlsEnabled = false;
        }
    }
}