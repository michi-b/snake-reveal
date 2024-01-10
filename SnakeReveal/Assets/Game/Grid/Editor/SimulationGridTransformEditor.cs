using Editor;
using Extensions;
using UnityEditor;
using UnityEngine;

namespace Game.Grid.Editor
{
    [CustomEditor(typeof(GridTransform))]
    public class SimulationGridTransformEditor : UnityEditor.Editor
    {
        private SerializedProperty _positionProperty;

        protected virtual void OnEnable()
        {
            _positionProperty = serializedObject.FindDirectChild(GridTransform.PositionPropertyName);
            Tools.hidden = true;
        }

        protected void OnDisable()
        {
            Tools.hidden = false;
        }

        protected virtual void OnSceneGUI()
        {
            var transform = (GridTransform)target;
            SimulationGrid grid = transform.Grid;
            if (grid == null)
            {
                return;
            }

            if(HandlesUtility.TryGridHandleMove(transform.Position, transform.transform.position.z, grid, out Vector2Int newGridPosition))
            {
                transform.RecordUndo("Move Grid Transform Handle");
                transform.Position = newGridPosition;
            }
           
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            bool defaultEditorChanged = EditorGUI.EndChangeCheck();

            var transform = (GridTransform)target;

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