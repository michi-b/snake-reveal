using Extensions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineLoop))]
    public class LineLoopEditor : UnityEditor.Editor
    {
        private SimulationGrid _grid;

        private int2 _halfQuadSize = new(10, 10);
        private LineCache _linePrefab;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var lineLoop = (LineLoop)target;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Initialization");

            if (_grid == null)
            {
                _grid = FindAnyObjectByType<SimulationGrid>();
            }

            if (_linePrefab == null)
            {
                _linePrefab = FindAnyObjectByType<LineCache>();
            }

            if (_grid == null || _linePrefab == null)
            {
                EditorGUILayout.HelpBox("Missing grid or line prefab", MessageType.Error);
                return;
            }

            _halfQuadSize = EditorGUILayout.Vector2Field("Half Quad Size", _halfQuadSize.ToVector2()).ToInt2();

            if (GUILayout.Button("Initialize Quad"))
            {
                Undo.RegisterFullObjectHierarchyUndo(target, "Initialize Quad");

                Line start = lineLoop.Start;
                if (start != null)
                {
                    Line last = start.Previous;
                    while (last != null)
                    {
                        Line next = last.Previous;
                        DestroyImmediate(last.gameObject);
                        last = next;
                    }
                }

                int2 center = _grid.Size / 2;
                int2 topLeft = center + _halfQuadSize * new int2(-1, 1);
                int2 topRight = center + _halfQuadSize * new int2(1, 1);
                int2 bottomRight = center + _halfQuadSize * new int2(1, -1);
                int2 bottomLeft = center + _halfQuadSize * new int2(-1, -1);

                lineLoop.Set(_grid, _linePrefab, topLeft, topRight, bottomRight, bottomLeft);
            }
        }
    }
}