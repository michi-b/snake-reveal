using Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(DoubleLinkedLineList), true)]
    public class LinkedLineListEditor : UnityEditor.Editor
    {
        private const string IsInsertExpandedKey = "LinkedLineListEditor.IsInsertionExpanded";
        private const string InsertTargetIdKey = "LinkedLineListEditor.InsertTargetId";

        private const string LinePositionHandleMoveOperationName = "Line Position Handle Move";

        private SerializedProperty _clockwiseTurnWeightProperty;
        private DoubleLinkedLineList _insertTarget;
        private int _insertTargetId;
        private bool _isInsertExpanded;
        private SerializedProperty _startProperty;
        private SerializedProperty _loopProperty;
        private Vector2Int _move;

        protected void OnEnable()
        {
            _startProperty = serializedObject.FindDirectChild(DoubleLinkedLineList.EditModeUtility.StartPropertyName);
            _isInsertExpanded = EditorPrefs.GetBool(IsInsertExpandedKey, false);

            _insertTargetId = EditorPrefs.GetInt(InsertTargetIdKey, 0);
            if (_insertTargetId != 0)
            {
                _insertTarget = EditorUtility.InstanceIDToObject(_insertTargetId) as DoubleLinkedLineList;
            }

            // Undo.undoRedoEvent += OnUndoRedo;
            //
            // ApplyChainChanges((DoubleLinkedLineList)target);
        }


        // protected void OnDisable()
        // {
        //     Undo.undoRedoEvent -= OnUndoRedo;
        // }

        protected virtual void OnSceneGUI()
        {
            var chain = (DoubleLinkedLineList)target;
            if (chain.Grid == null)
            {
                return;
            }

            Matrix4x4 originalHandlesMatrix = Handles.matrix;
            const float handleScale = 0.8f;
            const float inverseHandleScale = 1f / handleScale;
            Handles.matrix = Matrix4x4.Scale(Vector3.one * handleScale) * Handles.matrix;
            Line start = DoubleLinkedLineList.EditModeUtility.GetStart(chain);
            if (start == null)
            {
                return;
            }

            if (PositionHandle(start.Start, out Vector2Int newStartStart))
            {
                start.RecordUndoWithNeighbors(LinePositionHandleMoveOperationName);
                start.Start = newStartStart;
            }

            if (PositionHandle(start.End, out Vector2Int newStartEnd))
            {
                start.RecordUndoWithNeighbors(LinePositionHandleMoveOperationName);
                start.End = newStartEnd;
            }

            foreach (Line lineAfterFirst in start.SkipFirst())
            {
                if (PositionHandle(lineAfterFirst.End, out Vector2Int newLineAfterStartEnd))
                {
                    lineAfterFirst.RecordUndoWithNeighbors(LinePositionHandleMoveOperationName);
                    lineAfterFirst.End = newLineAfterStartEnd;
                }
            }

            bool PositionHandle(Vector2Int originalGridPosition, out Vector2Int newGridPosition)
            {
                Vector3 newPosition = Handles.PositionHandle(chain.GetWorldPosition(originalGridPosition) * inverseHandleScale, Quaternion.identity) * handleScale;
                newGridPosition = chain.Grid.RoundToGrid(newPosition);
                return newGridPosition != originalGridPosition;
            }

            Handles.matrix = originalHandlesMatrix;
        }


        // private void OnUndoRedo(in UndoRedoInfo undoRedoInfo)
        // {
        //     var chain = (DoubleLinkedLineList)target;
        //     ApplyChainChanges(chain);
        // }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_startProperty);
            }

            // GUI.enabled = !Application.isPlaying;
            //
            // EditorGUI.BeginChangeCheck();
            // _move = EditorGUILayout.Vector2IntField("Move", _move);
            // if (EditorGUI.EndChangeCheck() && _move != Vector2Int.zero)
            // {
            //     Undo.RecordObject(chain, "Move Chain");
            //     for (int i = 0; i < chain.Count; i++)
            //     {
            //         chain[i] = chain[i].Move(_move);
            //     }
            //
            //     ApplyChainChanges(chain);
            //     _move = Vector2Int.zero;
            // }
            //
            // if (GUILayout.Button("Invert"))
            // {
            //     DoubleLinkedLineList.EditModeUtility.Invert(chain);
            //     ApplyChainChanges(chain);
            // }
            //
            // DrawInsert(chain);
        }

#if false
        private static void HandleChainCountChange(DoubleLinkedLineList container, int oldCount)
        {
            if (container.Count <= oldCount)
            {
                container[^1] = container[^1].AsOpenChainEnd(!container.Loop);
                return;
            }

            // chain.Count > oldCount
            int oldLastIndex = oldCount - 1;
            Line oldChainEnd = container[oldLastIndex];

            container[oldLastIndex] = container[oldLastIndex].AsOpenChainEnd(false);

            for (int i = oldCount; i < container.Count - 1; i++)
            {
                container[i] = container[i].WithStart(oldChainEnd.End).AsOpenChainEnd(false);
            }

            container[^1] = container[^1].WithStart(oldChainEnd.End).AsOpenChainEnd(!container.Loop);
        }

        private static void HandleChainIsLoopChange(DoubleLinkedLineList container)
        {
            // open -> loop
            if (container.Loop)
            {
                container[^1] = container[^1].AsOpenChainEnd(false);
                container.AppendToChain(container[^1].End);
                return;
            }

            // loop -> open
            container.RemoveLastFromChain();
            container[^1] = container[^1].AsOpenChainEnd(true);
        }

        private static void ApplyChainChanges(DoubleLinkedLineList container)
        {
            if (container.Grid == null)
            {
                return;
            }

            Undo.RecordObject(container, nameof(ApplyChainChanges));
            DoubleLinkedLineList.EditModeUtility.FixLines(container);
            DoubleLinkedLineList.EditModeUtility.EditModeReevaluateClockwiseTurnWeight(container);
            DoubleLinkedLineList.EditModeUtility.RebuildLineRenderers(container);
            DoubleLinkedLineList.EditModeUtility.RebuildLineColliders(container);
        }

        private void DrawInsert(DoubleLinkedLineList container)
        {
            EditorGUI.BeginChangeCheck();
            _isInsertExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_isInsertExpanded, "Insert");
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(IsInsertExpandedKey, _isInsertExpanded);
            }

            if (_isInsertExpanded)
            {
                DrawInsertContent(container);
            }

            EditorGUI.EndFoldoutHeaderGroup();
        }

        private void DrawInsertContent(DoubleLinkedLineList container)
        {
            using var indent = new EditorGUI.IndentLevelScope(1);

            EditorGUI.BeginChangeCheck();
            _insertTarget = EditorGUILayout.ObjectField("Insert Target", _insertTarget, typeof(DoubleLinkedLineList), true) as DoubleLinkedLineList;
            if (EditorGUI.EndChangeCheck())
            {
                _insertTargetId = _insertTarget != null ? _insertTarget.GetInstanceID() : 0;
                EditorPrefs.SetInt(InsertTargetIdKey, _insertTargetId);
            }

            bool mayBeAbleToInsert = _insertTarget != null
                                     && _insertTarget != container
                                     && container.Loop
                                     && !_insertTarget.Loop;

            using (new EditorGUI.DisabledScope(!mayBeAbleToInsert))
            {
                if (GUILayout.Button("Insert"))
                {
                    DoubleLinkedLineList.InsertUtility.Insert(container, _insertTarget);
                }
            }
        }
#endif
    }
}