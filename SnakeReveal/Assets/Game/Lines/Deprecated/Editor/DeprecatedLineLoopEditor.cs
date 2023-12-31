using System.Linq;
using Game.Grid;
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
        private Vector2Int _halfQuadSize = new(30, 30);
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
                _halfQuadSize = EditorGUILayout.Vector2IntField("Half Quad Size", Vector2Int.FloorToInt(_halfQuadSize));
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
                Vector2Int[] quadPositions = GetCenteredQuadPositionsClockwise(lineLoop.Grid);
                lineLoop.Set(quadPositions);
            }

            const string initializeQuadCounterClockwise = "Initialize Quad Counter-Clockwise";
            if (GUILayout.Button(initializeQuadCounterClockwise))
            {
                Undo.RegisterFullObjectHierarchyUndo(target, initializeQuadCounterClockwise);
                ClearLineLoop(lineLoop);
                Vector2Int[] quadPositions = GetCenteredQuadPositionsClockwise(lineLoop.Grid).Reverse().ToArray();
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

        private Vector2Int[] GetCenteredQuadPositionsClockwise(SimulationGrid grid)
        {
            Vector2Int center = grid.Size / 2;
            Vector2Int topLeft = center + _halfQuadSize * new Vector2Int(-1, 1);
            Vector2Int topRight = center + _halfQuadSize * new Vector2Int(1, 1);
            Vector2Int bottomRight = center + _halfQuadSize * new Vector2Int(1, -1);
            Vector2Int bottomLeft = center + _halfQuadSize * new Vector2Int(-1, -1);

            Vector2Int[] quadPositions = { topLeft, topRight, bottomRight, bottomLeft };
            return quadPositions;
        }
    }
}