using System;
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
    [CustomEditor(typeof(LineContainer), false)]
    public abstract class LineContainerEditor : UnityEditor.Editor
    {
        private const string LinePositionHandleMoveOperationName = "Line Position Handle Move";

        private static readonly GUIContent InitializeCornersLabel = new("Initialize Corners", "Initialize the corners list and apply it to form a single line.");

        private static readonly GUIContent ReadCornersLabel = new("Read Corners", "Read the corners from connected lines starting at the start line.");

        private static readonly GUIContent ReverseCornersLabel = new("Reverse", "Reverse the corners list and apply it.\n" +
                                                                                "If the container is a loop, the first corner keeps its place.");

        private static readonly GUIContent ApplyCornersLabel = new("Apply Corners", "Clear and rebuild all line GameObjects from the corners list.");

        private static readonly GUIContent MoveLabel = new("Move", "Move all corners by modifying this vector.");
        
        private SerializedProperty _hideLinesInSceneViewProperty;
        private SerializedProperty _clockwiseTurnWeightProperty;
        private SerializedProperty _startProperty;

        private readonly List<Vector2Int> _corners = new();
        private ReorderableList _cornersList;

        private Vector2Int _move;


        protected virtual IEnumerable<LineContainer> AdditionalHandlesTargets => Array.Empty<LineContainer>();

        protected virtual void OnEnable()
        {
            _startProperty = serializedObject.FindDirectChild(LineContainer.EditModeUtility.StartPropertyName);
            _hideLinesInSceneViewProperty = serializedObject.FindDirectChild(LineContainer.EditModeUtility.DisplayLinesInHierarchyPropertyName);
            _clockwiseTurnWeightProperty = serializedObject.FindDirectChild(LineContainer.EditModeUtility.ClockwiseTurnWeightPropertyName);

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
                _cornersList.Select(GetInitialSelectionIndex(_cornersList.count));
            }

            Undo.undoRedoEvent += OnUndoRedo;

            Tools.hidden = true;
        }

        protected void OnDisable()
        {
            _cornersList.drawElementCallback -= DrawCorner;
            Undo.undoRedoEvent -= OnUndoRedo;
            Tools.hidden = false;
        }

        protected virtual void OnSceneGUI()
        {
            var container = (LineContainer)target;

            DrawHandles(container, true);

            foreach (LineContainer additionalHandlesTarget in AdditionalHandlesTargets)
            {
                DrawHandles(additionalHandlesTarget, false);
            }
        }

        private void CornersListAdd(ReorderableList list)
        {
            ReadOnlyCollection<int> selectedIndices = _cornersList.selectedIndices;

            // evaluate "current index", which is either the selected one, or the last one if there is no selection
            int currentIndex = selectedIndices.Count == 0
                ? _cornersList.count - 1
                : selectedIndices[0];
            Vector2Int current = _corners[currentIndex];

            // evaluate an intuitive position of the new inserted or appended corner
            GridDirection flowDirection = currentIndex == _cornersList.count - 1
                ? ((LineContainer)target).Loop
                    ? current.GetDirection(_corners[(currentIndex + 1) % _cornersList.count])
                    : _corners[currentIndex - 1].GetDirection(current) // current is last
                : current.GetDirection(_corners[currentIndex + 1]); // current is not last
            Vector2Int newPosition = current + flowDirection.ToVector2Int();

            int newIndex = currentIndex + 1;
            _corners.Insert(newIndex, newPosition);
            _cornersList.Select(newIndex);
        }

        protected abstract int GetInitialSelectionIndex(int count);

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

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawProperties();

            var container = (LineContainer)target;

            EditorGUI.BeginChangeCheck();
            SerializedProperty hideProp = _hideLinesInSceneViewProperty;
            hideProp.boolValue = EditorGUILayout.ToggleLeft(hideProp.displayName, hideProp.boolValue);
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
                if (GUILayout.Button(ReadCornersLabel))
                {
                    ReadLinesIntoCorners();
                }

                if (GUILayout.Button(ReverseCornersLabel))
                {
                    if (container.Loop)
                    {
                        _corners.Reverse(1, _corners.Count - 1);
                    }
                    else
                    {
                        _corners.Reverse();
                    }

                    ApplyCorners(container);
                }

                if (GUILayout.Button(ApplyCornersLabel))
                {
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
        }

        protected virtual void DrawProperties()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_startProperty);
            }
            EditorGUILayout.PropertyField(_clockwiseTurnWeightProperty);
        }

        private void DrawHandles(LineContainer container, bool drawThisContainer)
        {
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
                LineContainer.EditModeUtility.PostProcessLineChanges(container);
                if (drawThisContainer)
                {
                    ReadLinesIntoCorners();
                    Repaint();
                }
            }

            bool PositionHandle(Vector2Int originalGridPosition, out Vector2Int newGridPosition)
            {
                Vector3 originalWorldPosition = container.GetWorldPosition(originalGridPosition);

                if (drawThisContainer && _cornersList.selectedIndices.Contains(handleIndex))
                {
                    Handles.DrawWireDisc(originalWorldPosition, Vector3.back, grid.SceneCellSize.magnitude * 0.5f);
                }

                EditorGUI.BeginChangeCheck();
                Vector3 newWorldPosition = Handles.PositionHandle(originalWorldPosition, Quaternion.identity);
                newGridPosition = grid.Round(newWorldPosition);


                if (EditorGUI.EndChangeCheck())
                {
                    if (drawThisContainer)
                    {
                        _cornersList.Select(handleIndex);
                    }

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

        private void ReadLinesIntoCorners()
        {
            _corners.Clear();
            Line start = LineContainer.EditModeUtility.GetStart((LineContainer)target);
            if (start != null)
            {
                _corners.Add(start.Start);
                _corners.Add(start.End);
                Line exclusiveEnd = LineContainer.EditModeUtility.GetExclusiveEnd((LineContainer)target);
                // ReSharper disable once Unity.NoNullPropagation
                List<Line> lineSpan = new LineSpan(start.Next, exclusiveEnd?.Previous).ToList();
                _corners.AddRange(lineSpan.Select(line => line.End));
            }
        }

        private void DrawCorner(Rect rect, int index, bool isActive, bool isFocused)
        {
            _corners[index] = EditorGUI.Vector2IntField(rect, GUIContent.none, _corners[index]);
        }

        private void ApplyCorners(LineContainer container)
        {
            LineContainer.EditModeUtility.Rebuild(container, _corners);
            LineContainer.EditModeUtility.PostProcessLineChanges(container);
        }
    }
}