using System.Linq;
using Extensions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Deprecated.Editor
{
    [CustomEditor(typeof(DeprecatedLineLoop))]
    public class DeprecatedLineLoopEditor : UnityEditor.Editor
    {
        private const string FullName = "Game.Lines.Editor.LineLoopEditor";
        private const string HalfQuadSizeKey = FullName + ".HalfQuadSize";
        private const string HalfQuadSizeXKey = HalfQuadSizeKey + ".x";
        private const string HalfQuadSizeYKey = HalfQuadSizeKey + ".y";
        private int2 _halfQuadSize = new(30, 30);
        private bool _isInitialized;

        public override void OnInspectorGUI()
        {
            if (!_isInitialized)
            {
                _halfQuadSize.x = EditorPrefs.GetInt(HalfQuadSizeXKey, _halfQuadSize.x);
                _halfQuadSize.y = EditorPrefs.GetInt(HalfQuadSizeYKey, _halfQuadSize.y);
                _isInitialized = true;
            }

            base.OnInspectorGUI();
            var lineLoop = (DeprecatedLineLoop)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Initialization");

            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                _halfQuadSize = EditorGUILayout.Vector2Field("Half Quad Size", _halfQuadSize.ToVector2()).ToInt2();
                if (changeCheckScope.changed)
                {
                    EditorPrefs.SetInt(HalfQuadSizeXKey, _halfQuadSize.x);
                    EditorPrefs.SetInt(HalfQuadSizeYKey, _halfQuadSize.y);
                }
            }

            const string initializeQuadClockwise = "Initialize Quad Clockwise";
            if (GUILayout.Button(initializeQuadClockwise))
            {
                Undo.RegisterFullObjectHierarchyUndo(target, initializeQuadClockwise);
                ClearLineLoop(lineLoop);
                int2[] quadPositions = GetCenteredQuadPositionsClockwise(lineLoop.Grid);
                lineLoop.Set(quadPositions);
            }

            const string initializeQuadCounterClockwise = "Initialize Quad Counter-Clockwise";
            if (GUILayout.Button(initializeQuadCounterClockwise))
            {
                Undo.RegisterFullObjectHierarchyUndo(target, initializeQuadCounterClockwise);
                ClearLineLoop(lineLoop);
                int2[] quadPositions = GetCenteredQuadPositionsClockwise(lineLoop.Grid).Reverse().ToArray();
                lineLoop.Set(quadPositions);
            }
        }

        private static void ClearLineLoop(DeprecatedLineLoop lineLoop)
        {
            DeprecatedLine start = lineLoop.Start;

            if (start == null)
            {
                return;
            }

            DeprecatedLine last = start.Previous;
            while (last != null)
            {
                DeprecatedLine next = last.Previous;
                DestroyImmediate(last.gameObject);
                last = next;
            }
        }

        private int2[] GetCenteredQuadPositionsClockwise(SimulationGrid grid)
        {
            int2 center = grid.Size / 2;
            int2 topLeft = center + _halfQuadSize * new int2(-1, 1);
            int2 topRight = center + _halfQuadSize * new int2(1, 1);
            int2 bottomRight = center + _halfQuadSize * new int2(1, -1);
            int2 bottomLeft = center + _halfQuadSize * new int2(-1, -1);

            int2[] quadPositions = { topLeft, topRight, bottomRight, bottomLeft };
            return quadPositions;
        }
    }
}