using System;
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

            GUI.enabled = !Application.isPlaying;

            if (GUI.enabled)
            {
                Undo.RecordObject(chain, nameof(LineChainEditor));
            }

            DrawCornerCount(chain);
            DrawCorners(chain);

            if (GUILayout.Button("Reevaluate All Directions"))
            {
                for (int i = 0; i < chain.Count; i++)
                {
                    chain.ReevaluateDirection(i);
                }
            }

            GUI.enabled = true;
        }

        private static void DrawCornerCount(LineChain chain)
        {
            using var changeCheck = new EditorGUI.ChangeCheckScope();

            int count = EditorGUILayout.IntField("Corners", chain.Count);

            if (!changeCheck.changed)
            {
                return;
            }

            int delta = count - chain.Count;
            if (delta > 0)
            {
                for (int i = 0; i < delta; i++)
                {
                    chain.Append();
                }
            }
            else
            {
                delta = Math.Abs(delta);
                for (int i = 0; i < delta; i++)
                {
                    chain.RemoveLast();
                }
            }
        }

        private void DrawCorners(LineChain chain)
        {
            using var scope = new EditorGUI.IndentLevelScope(1);

            SimulationGrid grid = chain.Grid;

            bool guiWasEnabled = GUI.enabled;

            for (int i = 0; i < chain.Count; i++)
            {
                LineChain.Corner corner = chain[i];
                Rect line = EditorGUILayout.GetControlRect();
                float halfWidth = Mathf.Floor(line.width * 0.5f);
                Rect directionRect = line.TakeFromRight(halfWidth);
                Rect positionRect = line.TakeFromLeft(halfWidth);

                var position = corner.Position.ToVector2Int();
                bool positionChanged = false;
                using (var changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    corner.Position = EditorGUI.Vector2IntField(positionRect, GUIContent.none, position).ToInt2();
                    if (grid != null)
                    {
                        corner.Position = grid.Clamp(corner.Position);
                    }

                    if (changeCheck.changed)
                    {
                        positionChanged = true;
                    }
                }

                GUI.enabled = false;
                corner.Direction = (GridDirection)EditorGUI.EnumPopup(directionRect, corner.Direction);
                GUI.enabled = guiWasEnabled;

                if (!positionChanged)
                {
                    continue;
                }

                chain[i] = corner;
                ApplyPositionChange(chain, i);
            }

            DrawAddRemoveCornerButtons(chain);
        }

        private static void DrawAddRemoveCornerButtons(LineChain chain)
        {
            Rect line = EditorGUILayout.GetControlRect(false);
            line.TakeFromLeft(EditorGUI.indentLevel * EditorGuiUtility.IndentSize);
            Rect leftButtonRect = line.TakeFromLeft(Mathf.Floor(line.width * 0.5f));
            Rect rightButtonRect = line;

            using (new GUILayout.HorizontalScope())
            {
                if (GUI.Button(leftButtonRect, "-"))
                {
                    chain.RemoveLast();
                    chain.ReevaluateDirection(chain.Count - 1);
                }

                // ReSharper disable once InvertIf
                if (GUI.Button(rightButtonRect, "+"))
                {
                    int formerLastIndex = chain.Count - 1;
                    chain.Append();
                    chain[^1] = new LineChain.Corner
                    {
                        Position = chain[formerLastIndex].Position,
                        Direction = GridDirection.None
                    };
                    chain.ReevaluateDirection(formerLastIndex);
                }
            }
        }

        private static void ApplyPositionChange(LineChain chain, int i)
        {
            chain.ReevaluateDirection(i);
            int previousIndex = i - 1;
            if (previousIndex >= 0 || chain.Loop)
            {
                chain.ReevaluateDirection((previousIndex + chain.Count) % chain.Count);
            }
        }
    }
}