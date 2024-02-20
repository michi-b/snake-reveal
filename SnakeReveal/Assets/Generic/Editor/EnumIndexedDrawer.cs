using Extensions.Editor;
using UnityEditor;
using UnityEngine;

namespace Generic.Editor
{
    [CustomPropertyDrawer(typeof(EnumIndexed<,>))]
    public class EnumIndexedDrawer : PropertyDrawer
    {
        public const string ItemsFieldName = "_items";
        public const string EnumFieldName = "_enum";
        private const bool IncludeChildren = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty items = property.FindPropertyRelative(ItemsFieldName);
            SerializedProperty enumProperty = property.FindPropertyRelative(EnumFieldName);

            EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(position.TakeSingleLineFromTop(), property.isExpanded, label);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                string[] enumNames = enumProperty.enumNames;

                if (enumNames.Length != items.arraySize)
                {
                    Debug.LogError($"Property has {enumNames.Length} enum names ({string.Join(", ", enumNames)}) but {items.arraySize} items. " +
                                   $"Can't draw that shit. ({property.propertyPath})");
                }

                for (int i = 0; i < items.arraySize; i++)
                {
                    SerializedProperty item = items.GetArrayElementAtIndex(i);
                    var itemLabel = new GUIContent(enumNames[i]);
                    float itemHeight = EditorGUI.GetPropertyHeight(item, itemLabel, IncludeChildren);
                    Rect itemPosition = position.TakeSingleLineFromTop();
                    EditorGUI.BeginProperty(itemPosition, itemLabel, item);
                    EditorGUI.PropertyField(itemPosition, item, itemLabel);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndFoldoutHeaderGroup();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty items = property.FindPropertyRelative(ItemsFieldName);

            float height = EditorGUIUtility.singleLineHeight;

            for (int i = 0; i < items.arraySize; i++)
            {
                SerializedProperty item = items.GetArrayElementAtIndex(i);
                height += EditorGUI.GetPropertyHeight(item, GUIContent.none, IncludeChildren);
            }

            return height;
        }
    }
}