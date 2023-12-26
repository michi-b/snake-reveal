using Extensions;
using Game.Enums;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineContainer), true)]
    public class LineContainerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            _rightAlignedLabelStyle ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            
            base.OnInspectorGUI();

            var container = (LineContainer)target;

            DisplayNodes(container);

            using (new GUILayout.HorizontalScope())
            {
                if(GUILayout.Button("-"))
                {
                    container.RemoveLast();
                }
                
                if (GUILayout.Button("+"))
                {
                    container.Append();
                }
            }
        }

        private static void DisplayNodes(LineContainer container)
        {
            for (int i = 0; i < container.Count; i++)
            {
                LineNode node = container[i];
                Rect line = EditorGUILayout.GetControlRect();
                float halfWidth = Mathf.Floor(line.width * 0.5f);
                Rect positionRect = line.TakeFromLeft(Mathf.Min(halfWidth, 100f));
                Rect directionRect = line.TakeFromRight(Mathf.Min(halfWidth, 50f));

                node.Position = EditorGUI.Vector2IntField(positionRect, GUIContent.none, node.Position.ToVector2Int()).ToInt2();
                node.Position = container.Grid.Clamp(node.Position);

                string description = container.GetLineDescription(i);
                var label = new GUIContent(description, description);
                EditorGUI.LabelField(line, label, _rightAlignedLabelStyle);

                node.Direction = (GridDirection)EditorGUI.EnumPopup(directionRect, node.Direction);

                container[i] = node;
            }
        }

        private static GUIStyle _rightAlignedLabelStyle;
    }
}