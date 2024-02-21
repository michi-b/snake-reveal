using UnityEditor;

namespace Game.Simulation.Grid.Editor
{
    [CustomEditor(typeof(SimulationGrid))]
    public class SimulationGridEditor : UnityEditor.Editor
    {
        protected void OnEnable()
        {
            _collidersThicknessProperty = serializedObject.FindProperty(SimulationGrid.CollidersThicknessPropertyName);
            _paddingThicknessProperty = serializedObject.FindProperty(SimulationGrid.PaddingThicknessPropertyName);
        }

        public override void OnInspectorGUI()
        {
            var grid = (SimulationGrid)target;

            DrawSceneSize(grid);

            base.OnInspectorGUI();

            DrawPadding(grid);

            DrawColliders(grid);
        }

        private void DrawSceneSize(SimulationGrid grid)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(SimulationGrid.SceneSizePropertyName));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RegisterFullObjectHierarchyUndo(grid.gameObject, "Change Scene Size");
                grid.ApplyPaddingThickness();
                grid.ApplyColliderThickness();
            }
        }

        private void DrawColliders(SimulationGrid grid)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_collidersThicknessProperty);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RegisterFullObjectHierarchyUndo(grid.gameObject, "Change Colliders Thickness");
                grid.ApplyColliderThickness();
            }
        }

        private void DrawPadding(SimulationGrid grid)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_paddingThicknessProperty);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RegisterFullObjectHierarchyUndo(grid.gameObject, "Change Padding Thickness");
                grid.ApplyPaddingThickness();
                grid.ApplyColliderThickness();
            }
        }

        private SerializedProperty _collidersThicknessProperty;
        private SerializedProperty _paddingThicknessProperty;
    }
}