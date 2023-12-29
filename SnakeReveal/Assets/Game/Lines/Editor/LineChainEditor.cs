using UnityEditor;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineChain), true)]
    public class LineChainEditor : UnityEditor.Editor
    {
        private const string IsInsertExpandedKey = "LineChainEditor.IsInsertionExpanded";
        private const string InsertTargetIdKey = "LineChainEditor.InsertTargetId";


        private SerializedProperty _clockwiseTurnWeightProperty;
        private LineChain _insertTarget;
        private int _insertTargetId;
        private bool _isInsertExpanded;
        private SerializedProperty _linesProperty;
        private SerializedProperty _loopProperty;

        private Vector2Int _move;

        protected void OnEnable()
        {
            _linesProperty = serializedObject.FindProperty(LineChain.EditModeUtility.LinesPropertyName);
            _loopProperty = serializedObject.FindProperty(LineChain.EditModeUtility.LoopPropertyName);
            _clockwiseTurnWeightProperty = serializedObject.FindProperty(LineChain.EditModeUtility.ClockwiseTurnWeightPropertyName);
            _isInsertExpanded = EditorPrefs.GetBool(IsInsertExpandedKey, false);

            _insertTargetId = EditorPrefs.GetInt(InsertTargetIdKey, 0);
            if (_insertTargetId != 0)
            {
                _insertTarget = EditorUtility.InstanceIDToObject(_insertTargetId) as LineChain;
            }

            Undo.undoRedoEvent += OnUndoRedo;

            ApplyChainChanges((LineChain)target);
        }


        protected void OnDisable()
        {
            Undo.undoRedoEvent -= OnUndoRedo;
        }

        protected virtual void OnSceneGUI()
        {
            var chain = (LineChain)target;
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
                if (!MoveWithHandle(oldStartPosition, out Vector2Int newStartPosition))
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
                if (MoveWithHandle(oldEndPosition, out Vector2Int newEndPosition))
                {
                    Undo.RecordObject(chain, "Move Corner");
                    chain[^1] = chain[^1].WithEnd(newEndPosition);
                    ApplyChainChanges(chain);
                }
            }

            bool MoveWithHandle(Vector2Int originalGridPosition, out Vector2Int newGridPosition)
            {
                Vector3 newPosition = Handles.PositionHandle(chain.GetWorldPosition(originalGridPosition) * inverseHandleScale, Quaternion.identity) * handleScale;
                newGridPosition = chain.Grid.RoundToGrid(newPosition);
                return newGridPosition != originalGridPosition;
            }

            Handles.matrix = originalHandlesMatrix;
        }

        private void OnUndoRedo(in UndoRedoInfo undoRedoInfo)
        {
            var chain = (LineChain)target;
            ApplyChainChanges(chain);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var chain = (LineChain)target;

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
                LineChain.EditModeUtility.Invert(chain);
                ApplyChainChanges(chain);
            }

            DrawInsert(chain);
        }

        private void HandleChainCountChange(LineChain chain, int oldCount)
        {
            if (chain.Count <= oldCount)
            {
                chain[^1] = chain[^1].AsOpenChainEnd(!chain.Loop);
                return;
            }

            // chain.Count > oldCount
            int oldLastIndex = oldCount - 1;
            Line oldChainEnd = chain[oldLastIndex];

            chain[oldLastIndex] = chain[oldLastIndex].AsOpenChainEnd(false);

            for (int i = oldCount; i < chain.Count - 1; i++)
            {
                chain[i] = chain[i].WithStart(oldChainEnd.End).AsOpenChainEnd(false);
            }

            chain[^1] = chain[^1].WithStart(oldChainEnd.End).AsOpenChainEnd(!chain.Loop);
        }

        private void HandleChainIsLoopChange(LineChain chain)
        {
            // open -> loop
            if (chain.Loop)
            {
                chain[^1] = chain[^1].AsOpenChainEnd(false);
                chain.Append(chain[^1].End);
                return;
            }

            // loop -> open
            chain.RemoveLast();
            chain[^1] = chain[^1].AsOpenChainEnd(true);
        }

        private static void ApplyChainChanges(LineChain chain)
        {
            if (chain.Grid == null)
            {
                return;
            }

            Undo.RecordObject(chain, nameof(ApplyChainChanges));
            LineChain.EditModeUtility.EditModeFixLines(chain);
            LineChain.EditModeUtility.EditModeReevaluateClockwiseTurnWeight(chain);
            LineChain.EditModeUtility.EditModeRebuildLineRenderers(chain);
        }

        private void DrawInsert(LineChain chain)
        {
            EditorGUI.BeginChangeCheck();
            _isInsertExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_isInsertExpanded, "Insert");
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(IsInsertExpandedKey, _isInsertExpanded);
            }

            if (_isInsertExpanded)
            {
                DrawInsertContent(chain);
            }

            EditorGUI.EndFoldoutHeaderGroup();
        }

        private void DrawInsertContent(LineChain chain)
        {
            using var indent = new EditorGUI.IndentLevelScope(1);

            EditorGUI.BeginChangeCheck();
            _insertTarget = EditorGUILayout.ObjectField("Insert Target", _insertTarget, typeof(LineChain), true) as LineChain;
            if (EditorGUI.EndChangeCheck())
            {
                _insertTargetId = _insertTarget != null ? _insertTarget.GetInstanceID() : 0;
                EditorPrefs.SetInt(InsertTargetIdKey, _insertTargetId);
            }

            bool mayBeAbleToInsert = _insertTarget != null
                                     && _insertTarget != chain
                                     && chain.Loop
                                     && !_insertTarget.Loop;

            using (new EditorGUI.DisabledScope(!mayBeAbleToInsert))
            {
                if (GUILayout.Button("Insert"))
                {
                    LineChain.InsertUtility.Insert(chain, _insertTarget);
                }
            }
        }
    }
}