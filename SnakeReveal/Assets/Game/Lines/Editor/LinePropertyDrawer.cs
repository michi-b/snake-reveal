using Editor;
using Extensions;
using Game.Enums;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CustomPropertyDrawer(typeof(Line))]
    public class LinePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty startProperty = property.FindDirectChild(Line.StartPropertyName);
            SerializedProperty directionProperty = property.FindDirectChild(Line.DirectionPropertyName);
            SerializedProperty endProperty = property.FindDirectChild(Line.EndPropertyName);

            float thirdOfWidth = Mathf.Floor(position.width * 0.33333333f);
            float directionRectWidth = Mathf.Min(thirdOfWidth, 60f);
            float vectorWidth = Mathf.Floor((position.width - directionRectWidth) * 0.5f);
            Rect startRect = position.TakeFromLeft(vectorWidth);
            Rect directionRect = position.TakeFromLeft(directionRectWidth);
            Rect endRect = position.TakeFromLeft(vectorWidth);

            EditorGUI.PropertyField(startRect, startProperty, GUIContent.none);
            EditorGUI.PropertyField(endRect, endProperty, GUIContent.none);
            
            bool guiWasEnabled = GUI.enabled;
            GUI.enabled = false;
            {
                var direction = directionProperty.GetEnumValue<GridDirection>();
                Color originalGuiColor = GUI.color;
                bool hasDirection = direction != GridDirection.None;
                GUI.color = hasDirection ? originalGuiColor : new Color(1f, 0.5f, 0f);
                {
                    EditorGUI.PropertyField(directionRect, directionProperty, GUIContent.none);
                }
                GUI.color = originalGuiColor;
            }
            GUI.enabled = guiWasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}