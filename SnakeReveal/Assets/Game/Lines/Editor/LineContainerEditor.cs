using Editor;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineContainer), true)]
    public class LineContainerEditor : UnityEditor.Editor
    {
        private const string IsInsertExpandedKey = "LineContainerEditor.IsInsertionExpanded";
        private const string InsertTargetIdKey = "LineContainerEditor.InsertTargetId";


        private SerializedProperty _clockwiseTurnWeightProperty;
        private LineContainer _insertTarget;
        private int _insertTargetId;
        private bool _isInsertExpanded;
        private SerializedProperty _linesProperty;
        private SerializedProperty _loopProperty;

        private Vector2Int _move;

        protected void OnEnable()
        {
            _linesProperty = serializedObject.FindDirectChild(LineContainer.EditModeUtility.LinesPropertyName);
            _loopProperty = serializedObject.FindDirectChild(LineContainer.EditModeUtility.LoopPropertyName);
            _clockwiseTurnWeightProperty = serializedObject.FindDirectChild(LineContainer.EditModeUtility.ClockwiseTurnWeightPropertyName);
            _isInsertExpanded = EditorPrefs.GetBool(IsInsertExpandedKey, false);

            _insertTargetId = EditorPrefs.GetInt(InsertTargetIdKey, 0);
            if (_insertTargetId != 0)
            {
                _insertTarget = EditorUtility.InstanceIDToObject(_insertTargetId) as LineContainer;
            }

            Undo.undoRedoEvent += OnUndoRedo;

            ApplyChainChanges((LineContainer)target);
        }


        protected void OnDisable()
        {
            Undo.undoRedoEvent -= OnUndoRedo;
        }

        protected virtual void OnSceneGUI()
        {
            var chain = (LineContainer)target;
            if (chain.Grid == null)
            {
                return;
            }

            Matrix4x4 originalHandlesMatrix = Handles.matrix;
            const float handleScale = 0.8f;
            const float inverseHandleScale = 1f / handleScale;
            Handles.matrix = Matrix4x4.Scale(Vector3.one * handleScale) * Handles.matrix;

            for (int i = 0; i < chain.Count; i++)
            {
                Vector2Int oldStartPosition = chain[i].Start;
                if (!PositionHandle(oldStartPosition, out Vector2Int newStartPosition))
                {
                    continue;
                }

                Undo.RecordObject(chain, "Move Corner");
                chain[i] = chain[i].WithStart(newStartPosition);
                ApplyChainChanges(chain);
            }

            if (!chain.Loop)
            {
                Vector2Int oldEndPosition = chain[^1].End;
                if (PositionHandle(oldEndPosition, out Vector2Int newEndPosition))
                {
                    Undo.RecordObject(chain, "Move Corner");
                    chain[^1] = chain[^1].WithEnd(newEndPosition);
                    ApplyChainChanges(chain);
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

        private void OnUndoRedo(in UndoRedoInfo undoRedoInfo)
        {
            var chain = (LineContainer)target;
            ApplyChainChanges(chain);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var chain = (LineContainer)target;

            {
                int oldCount = chain.Count;
                bool wasLoop = chain.Loop;

                serializedObject.Update();
                EditorGUILayout.PropertyField(_loopProperty);
                EditorGUILayout.PropertyField(_linesProperty);

                if (serializedObject.ApplyModifiedProperties())
                {
                    if (chain.Count != oldCount)
                    {
                        HandleChainCountChange(chain, oldCount);
                    }
                    else if (chain.Loop != wasLoop)
                    {
                        HandleChainIsLoopChange(chain);
                    }

                    ApplyChainChanges(chain);
                }
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_clockwiseTurnWeightProperty);
            }

            GUI.enabled = !Application.isPlaying;

            EditorGUI.BeginChangeCheck();
            _move = EditorGUILayout.Vector2IntField("Move", _move);
            if (EditorGUI.EndChangeCheck() && _move != Vector2Int.zero)
            {
                Undo.RecordObject(chain, "Move Chain");
                for (int i = 0; i < chain.Count; i++)
                {
                    chain[i] = chain[i].Move(_move);
                }

                ApplyChainChanges(chain);
                _move = Vector2Int.zero;
            }

            if (GUILayout.Button("Invert"))
            {
                LineContainer.EditModeUtility.Invert(chain);
                ApplyChainChanges(chain);
            }

            DrawInsert(chain);
        }

        private static void HandleChainCountChange(LineContainer container, int oldCount)
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

        private static void HandleChainIsLoopChange(LineContainer container)
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

        private static void ApplyChainChanges(LineContainer container)
        {
            if (container.Grid == null)
            {
                return;
            }

            Undo.RecordObject(container, nameof(ApplyChainChanges));
            LineContainer.EditModeUtility.FixLines(container);
            LineContainer.EditModeUtility.EditModeReevaluateClockwiseTurnWeight(container);
            LineContainer.EditModeUtility.RebuildLineRenderers(container);
            LineContainer.EditModeUtility.RebuildLineColliders(container);
        }

        private void DrawInsert(LineContainer container)
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

        private void DrawInsertContent(LineContainer container)
        {
            using var indent = new EditorGUI.IndentLevelScope(1);

            EditorGUI.BeginChangeCheck();
            _insertTarget = EditorGUILayout.ObjectField("Insert Target", _insertTarget, typeof(LineContainer), true) as LineContainer;
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
                    LineContainer.InsertUtility.Insert(container, _insertTarget);
                }
            }
        }
    }
}