using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Editor;
using Game.Enums;
using Game.Grid;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Utility;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineContainer), true)]
    public class LineContainerEditor : UnityEditor.Editor
    {
        private const string IsInsertExpandedKey = "LinkedLineListEditor.IsInsertionExpanded";
        private const string InsertTargetIdKey = "LinkedLineListEditor.InsertTargetId";
        private const string LinePositionHandleMoveOperationName = "Line Position Handle Move";

        private static readonly GUIContent InitializeCornersLabel = new("Initialize Corners", "Initialize the corners list and apply it to form a single line.");
        private static readonly GUIContent ApplyCornersLabel = new("Apply Corners", "Clear and rebuild all line GameObjects from the corners list.");

        private static readonly GUIContent ReverseCornersLabel = new("Reverse Corners", "Reverse the corners list and apply it.\n" +
                                                                                        "If the container is a loop, the first corner keeps its place.");

        private static readonly GUIContent MoveLabel = new("Move", "Move all corners by modifying this vector.");

        private readonly List<Vector2Int> _corners = new();
        private ReorderableList _cornersList;

        private SerializedProperty _startProperty;
        private SerializedProperty _hideLinesInSceneViewProperty;

        private LineContainer _insertTarget;
        private int _insertTargetId;
        private bool _isInsertExpanded;

        private Vector2Int _move;

        protected void OnEnable()
        {
            _startProperty = serializedObject.FindDirectChild(LineContainer.EditModeUtility.StartPropertyName);
            _hideLinesInSceneViewProperty = serializedObject.FindDirectChild(LineContainer.EditModeUtility.DisplayLinesInHierarchyPropertyName);
            _isInsertExpanded = EditorPrefs.GetBool(IsInsertExpandedKey, false);

            _insertTargetId = EditorPrefs.GetInt(InsertTargetIdKey, 0);
            if (_insertTargetId != 0)
            {
                _insertTarget = EditorUtility.InstanceIDToObject(_insertTargetId) as LineContainer;
            }

            ReadLinesIntoCorners();

            _cornersList = new ReorderableList(_corners, typeof(Vector2Int), true, true, true, true);
            _cornersList.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Corners");
            _cornersList.drawElementCallback += DrawCorner;
            _cornersList.onCanRemoveCallback += list => list.count > 2;
            _cornersList.onCanAddCallback += list => list.count >= 2;
            _cornersList.onAddCallback += CornersListAdd;
            _cornersList.onSelectCallback += _ => SceneView.RepaintAll();

            if (_cornersList.count > 0)
            {
                _cornersList.Select(0);
            }

            Undo.undoRedoEvent += OnUndoRedo;

            Tools.hidden = true;
        }

        private void CornersListAdd(ReorderableList list)
        {
            bool isLoop = LineContainer.EditModeUtility.GetIsLoop((LineContainer)target);
            ReadOnlyCollection<int> selectedIndices = _cornersList.selectedIndices;

            // evaluate "current index", which is either the selected one, or the last one if there is no selection
            int currentIndex = selectedIndices.Count == 0
                ? _cornersList.count - 1
                : selectedIndices[0];
            Vector2Int current = _corners[currentIndex];

            // evaluate an intuitive position of the new inserted or appended corner
            GridDirection flowDirection = currentIndex == _cornersList.count - 1
                ? isLoop
                    ? current.GetDirection(_corners[(currentIndex + 1) % _cornersList.count])
                    : _corners[currentIndex - 1].GetDirection(current) // current is last
                : current.GetDirection(_corners[currentIndex + 1]); // current is not last
            Vector2Int newPosition = current + flowDirection.ToVector2Int();

            int newIndex = currentIndex + 1;
            _corners.Insert(newIndex, newPosition);
            _cornersList.Select(newIndex);
        }

        private void OnUndoRedo(in UndoRedoInfo undo)
        {
            ReadLinesIntoCorners();

            // ensure that the selected index is not out of bounds
            if (_cornersList.selectedIndices.Count > 0)
            {
                int selectedIndex = _cornersList.selectedIndices[0];
                if (selectedIndex > _cornersList.count - 1)
                {
                    _cornersList.Select(_cornersList.count - 1);
                }
            }
        }

        protected virtual void OnSceneGUI()
        {
            var container = (LineContainer)target;

            if (!LineContainer.EditModeUtility.GetHasGridAndLineCache(container))
            {
                return;
            }

            SimulationGrid grid = LineContainer.EditModeUtility.GetGrid(container);
            Line start = LineContainer.EditModeUtility.GetStart(container);

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

            Line exclusiveEnd = LineContainer.EditModeUtility.GetExclusiveEnd(container);
            var skipFirstLineSpan = new LineSpan(start.Next, exclusiveEnd);
            foreach (Line lineAfterFirst in skipFirstLineSpan)
            {
                if (PositionHandle(lineAfterFirst.End, out Vector2Int newLineAfterStartEnd))
                {
                    Line.EditModeUtility.RecordUndoWithNeighbors(lineAfterFirst, LinePositionHandleMoveOperationName);
                    lineAfterFirst.End = newLineAfterStartEnd;
                }
            }

            if (anyPositionHandleChanged)
            {
                ReadLinesIntoCorners();
                Repaint();
            }

            bool PositionHandle(Vector2Int originalGridPosition, out Vector2Int newGridPosition)
            {
                Vector3 originalWorldPosition = container.GetWorldPosition(originalGridPosition);

                if (_cornersList.selectedIndices.Contains(handleIndex))
                {
                    Handles.DrawWireDisc(originalWorldPosition, Vector3.back, grid.SceneCellSize.magnitude * 0.5f);
                }

                EditorGUI.BeginChangeCheck();
                Vector3 newWorldPosition = Handles.PositionHandle(originalWorldPosition, Quaternion.identity);
                newGridPosition = grid.Round(newWorldPosition);


                if (EditorGUI.EndChangeCheck())
                {
                    _cornersList.Select(handleIndex);

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

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_startProperty);
            }

            var container = (LineContainer)target;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_hideLinesInSceneViewProperty);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                LineContainer.EditModeUtility.ApplyHideLinesInSceneView(container);
            }

            bool hasGridAndLineCache = LineContainer.EditModeUtility.GetHasGridAndLineCache(container);
            if (Application.isPlaying || !hasGridAndLineCache)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            _cornersList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                ApplyCorners(container);
            }

            if (_corners.Count < 2)
            {
                if (GUILayout.Button(InitializeCornersLabel))
                {
                    //ensure we got at least 2 positions
                    if (_corners.Count == 0)
                    {
                        _corners.Add(LineContainer.EditModeUtility.GetGrid(container).CenterPosition);
                    }

                    if (_corners.Count == 1)
                    {
                        _corners.Add(_corners[0] + Vector2Int.right);
                    }

                    ApplyCorners(container);
                }
            }
            else
            {
                if (GUILayout.Button(ApplyCornersLabel))
                {
                    ApplyCorners(container);
                }

                if (GUILayout.Button(ReverseCornersLabel))
                {
                    _corners.Reverse(1, _corners.Count - 1);
                    ApplyCorners(container);
                }
            }

            EditorGUI.BeginChangeCheck();
            _move = EditorGUILayout.Vector2IntField(MoveLabel, _move);
            if (EditorGUI.EndChangeCheck() && _move != Vector2Int.zero)
            {
                for (int i = 0; i < _corners.Count; i++)
                {
                    _corners[i] = _corners[i] + _move;
                }

                ApplyCorners(container);
                _move = Vector2Int.zero;
            }

            DrawInsert(container);
        }

        private void ReadLinesIntoCorners()
        {
            _corners.Clear();
            Line start = LineContainer.EditModeUtility.GetStart((LineContainer)target);
            if (start != null)
            {
                _corners.Add(start.Start);
                _corners.Add(start.End);
                bool isLoop = LineContainer.EditModeUtility.GetIsLoop((LineContainer)target);
                Line exclusiveEnd = isLoop ? start.Previous : null;
                List<Line> lineSpan = new LineSpan(start.Next, exclusiveEnd).ToList();
                _corners.AddRange(lineSpan.Select(line => line.End));
            }
        }

        private void DrawCorner(Rect rect, int index, bool isActive, bool isFocused)
        {
            _corners[index] = EditorGUI.Vector2IntField(rect, GUIContent.none, _corners[index]);
        }

        protected void OnDisable()
        {
            _cornersList.drawElementCallback -= DrawCorner;
            Undo.undoRedoEvent -= OnUndoRedo;
            Tools.hidden = false;
        }

        private void ApplyCorners(LineContainer container)
        {
            LineContainer.EditModeUtility.Rebuild(container, _corners);
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
                                     && LineContainer.EditModeUtility.GetIsLoop(container)
                                     && !LineContainer.EditModeUtility.GetIsLoop(_insertTarget);

            using (new EditorGUI.DisabledScope(!mayBeAbleToInsert))
            {
                if (GUILayout.Button("Insert"))
                {
                    LineContainer.EditModeUtility.Insert(container, _insertTarget);
                }
            }
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


#endif
    }
}