using System;
using System.Collections.Generic;
using Extensions;
using Game.Enums;
using Game.Lines.Colliders;
using UnityEngine;

namespace Game.Lines
{
    public partial class LineContainer : MonoBehaviour
    {
        private const int InitialLinesCapacity = 1000;

        [SerializeField] private SimulationGrid _grid;

        [SerializeField] private LineColliderCache _colliderCache;
        
        [SerializeField] private Transform _colliderContainer;

        [SerializeField] private bool _drawGizmos = true;

        [SerializeField] private Color _gizmosColor = Color.blue;

        [SerializeField] private LineChainRenderer[] _lineRenderers = Array.Empty<LineChainRenderer>();

        [SerializeField] [HideInInspector] private List<Line> _lines = new(InitialLinesCapacity);

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

        private Turn Turn => _clockwiseTurnWeight switch
        {
            0 => Turn.None,
            _ => _clockwiseTurnWeight > 0 ? Turn.Right : Turn.Left
        };

        protected void OnDrawGizmos()
        {
            if (_drawGizmos && Count > 0 && Grid != null)
            {
                Color originalGizmoColor = Gizmos.color;
                Gizmos.color = _gizmosColor;
                Vector3 startPosition = GetWorldPosition(this[0].Start);
                Vector3 endPosition = GetWorldPosition(this[0].End);
                Gizmos.DrawWireSphere(startPosition, Grid.SceneCellSize.magnitude * 0.5f);
                Gizmos.DrawLine(startPosition, endPosition);
                Gizmos.color = originalGizmoColor;
            }
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