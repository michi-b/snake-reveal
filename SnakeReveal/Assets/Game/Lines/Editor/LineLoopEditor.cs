using System.Collections.Generic;
using System.Linq;
using Editor;
using Game.Enums;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineLoop))]
    public class LineLoopEditor : LineContainerEditor
    {
        private const string IsInsertExpandedKey = "LineLoopEditor.IsInsertionExpanded";
        private const string DeactivateTargetOnInsertKey = "LineLoopEditor.DeactivateTargetOnInsert";
        private const string InsertTargetIdKey = "LineLoopEditor.InsertTargetId";
        private const string InitializationSizeKey = "LineLoopEditor.InitializationSize";
        private bool _deactivateTargetOnInsert;
        private int _initializationSize;

        private LineChain _insertTarget;
        private int _insertTargetId;
        private bool _isInsertExpanded;
        private SerializedProperty _turnProperty;

        protected override IEnumerable<LineContainer> AdditionalHandlesTargets
        {
            get
            {
                if (_insertTarget != null && _isInsertExpanded && _insertTarget.gameObject.activeInHierarchy)
                {
                    yield return _insertTarget;
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _turnProperty = serializedObject.FindDirectChild(LineLoop.TurnFieldName);

            _isInsertExpanded = EditorPrefs.GetBool(IsInsertExpandedKey, false);
            _deactivateTargetOnInsert = EditorPrefs.GetBool(DeactivateTargetOnInsertKey, false);
            _initializationSize = EditorPrefs.GetInt(InitializationSizeKey, 10);
            _insertTargetId = EditorPrefs.GetInt(InsertTargetIdKey, 0);
            if (_insertTargetId != 0)
            {
                _insertTarget = EditorUtility.InstanceIDToObject(_insertTargetId) as LineChain;
            }
        }

        protected override int GetInitialSelectionIndex(int count)
        {
            return 0;
        }

        protected override void DrawProperties()
        {
            base.DrawProperties();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_turnProperty);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var loop = (LineLoop)target;
            DrawInsert(loop);
        }

        protected override void DrawInitializeCornersButton(LineContainer container)
        {
            EditorGUI.BeginChangeCheck();
            _initializationSize = EditorGUILayout.IntField("Rectangle Size", _initializationSize);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetInt(InitializationSizeKey, _initializationSize);
            }

            base.DrawInitializeCornersButton(container);
        }

        protected override void InitializeCorners(List<Vector2Int> corners)
        {
            int size = _initializationSize;
            var container = (LineLoop)target;
            Vector2Int center = container.Grid.CenterPosition;
            corners.Add(center + new Vector2Int(-size, size));
            corners.Add(center + new Vector2Int(size, size));
            corners.Add(center + new Vector2Int(size, -size));
            corners.Add(center + new Vector2Int(-size, -size));
        }

        private void DrawInsert(LineLoop loop)
        {
            EditorGUI.BeginChangeCheck();
            _isInsertExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_isInsertExpanded, "Insert");
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(IsInsertExpandedKey, _isInsertExpanded);
                SceneView.RepaintAll();
            }

            if (_isInsertExpanded)
            {
                DrawInsertContent(loop);
            }

            EditorGUI.EndFoldoutHeaderGroup();
        }

        private void DrawInsertContent(LineLoop loop)
        {
            using var indent = new EditorGUI.IndentLevelScope(1);

            EditorGUI.BeginChangeCheck();
            _insertTarget = EditorGUILayout.ObjectField("Insert Target", _insertTarget, typeof(LineChain), true) as LineChain;
            if (EditorGUI.EndChangeCheck())
            {
                _insertTargetId = _insertTarget != null ? _insertTarget.GetInstanceID() : 0;
                EditorPrefs.SetInt(InsertTargetIdKey, _insertTargetId);
            }

            EditorGUI.BeginChangeCheck();
            _deactivateTargetOnInsert = EditorGUILayout.ToggleLeft("Deactivate Target On Insert", _deactivateTargetOnInsert);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(DeactivateTargetOnInsertKey, _deactivateTargetOnInsert);
            }

            using (new EditorGUI.DisabledScope(_insertTarget == null || loop.Turn == Turn.None))
            {
                if (GUILayout.Button("Insert"))
                {
                    if (loop.Any(line => line.Direction == GridDirection.None))
                    {
                        Debug.LogWarning("Insertion cancelled because loop contains lines with  \"NONE\" direction");
                        return;
                    }

                    // ReSharper disable once AssignNullToNotNullAttribute
                    if (_insertTarget.Any(line => line.Direction == GridDirection.None))
                    {
                        Debug.LogWarning("Insertion cancelled because target chain contains lines with  \"NONE\" direction");
                        return;
                    }

                    loop.EditModeInsert(_insertTarget);
                    ReadLinesIntoCorners();

                    if (_deactivateTargetOnInsert)
                    {
                        Undo.RegisterFullObjectHierarchyUndo(_insertTarget.gameObject, "Deactivate Insert Target");
                        _insertTarget.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}