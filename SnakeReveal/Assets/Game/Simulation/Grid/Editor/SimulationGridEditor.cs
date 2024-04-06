using PlasticPipe.PlasticProtocol;
using UnityEditor;
using UnityEngine;

namespace Game.Simulation.Grid.Editor
{
    [CustomEditor(typeof(SimulationGrid))]
    public class SimulationGridEditor : UnityEditor.Editor
    {
        protected void OnEnable()
        {
            _sceneSizeProperty = serializedObject.FindProperty(SimulationGrid.SceneSizePropertyName);
            _collidersThicknessProperty = serializedObject.FindProperty(SimulationGrid.CollidersThicknessPropertyName);
            _paddingThicknessProperty = serializedObject.FindProperty(SimulationGrid.PaddingThicknessPropertyName);
            
            _cameraProperty = serializedObject.FindProperty(SimulationGrid.CameraPropertyName);
            _orthographicHeightProperty = serializedObject.FindProperty(SimulationGrid.OrthographicHeightPropertyName);
            _sceneAspectProperty = serializedObject.FindProperty(SimulationGrid.SceneAspectPropertyName);
        }

        public override void OnInspectorGUI()
        {
            var grid = (SimulationGrid)target;

            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(_cameraProperty);

                DrawSceneSize(grid);

                DrawPadding(grid);

                DrawColliders(grid);
                
                bool guiWasEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.PropertyField(_sceneAspectProperty);
                EditorGUILayout.PropertyField(_orthographicHeightProperty);
                GUI.enabled = guiWasEnabled;
            }
            if (EditorGUI.EndChangeCheck() || GUILayout.Button("Adjust Camera"))
            {
                serializedObject.ApplyModifiedProperties();
                AdjustCamera(grid);
            }
        }

        private void DrawSceneSize(SimulationGrid grid)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_sceneSizeProperty);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                Undo.RegisterFullObjectHierarchyUndo(grid.gameObject, "Change Scene Size");
                grid.ApplyPaddingThickness();
                grid.ApplyColliderThickness();
                AdjustCamera(grid);
            }

        }

        private void AdjustCamera(SimulationGrid grid)
        {
            Vector2 sceneSizeWithPadding = _sceneSizeProperty.vector2Value + _paddingThicknessProperty.floatValue * 2f * Vector2.one;
            _orthographicHeightProperty.floatValue = sceneSizeWithPadding.y * 0.5f;
            _sceneAspectProperty.floatValue = sceneSizeWithPadding.x / sceneSizeWithPadding.y;
            serializedObject.ApplyModifiedProperties();
            
            if (grid.Camera != null)
            {
                Undo.RecordObject(grid.Camera, "Apply Scene Size to Camera");
                grid.AdjustCamera();
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

        private SerializedProperty _sceneSizeProperty;
        private SerializedProperty _collidersThicknessProperty;
        private SerializedProperty _paddingThicknessProperty;
        
        private SerializedProperty _cameraProperty;
        private SerializedProperty _orthographicHeightProperty;
        private SerializedProperty _sceneAspectProperty;
    }
}