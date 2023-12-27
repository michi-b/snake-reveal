using System;
using Editor;
using Extensions;
using Game.Enums;
using Unity.Mathematics;
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
            
            if(GUILayout.Button("Update Renderers"))
            {
                chain.UpdateRenderersInEditMode();
            }
            
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

                var position = corner._position.ToVector2Int();
                bool positionChanged = false;
                using (var changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    corner._position = EditorGUI.Vector2IntField(positionRect, GUIContent.none, position).ToInt2();
                    if (grid != null)
                    {
                        corner._position = grid.Clamp(corner._position);
                    }

                    if (changeCheck.changed)
                    {
                        positionChanged = true;
                    }
                }

                GUI.enabled = false;
                corner._direction = (GridDirection)EditorGUI.EnumPopup(directionRect, corner._direction);
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
                    int lastIndex = chain.Count - 1;
                    if (lastIndex > 0)
                    {
                        chain.ReevaluateDirection(lastIndex);
                    }
                }

                // ReSharper disable once InvertIf
                if (GUI.Button(rightButtonRect, "+"))
                {
                    bool isFirst = chain.Count == 0;
                    chain.Append();
                    int formerLastIndex = chain.Count - 2;
                    int2 position = isFirst 
                        ? chain.Grid != null ? chain.Grid.Size / 2 : new int2(0, 0) 
                        : chain[formerLastIndex]._position;
                    chain[^1] = new LineChain.Corner(position, GridDirection.None);
                    if (!isFirst)
                    {
                        chain.ReevaluateDirection(formerLastIndex);
                    }
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