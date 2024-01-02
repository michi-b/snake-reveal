using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Editor;
using Game.Grid;
using UnityEditor;
using UnityEditorInternal;
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

            ReadLinesIntoPositions();

            _positionsList = new ReorderableList(_positions, typeof(Vector2Int), true, true, true, true);
            _positionsList.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Positions");
            _positionsList.drawElementCallback += DrawPositionsListElement;
            _positionsList.onCanRemoveCallback += list => list.count > 2;
            _positionsList.onCanAddCallback += list => list.count >= 2;
            _positionsList.onAddCallback += PositionsListAdd;
            _positionsList.onSelectCallback += list => SceneView.RepaintAll();

            Undo.undoRedoEvent += OnUndoRedo;

            Tools.hidden = true;
        }

        private void PositionsListAdd(ReorderableList list)
        {
            ReadOnlyCollection<int> selectedIndices = _positionsList.selectedIndices;
            if (selectedIndices.Count != 1)
            {
                _positions.Add(_positions.Last() + Vector2Int.right);
            }
            else
            {
                int selectedIndex = selectedIndices[0];
                Vector2Int newPosition = _positions[selectedIndex] + Vector2Int.right;
                _positions.Insert(selectedIndex + 1, newPosition);
            }
        }

        private void OnUndoRedo(in UndoRedoInfo undo)
        {
            ReadLinesIntoPositions();
        }

        private void ReadLinesIntoPositions()
        {
            _positions.Clear();
            Line start = DoubleLinkedLineList.EditModeUtility.GetStart((DoubleLinkedLineList)target);
            if (start != null)
            {
                _positions.Add(start.Start);
                _positions.Add(start.End);
                _positions.AddRange(start.SkipFirst().Select(line => line.End));
            }
        }

        private void DrawPositionsListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            _positions[index] = EditorGUI.Vector2IntField(rect, GUIContent.none, _positions[index]);
        }

        private ReorderableList _positionsList;

        protected void OnDisable()
        {
            _positionsList.drawElementCallback -= DrawPositionsListElement;
            Undo.undoRedoEvent -= OnUndoRedo;
            Tools.hidden = false;
        }

        protected virtual void OnSceneGUI()
        {
            var container = (DoubleLinkedLineList)target;
            if (!IsFullyAssigned)
            {
                return;
            }

            SimulationGrid grid = DoubleLinkedLineList.EditModeUtility.GetGrid(container);
            Matrix4x4 originalHandlesMatrix = Handles.matrix;


            Line start = DoubleLinkedLineList.EditModeUtility.GetStart(container);
            if (start == null)
            {
                return;
            }

            bool anyPositionHandleChanged = false;
            int handleIndex = 0;

            if (PositionHandle(start.Start, out Vector2Int newStartStart))
            {
                Line.EditModeUtility.RecordUndoWithNeighbors(start, LinePositionHandleMoveOperationName);
                start.Start = newStartStart;
            }

            if (PositionHandle(start.End, out Vector2Int newStartEnd))
            {
                Line.EditModeUtility.RecordUndoWithNeighbors(start, LinePositionHandleMoveOperationName);
                start.End = newStartEnd;
            }

            foreach (Line lineAfterFirst in start.SkipFirst())
            {
                if (PositionHandle(lineAfterFirst.End, out Vector2Int newLineAfterStartEnd))
                {
                    Line.EditModeUtility.RecordUndoWithNeighbors(lineAfterFirst, LinePositionHandleMoveOperationName);
                    lineAfterFirst.End = newLineAfterStartEnd;
                }
            }

            if (anyPositionHandleChanged)
            {
                ReadLinesIntoPositions();
                Repaint();
            }

            bool PositionHandle(Vector2Int originalGridPosition, out Vector2Int newGridPosition)
            {
                Vector3 originalWorldPosition = container.GetWorldPosition(originalGridPosition);

                if (_positionsList.selectedIndices.Contains(handleIndex))
                {
                    Handles.DrawWireDisc(originalWorldPosition, Vector3.back, grid.SceneCellSize.magnitude * 0.5f);
                }

                EditorGUI.BeginChangeCheck();
                Vector3 newWorldPosition = Handles.PositionHandle(originalWorldPosition, Quaternion.identity);
                newGridPosition = grid.Round(newWorldPosition);


                if (EditorGUI.EndChangeCheck())
                {
                    _positionsList.Select(handleIndex);

                    if (newGridPosition != originalGridPosition)
                    {
                        anyPositionHandleChanged = true;
                        handleIndex++;
                        return true;
                    }
                }

                handleIndex++;
                return false;
            }
        }


        // private void OnUndoRedo(in UndoRedoInfo undoRedoInfo)
        // {
        //     var chain = (DoubleLinkedLineList)target;
        //     ApplyChainChanges(chain);
        // }

        private readonly List<Vector2Int> _positions = new();

        private bool IsFullyAssigned => DoubleLinkedLineList.EditModeUtility.GetIsFullyAssigned((DoubleLinkedLineList)target);

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_startProperty);
            }

            if (Application.isPlaying || !IsFullyAssigned)
            {
                return;
            }

            var container = (DoubleLinkedLineList)target;

            EditorGUI.BeginChangeCheck();
            _positionsList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                ApplyPositions(container);
            }

            if (_positions.Count < 2 && GUILayout.Button("Initialize Positions"))
            {
                //ensure we got at least 2 positions
                if (_positions.Count == 0)
                {
                    _positions.Add(DoubleLinkedLineList.EditModeUtility.GetGrid(container).CenterPosition);
                }

                if (_positions.Count == 1)
                {
                    _positions.Add(_positions[0] + Vector2Int.right);
                }

                ApplyPositions(container);
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

        private void ApplyPositions(DoubleLinkedLineList container)
        {
            DoubleLinkedLineList.EditModeUtility.Rebuild(container, _positions);
            ReadLinesIntoPositions();
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