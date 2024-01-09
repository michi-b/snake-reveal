using UnityEditor;
using UnityEngine;

namespace CustomPropertyDrawers.Editor
{
    [CustomPropertyDrawer(typeof(ToggleLeftAttribute))]
    public class ToggleLeftPropertyDrawer : PropertyDrawer
    {
        private bool _isBool;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_isBool)
            {
                property.boolValue = EditorGUI.ToggleLeft(position, label, property.boolValue);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            _isBool = property.propertyType == SerializedPropertyType.Boolean;
            return _isBool ? EditorGUIUtility.singleLineHeight : EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}