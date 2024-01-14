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

        private readonly List<Vector2Int> _corners = new();
        private SerializedProperty _clockwiseTurnWeightProperty;
        private ReorderableList _cornersList;

        private SerializedProperty _hideLinesInSceneViewProperty;

        protected SerializedProperty StartProperty;

        protected virtual IEnumerable<LineContainer> AdditionalHandlesTargets => Array.Empty<LineContainer>();

        protected virtual void OnEnable()
        {
            StartProperty = serializedObject.FindDirectChild(LineContainer.EditModeUtility.StartPropertyName);
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

            bool hasGridAndLineCache = container.Grid != null && container.Cache != null;
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
                DrawInitializeCornersButton(container);
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
        }

        protected virtual void DrawInitializeCornersButton(LineContainer container)
        {
            if (GUILayout.Button(InitializeCornersLabel))
            {
                //ensure we got at least 2 positions
                _corners.Clear();
                InitializeCorners(_corners);
                ApplyCorners(container);
            }
        }

        protected virtual void InitializeCorners(List<Vector2Int> corners)
        {
            var container = (LineContainer)target;
            corners.Add(container.Grid.CenterPosition);
            corners.Add(_corners[0] + Vector2Int.right);
        }

        protected virtual void DrawProperties()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(StartProperty);
                EditorGUILayout.PropertyField(_clockwiseTurnWeightProperty);
            }
        }

        private void DrawHandles(LineContainer container, bool drawThisContainer)
        {
            SimulationGrid grid = container.Grid;
            if (grid == null || container.Cache == null)
            {
                return;
            }

            Line startLine = container.Start;

            if (startLine == null || container.End == null)
            {
                return;
            }

            bool anyPositionHandleChanged = false;
            int handleIndex = 0;

            if (PositionHandle(startLine.Start, out Vector2Int newStartStart))
            {
                startLine.RegisterUndoWithNeighbors(LinePositionHandleMoveOperationName);
                startLine.Start = newStartStart;
            }

            if (PositionHandle(startLine.End, out Vector2Int newStartEnd))
            {
                startLine.RegisterUndoWithNeighbors(LinePositionHandleMoveOperationName);
                startLine.End = newStartEnd;
            }

            Vector2Int positionsSum = startLine.Start + startLine.End;
            int positionsCount = 2;

            if (container.Start != container.End)
            {
                var skipFirstLineSpan = new LineSpan(startLine.Next, container.End);
                foreach (Line lineAfterFirst in skipFirstLineSpan)
                {
                    if (PositionHandle(lineAfterFirst.End, out Vector2Int newLineAfterStartEnd))
                    {
                        lineAfterFirst.RegisterUndoWithNeighbors(LinePositionHandleMoveOperationName);
                        lineAfterFirst.End = newLineAfterStartEnd;
                    }

                    positionsSum += lineAfterFirst.End;
                    positionsCount++;
                }
            }

            Vector2Int centerPosition = positionsSum / positionsCount;

            if (HandlesUtility.TryGridHandleMove(centerPosition, container.transform.position.z, grid, out Vector2Int newCenter))
            {
                Vector2Int delta = newCenter - centerPosition;
                foreach (Line line in container)
                {
                    Undo.RegisterFullObjectHierarchyUndo(line.gameObject, "Move Line Container Center Grid Handle");
                    line.Start += delta;
                }

                anyPositionHandleChanged = true;
            }

            if (anyPositionHandleChanged)
            {
                LineContainer.EditModeUtility.PostProcessLineChanges(container);
                if (drawThisContainer)
                {
                    ReadLinesIntoCorners();
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

        protected void ReadLinesIntoCorners()
        {
            _corners.Clear();
            var container = (LineContainer)target;
            Line start = container.Start;
            Line end = container.End;
            if (start != null && end != null)
            {
                _corners.Add(start.Start);

                // skip last line if container is looping (it'd be the same corner as the first one)
                Line lastLine = container.Loop ? end.Previous : end;

                List<Vector2Int> endPositions = new LineSpan(start, lastLine).Select(line => line.End).ToList();

                _corners.AddRange(endPositions);
            }

            Repaint();
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