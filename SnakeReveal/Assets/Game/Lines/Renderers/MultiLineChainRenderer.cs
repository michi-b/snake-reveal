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

        public override void EditModeRebuild(SimulationGrid grid, IReadOnlyList<Line> lines)
        {
            Undo.RegisterCompleteObjectUndo(gameObject, nameof(EditModeRebuild));
            EditModeClear();
            foreach (Line line in lines)
            {
                Adopt(grid, Instantiate(_cache.Prefab), line);
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

        private void Adopt(SimulationGrid grid, LineRenderer lineRenderer, Line line)
        {
            Transform rendererTransform = lineRenderer.transform;
            rendererTransform.parent = transform;
            Vector2 startPosition = grid.GetScenePosition(line.Start);
            Vector2 endPosition = grid.GetScenePosition(line.End);
            rendererTransform.localPosition = startPosition.ToVector3(0f);
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, (endPosition - startPosition).ToVector3(0f));
            _lines.Add(lineRenderer);
        }
    }
}