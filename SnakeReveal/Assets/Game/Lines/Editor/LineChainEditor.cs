using System;
using Editor;
using Extensions;
using Game.Enums;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineChain), true)]
    public class LineChainEditor : UnityEditor.Editor
    {
        private static GUIStyle _rightAlignedLabelStyle;

        private static readonly GUIContent TurnLabel = new(nameof(LineChain.Turn));

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
            
            if (GUILayout.Button("Update Renderers"))
            {
                chain.UpdateRenderersInEditMode();
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup(TurnLabel, chain.Turn);
            }
            if(GUILayout.Button("Evaluate Turn"))
            {
                chain.EvaluateTurn();
            }

            DrawCornerCount(chain);
            DrawCorners(chain);

            if (GUILayout.Button("Reevaluate All Directions"))
            {
                for (int i = 0; i < chain.Count; i++)
                {
                    chain.EvaluateDirection(i);
                }
            }

            GUI.enabled = true;
        }

        private static void DrawCornerCount(LineChain chain)
        {
            using var changeCheck = new EditorGUI.ChangeCheckScope();

            int count = EditorGUILayout.IntField("Corners", chain.Count);
            count = Mathf.Max(0, count);

            if (!changeCheck.changed)
            {
                return;
            }

            int delta = count - chain.Count;
            if (delta > 0)
            {
                for (int i = 0; i < delta; i++)
                {
                    AppendCorner(chain);
                }
            }
            else
            {
                delta = Math.Abs(delta);
                for (int i = 0; i < delta; i++)
                {
                    RemoveLastCorner(chain);
                }
            }
        }

        private static void DrawCorners(LineChain chain)
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
                    RemoveLastCorner(chain);
                }

                // ReSharper disable once InvertIf
                if (GUI.Button(rightButtonRect, "+"))
                {
                    AppendCorner(chain);
                }
            }
        }

        private static void RemoveLastCorner(LineChain chain)
        {
            chain.RemoveLast();
            int lastIndex = chain.Count - 1;
            if (lastIndex > 0)
            {
                chain.EvaluateDirection(lastIndex);
            }
        }

        private static void AppendCorner(LineChain chain)
        {
            bool isFirst = chain.Count == 0;
            int2 position = isFirst
                ? chain.Grid != null ? chain.Grid.Size / 2 : new int2(0, 0)
                : chain[^1].Position;
            chain.Append(position);
        }

        private static void ApplyPositionChange(LineChain chain, int i)
        {
            chain.EvaluateDirection(i);
            int previousIndex = i - 1;
            if (previousIndex >= 0 || chain.Loop)
            {
                chain.EvaluateDirection((previousIndex + chain.Count) % chain.Count);
            }
        }
    }
}