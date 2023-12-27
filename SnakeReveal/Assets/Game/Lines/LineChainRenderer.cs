using System.Collections.Generic;
using Extensions;
using UnityEditor;
using UnityEngine;

namespace Game.Lines
{
    public class LineChainRenderer : MonoBehaviour
    {
        public const int InitialLineCapacity = 1000;

        [SerializeField] private LineCache _cache;

        [SerializeField] private List<LineRenderer> _lines = new(InitialLineCapacity);

        public void Set(IList<Vector3> points)
        {
            Clear();
            for (int i = 0; i < points.Count - 1; i++)
            {
                LineRenderer line = _cache.Get();
                Adopt(line);
                line.SetPosition(0, points[i]);
                line.SetPosition(1, points[i + 1]);
            }
        }

        public void SetInEditMode(IList<Vector3> points)
        {
            Undo.RecordObject(this, nameof(SetInEditMode));
            ClearInEditMode();
            for (int i = 0; i < points.Count - 1; i++)
            {
                LineRenderer line = Instantiate(_cache.Prefab);
                Adopt(line);
                line.SetPosition(0, points[i]);
                line.SetPosition(1, points[i + 1]);
            }
        }

        private void Clear()
        {
            foreach (LineRenderer line in _lines)
            {
                _cache.Return(line);
            }

            _lines.Clear();
        }

        private void ClearInEditMode()
        {
            foreach (LineRenderer line in _lines)
            {
                DestroyImmediate(line.gameObject);
            }

            _lines.Clear();
        }

        private void Adopt(LineRenderer line)
        {
            line.transform.parent = transform;
            line.transform.SetLocalPositionZ(0f);
            _lines.Add(line);
        }
    }
}