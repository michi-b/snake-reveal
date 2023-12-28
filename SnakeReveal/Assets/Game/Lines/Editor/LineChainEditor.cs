using System;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineChain), true)]
    public class LineChainEditor : UnityEditor.Editor
    {
        private const string IsIntegrationExpandedKey = "LineChainEditor.IsIntegrationExpanded";
        private SerializedProperty _clockwiseTurnWeightProperty;
        private bool _isIntegrationExpanded;
        private SerializedProperty _linesProperty;
        private SerializedProperty _loopProperty;

        private Vector2Int _move;

        protected void OnEnable()
        {
            _linesProperty = serializedObject.FindProperty(LineChain.LinesPropertyName);
            _loopProperty = serializedObject.FindProperty(LineChain.LoopPropertyName);
            _clockwiseTurnWeightProperty = serializedObject.FindProperty(LineChain.ClockwiseTurnWeightPropertyName);
            _isIntegrationExpanded = EditorPrefs.GetBool(LineChain.ClockwiseTurnWeightPropertyName, false);

            Undo.undoRedoEvent += OnUndoRedo;

            ApplyChainChanges((LineChain)target);
        }


        protected void OnDisable()
        {
            Undo.undoRedoEvent -= OnUndoRedo;
        }

        protected void OnSceneGUI()
        {
            var chain = (LineChain)target;
            if (chain.Grid == null)
            {
                return;
            }

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
                Vector3 newPosition = Handles.PositionHandle(chain.GetWorldPosition(originalGridPosition), Quaternion.identity);
                newGridPosition = chain.Grid.RoundToGrid(newPosition);
                return newGridPosition != originalGridPosition;
            }
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

            DrawIntegration(chain);
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

        private void HandleChainCountUpdate(LineChain chain)
        {
            throw new NotImplementedException();
        }

        private static void ApplyChainChanges(LineChain chain)
        {
            if (chain.Grid == null)
            {
                return;
            }

            Undo.RecordObject(chain, nameof(ApplyChainChanges));
            chain.EditModeFixLines();
            chain.EditModeReevaluateClockwiseTurnWeight();
            chain.EditModeRebuildLineRenderers();
        }

        private void DrawIntegration(LineChain chain)
        {
            EditorGUI.BeginChangeCheck();
            _isIntegrationExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_isIntegrationExpanded, "Integration");
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(IsIntegrationExpandedKey, _isIntegrationExpanded);
            }

            if (_isIntegrationExpanded)
            {
                DrawIntegrationExpansion(chain);
            }

            EditorGUI.EndFoldoutHeaderGroup();
        }

        private void DrawIntegrationExpansion(LineChain chain)
        {
        }
    }
}