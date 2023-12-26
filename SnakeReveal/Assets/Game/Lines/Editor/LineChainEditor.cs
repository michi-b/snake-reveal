using Editor;
using Extensions;
using Game.Enums;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineChain), true)]
    public class LineChainEditor : UnityEditor.Editor
    {
        private static GUIStyle _rightAlignedLabelStyle;

        public override void OnInspectorGUI()
        {
            _rightAlignedLabelStyle ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            base.OnInspectorGUI();

            var chain = (LineChain)target;

            DisplayNodes(chain);

            if (GUILayout.Button("Reevaluate All Directions"))
            {
                for (int i = 0; i < chain.Count; i++)
                {
                    chain.ReevaluateDirection(i);
                }
            }
        }

        private static void DrawNodeSizeControls(LineChain container)
        {
            Rect line = EditorGUILayout.GetControlRect(false);
            line.TakeFromLeft(EditorGUI.indentLevel * EditorGuiUtility.IndentSize);
            Rect leftButtonRect = line.TakeFromLeft(Mathf.Floor(line.width * 0.5f));
            Rect rightButtonRect = line;

            using (new GUILayout.HorizontalScope())
            {
                if (GUI.Button(leftButtonRect, "-"))
                {
                    container.RemoveLast();
                }

                // ReSharper disable once InvertIf
                if (GUI.Button(rightButtonRect, "+"))
                {
                    container.Append();
                    int previousIndex = container.Count - 2;
                    container[^1] = new LineNode
                    {
                        Position = container[previousIndex].Position,
                        Direction = GridDirection.None
                    };
                }
            }
        }

        private static void DisplayNodes(LineChain chain)
        {
            using var scope = new EditorGUI.IndentLevelScope(1);

            SimulationGrid grid = chain.Grid;

            for (int i = 0; i < chain.Count; i++)
            {
                LineNode node = chain[i];
                Rect line = EditorGUILayout.GetControlRect();
                float halfWidth = Mathf.Floor(line.width * 0.5f);
                Rect directionRect = line.TakeFromRight(halfWidth);
                Rect positionRect = line.TakeFromLeft(halfWidth);

                var position = node.Position.ToVector2Int();
                node.Position = EditorGUI.Vector2IntField(positionRect, GUIContent.none, position).ToInt2();
                if (grid != null)
                {
                    node.Position = grid.Clamp(node.Position);
                }

                string description = chain.GetLineDescription(i);

                node.Direction = (GridDirection)EditorGUI.EnumPopup(directionRect, node.Direction);

                chain[i] = node;
            }

            DrawNodeSizeControls(chain);
        }
    }
}