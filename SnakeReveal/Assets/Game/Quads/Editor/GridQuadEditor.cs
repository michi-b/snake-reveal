using Editor;
using Game.Grid;
using UnityEditor;
using UnityEngine;

namespace Game.Quads.Editor
{
    [CustomEditor(typeof(Quad))]
    public class GridQuadEditor : UnityEditor.Editor
    {
        protected virtual void OnEnable()
        {
            Tools.hidden = true;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        protected virtual void OnDisable()
        {
            Tools.hidden = false;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        public void OnSceneGUI()
        {
            var quad = (Quad)target;
            SimulationGrid grid = quad.Grid;
            if (grid == null)
            {
                return;
            }

            Vector2Int bottomLeft = quad.BottomLeft;
            Vector2Int topRight = quad.TopRight;
            Vector2Int center = (bottomLeft + topRight) / 2;

            float z = quad.transform.position.z;

            if (HandlesUtility.TryGridHandleMove(bottomLeft, z, grid, out Vector2Int newBottomLeft))
            {
                quad.RegisterUndo("Move Grid Quad Bottom Left Handle");
                quad.BottomLeft = newBottomLeft;
            }

            if (HandlesUtility.TryGridHandleMove(topRight, z, grid, out Vector2Int newTopRight))
            {
                quad.RegisterUndo("Move Grid Quad Top Right Handle");
                quad.TopRight = newTopRight;
            }

            if (HandlesUtility.TryGridHandleMove(center, z, grid, out Vector2Int newCenter))
            {
                quad.RegisterUndo("Move Grid Quad Center Handle");
                Vector2Int delta = newCenter - center;
                quad.Move(delta);
            }
        }

        private void OnUndoRedoPerformed()
        {
            var quad = (Quad)target;
            quad.Apply();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                var collider = (Quad)target;
                Undo.RegisterFullObjectHierarchyUndo(collider.gameObject, "Apply Grid Quad Collider");
                collider.Apply();
            }
        }
    }
}