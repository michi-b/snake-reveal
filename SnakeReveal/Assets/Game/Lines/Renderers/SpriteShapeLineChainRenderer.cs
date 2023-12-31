using System.Collections.Generic;
using Game.Grid;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.Lines.Renderers
{
    [RequireComponent(typeof(SpriteShapeController))]
    public class SpriteShapeLineChainRenderer : LineChainRenderer
    {
        // [SerializeField] [Range(0f, 0.1f)] private float _width = 0.02f;

        private SpriteShapeController _controller;

        private SpriteShapeController Controller => _controller ? _controller : _controller = GetComponent<SpriteShapeController>();

        public override void EditModeRebuild(SimulationGrid grid, IReadOnlyList<Line> lines)
        {
            bool loop = lines[0].Start == lines[^1].End;

            Undo.RecordObject(this, nameof(EditModeRebuild));
            Spline spline = Controller.spline;
            spline.Clear();

            for (int i = 0; i < lines.Count; i++)
            {
                spline.InsertPointAt(i, grid.GetScenePosition(lines[i].Start));
                // spline.SetHeight(i, _width);
                // spline.SetTangentMode(i, ShapeTangentMode.Linear);
                // spline.SetCorner(i, true);
            }

            int lastIndex = lines.Count - 1;
            if (!loop)
            {
                spline.InsertPointAt(lastIndex, grid.GetScenePosition(lines[lastIndex].End));
            }

            spline.isOpenEnded = !loop;
        }
    }
}