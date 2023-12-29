using System.Collections.Generic;
using Extensions;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Renderers
{
    public class MultiLineChainRenderer : LineChainRenderer
    {
        private const int InitialLineCapacity = 1000;

        [SerializeField] private LineCache _cache;
        [SerializeField] private List<LineRenderer> _lines = new(InitialLineCapacity);

        public override void EditModeRebuild(IReadOnlyList<Line> lines)
        {
            Undo.RegisterCompleteObjectUndo(gameObject, nameof(EditModeRebuild));
            EditModeClear();
            foreach (Line line in lines)
            {
                Adopt(Instantiate(_cache.Prefab), line);
            }
        }

        private void EditModeClear()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            _lines.Clear();
        }

        private void Adopt(LineRenderer lineRenderer, Line line)
        {
            Transform rendererTransform = lineRenderer.transform;
            rendererTransform.parent = transform;
            Vector2 startPosition = Grid.GetScenePosition(line.Start);
            Vector2 endPosition = Grid.GetScenePosition(line.End);
            rendererTransform.localPosition = startPosition.ToVector3(0f);
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, (endPosition - startPosition).ToVector3(0f));
            _lines.Add(lineRenderer);
        }
    }
}