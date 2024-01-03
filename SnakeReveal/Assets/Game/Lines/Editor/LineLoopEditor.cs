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
        private const string IsInsertExpandedKey = "LinkedLineListEditor.IsInsertionExpanded";
        private const string InsertTargetIdKey = "LinkedLineListEditor.InsertTargetId";

        private SerializedProperty _clockwiseTurnWeightProperty;

        private LineChain _insertTarget;
        private int _insertTargetId;
        private bool _isInsertExpanded;
        private SerializedProperty _turnProperty;

        public override bool IsLoop => true;

        protected override void OnEnable()
        {
            base.OnEnable();

            _clockwiseTurnWeightProperty = serializedObject.FindDirectChild(LineLoop.ClockwiseTurnWeightFieldName);
            _turnProperty = serializedObject.FindDirectChild(LineLoop.TurnFieldName);

            _isInsertExpanded = EditorPrefs.GetBool(IsInsertExpandedKey, false);

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

        protected override void DrawDerivedProperties()
        {
            using var disabledScope = new EditorGUI.DisabledScope(true);
            EditorGUILayout.PropertyField(_clockwiseTurnWeightProperty);
            EditorGUILayout.PropertyField(_turnProperty);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var loop = (LineLoop)target;
            DrawInsert(loop);
        }

        private void DrawInsert(LineLoop loop)
        {
            EditorGUI.BeginChangeCheck();
            _isInsertExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_isInsertExpanded, "Insert");
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(IsInsertExpandedKey, _isInsertExpanded);
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

            using (new EditorGUI.DisabledScope(_insertTarget == null))
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
                }
            }
        }
    }
}