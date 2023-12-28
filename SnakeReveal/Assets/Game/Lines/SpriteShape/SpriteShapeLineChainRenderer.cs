using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.Lines.SpriteShape
{
    [RequireComponent(typeof(SpriteShapeController))]
    public class SpriteShapeLineChainRenderer : LineChainRenderer
    {
        [SerializeField] [Range(0f, 0.1f)] private float _width = 0.02f;
        
        private SpriteShapeController _controller;

        private SpriteShapeController Controller => _controller ? _controller : _controller = GetComponent<SpriteShapeController>();

        public override void EditModeRebuild(IList<Vector2> points, bool loop)
        {
            Undo.RecordObject(this, nameof(EditModeRebuild));
            Controller.cornerAngleThreshold = 0f;
            Spline spline = Controller.spline;
            spline.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                spline.InsertPointAt(i, points[i]);
                spline.SetHeight(i, _width);
                spline.SetTangentMode(i, ShapeTangentMode.Linear);
                spline.SetCorner(i, true);
            }
            spline.isOpenEnded = !loop;
        }
    }
}