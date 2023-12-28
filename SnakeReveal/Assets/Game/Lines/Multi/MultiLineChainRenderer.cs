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

        public override void EditModeRebuild(IList<Vector2> points, bool loop)
        {
            Undo.RegisterCompleteObjectUndo(this.gameObject, nameof(EditModeRebuild));
            EditModeClear();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Adopt(Instantiate(_cache.Prefab), points[i], points[i + 1]);
            }

            if (!loop || points.Count < 2)
            {
                return;
            }

            Adopt(Instantiate(_cache.Prefab), points[^1], points[0]);
        }

        private void EditModeClear()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
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