using Editor;
using Extensions;
using UnityEditor;
using UnityEngine;

namespace Game.Grid.Editor
{
    [CustomEditor(typeof(SimulationGridTransform))]
    public class SimulationGridTransformEditor : UnityEditor.Editor
    {
        private SerializedProperty _positionProperty;

        protected virtual void OnEnable()
        {
            _positionProperty = serializedObject.FindDirectChild(SimulationGridTransform.PositionPropertyName);
            Tools.hidden = true;
        }

        protected void OnDisable()
        {
            Tools.hidden = false;
        }

        protected virtual void OnSceneGUI()
        {
            var transform = (SimulationGridTransform)target;
            if (transform.Grid == null)
            {
                return;
            }

            if (PositionHandle(transform.Position, out Vector2Int newGridPosition))
            {
                transform.RecordUndo("Move Grid Transform Handle");
                transform.transform.SetLocalPositionXY(transform.Grid.GetScenePosition(newGridPosition));
                _positionProperty.vector2IntValue = newGridPosition;
                serializedObject.ApplyModifiedProperties();
                transform.Apply();
            }

            bool PositionHandle(Vector2Int originalGridPosition, out Vector2Int result)
            {
                var originalPosition = transform.Grid.GetScenePosition(originalGridPosition).ToVector3(transform.transform.localPosition.z);
                Vector3 newPosition = Handles.PositionHandle(originalPosition, Quaternion.identity);
                result = transform.Grid.Round(newPosition);
                return result != originalGridPosition;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            bool defaultEditorChanged = EditorGUI.EndChangeCheck();

            var transform = (SimulationGridTransform)target;

            if (defaultEditorChanged)
            {
                if (transform.Grid != null)
                {
                    transform.RecordUndo("Apply Grid Transform");
                    transform.Apply();
                }
            }

            if (GUILayout.Button("Move To Grid Center"))
            {
                transform.RecordUndo("Move To Grid Center");
                transform.Position = transform.Grid.CenterPosition;
            }
        }
    }
}