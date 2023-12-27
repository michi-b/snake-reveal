using System.Collections.Generic;
using Extensions;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Multi
{
    public class MultiLineChainRenderer : LineChainRenderer
    {
        [SerializeField] private LineCache _cache;

        [SerializeField] private List<LineRenderer> _lines = new(InitialLineCapacity);

        public override void Set(IList<Vector2> points, bool loop)
        {
            Clear();
            for (int i = 0; i < points.Count - 1; i++)
            {
                LineRenderer line = _cache.Get();
                Adopt(line, points[i], points[i + 1]);
            }
            
            if (!loop)
            {
                return;
            }

            LineRenderer loopConnectionLine = _cache.Get();
            Adopt(loopConnectionLine, points[^1], points[0]);
        }

        public override void SetInEditMode(IList<Vector2> points, bool loop)
        {
            Undo.RecordObject(this, nameof(SetInEditMode));
            ClearInEditMode();
            for (int i = 0; i < points.Count - 1; i++)
            {
                LineRenderer line = Instantiate(_cache.Prefab);
                Adopt(line, points[i], points[i + 1]);
            }

            if (!loop || points.Count < 2)
            {
                return;
            }

            LineRenderer loopConnectionLine = Instantiate(_cache.Prefab);
            Adopt(loopConnectionLine, points[^1], points[0]);
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

        private void Adopt(LineRenderer line, Vector2 start, Vector2 end)
        {
            line.transform.parent = transform;
            line.transform.localPosition = start.ToVector3(0f);
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, (end - start).ToVector3(0f));
            _lines.Add(line);
        }
    }
}