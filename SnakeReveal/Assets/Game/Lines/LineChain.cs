using System;
using System.Collections.Generic;
using Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Lines
{
    public partial class LineChain : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private LineChainRenderer[] _lineRenderers = Array.Empty<LineChainRenderer>();

        [SerializeField] [HideInInspector] private List<Line> _lines = new();

        [SerializeField] [HideInInspector] private int _clockwiseTurnWeight;

        [SerializeField] [HideInInspector] private bool _loop;

        public SimulationGrid Grid => _grid;

        public int Count => _lines.Count;

        public bool Loop => _loop;

        public Line this[int index]
        {
            get => _lines[index];
            set => _lines[index] = value;
        }

        public Vector3 GetWorldPosition(Vector2Int position)
        {
            return Grid.GetScenePosition(position).ToVector3(transform.position.z);
        }

        public void Append(Vector2Int position)
        {
            if (_lines.Count == 0)
            {
                _lines.Add(new Line(position, position, !_loop));
                return;
            }

            if (Loop)
            {
                _lines.Add(new Line(position, _lines[0].Start));
                _lines[^1] = _lines[^1].WithEnd(position);
                return;
            }

            _lines[^1] = _lines[^1].AsOpenChainEnd(false);

            _lines.Add(new Line(_lines[^1].End, position, true));
        }

        public void RemoveLast()
        {
            Vector2Int lastStart = _lines[^1].Start;
            _lines.RemoveAt(_lines.Count - 1);
            if (_lines.Count > 0)
            {
                _lines[^1] = _lines[^1].WithEnd(Loop ? _lines[0].Start : lastStart);
            }
        }
    }
}