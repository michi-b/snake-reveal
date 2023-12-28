using Editor;
using Extensions;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineChain), true)]
    public class LineChainEditor : UnityEditor.Editor
    {
        private const string IsIntegrationExpandedKey = "LineChainEditor.IsIntegrationExpanded";
        private static readonly GUIContent TurnLabel = new(nameof(LineChain.Turn));
        private bool _isIntegrationExpanded;
        private SerializedProperty _linesProperty;
        private SerializedProperty _loopProperty;

        private Vector2Int _move;


        protected void OnEnable()
        {
            _linesProperty = serializedObject.FindProperty(LineChain.LinesPropertyName);
            _loopProperty = serializedObject.FindProperty(LineChain.LoopPropertyName);
            _isIntegrationExpanded = EditorPrefs.GetBool(IsIntegrationExpandedKey, false);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var chain = (LineChain)target;
            
            serializedObject.Update();
            EditorGUILayout.PropertyField(_loopProperty);
            EditorGUILayout.PropertyField(_linesProperty);
            if (serializedObject.ApplyModifiedProperties())
            {
                ApplyChainChanges(chain);
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

        private static void ApplyChainChanges(LineChain chain)
        {
            Undo.RecordObject(chain, nameof(ApplyChainChanges));
            chain.ReevaluateLinesFromStartPositions();
            chain.UpdateRenderersInEditMode();
        }

        private static void DrawAddRemoveCornerButtons(LineChain chain)
        {
            Rect line = EditorGUILayout.GetControlRect(false);
            line.TakeFromLeft(EditorGUI.indentLevel * EditorGuiUtility.IndentSize);
            Rect leftButtonRect = line.TakeFromLeft(Mathf.Floor(line.width * 0.5f));
            Rect rightButtonRect = line;

            using (new GUILayout.HorizontalScope())
            {
                if (GUI.Button(leftButtonRect, "-"))
                {
                    chain.RemoveLast();
                }

                // ReSharper disable once InvertIf
                if (GUI.Button(rightButtonRect, "+"))
                {
                    AppendCorner(chain);
                }
            }
        }

        private static void AppendCorner(LineChain chain)
        {
            bool isFirst = chain.Count == 0;
            Vector2Int position = isFirst
                ? chain.Grid != null ? chain.Grid.Size / 2 : new Vector2Int(0, 0)
                : chain[^1].Start;
            chain.Append(position);
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