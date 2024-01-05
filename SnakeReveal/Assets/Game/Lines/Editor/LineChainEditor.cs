using Editor;
using UnityEditor;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineChain))]
    public class LineChainEditor : LineContainerEditor
    {
        private SerializedProperty _lastLineProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _lastLineProperty = serializedObject.FindDirectChild(LineChain.LastFieldName);
        }

        protected override int GetInitialSelectionIndex(int count)
        {
            return count - 1;
        }

        protected override void DrawProperties()
        {
            base.DrawProperties();

            using var disabledScope = new EditorGUI.DisabledScope(true);
            EditorGUILayout.PropertyField(_lastLineProperty);
        }
    }
}