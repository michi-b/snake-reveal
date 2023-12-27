using System;
using Editor;
using Extensions;
using Game.Enums;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Lines.Editor
{
    [CustomEditor(typeof(LineChain), true)]
    public class LineChainEditor : UnityEditor.Editor
    {
        private static GUIStyle _rightAlignedLabelStyle;

        private static readonly GUIContent TurnLabel = new(nameof(LineChain.Turn));
        private SerializedProperty _cornersProperty;

        protected void OnEnable()
        {
            _cornersProperty = serializedObject.FindProperty(LineChain.CornersPropertyName);
            _isIntegrationExpanded = EditorPrefs.GetBool(IsIntegrationExpandedKey, false);
            _integrationTargetId = EditorPrefs.GetInt(IntegrationTargetIdKey, 0);
            if (_integrationTargetId > 0)
            {
                _integrationTarget = EditorUtility.InstanceIDToObject(_integrationTargetId) as LineChain;
            }
        }

        private const string IsIntegrationExpandedKey = "LineChainEditor.IsIntegrationExpanded";
        private bool _isIntegrationExpanded;

        public override void OnInspectorGUI()
        {
            using var changeCheck = new EditorGUI.ChangeCheckScope();
            
            _rightAlignedLabelStyle ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            base.OnInspectorGUI();

            var chain = (LineChain)target;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup(TurnLabel, chain.Turn);
            }
            
            if (changeCheck.changed && !Application.isPlaying)
            {
                chain.EvaluateTurn();
                chain.UpdateRenderersInEditMode();
            }

            DrawIntegration(chain);
        }

        private static void DrawCornerCount(LineChain chain)
        {
            using var changeCheck = new EditorGUI.ChangeCheckScope();

            int count = EditorGUILayout.IntField("Corners", chain.Count);
            count = Mathf.Max(0, count);

            if (!changeCheck.changed)
            {
                return;
            }

            int delta = count - chain.Count;
            if (delta > 0)
            {
                for (int i = 0; i < delta; i++)
                {
                    AppendCorner(chain);
                }
            }
            else
            {
                delta = Math.Abs(delta);
                for (int i = 0; i < delta; i++)
                {
                    chain.RemoveLast();
                }
            }
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
            int2 position = isFirst
                ? chain.Grid != null ? chain.Grid.Size / 2 : new int2(0, 0)
                : chain[^1];
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
            EditorGUI.BeginChangeCheck();
            _integrationTarget = EditorGUILayout.ObjectField("Target", _integrationTarget, typeof(LineChain), true) as LineChain;   
            if(EditorGUI.EndChangeCheck())
            {
                _integrationTargetId = _integrationTarget ? _integrationTarget.GetInstanceID() : 0;
                EditorPrefs.SetInt(IntegrationTargetIdKey, _integrationTargetId);
            }

            using (new EditorGUI.DisabledScope(_integrationTarget == null
                                               || _integrationTarget == chain
                                               || !chain.Loop
                                               || _integrationTarget.Loop))
            {
                if (GUILayout.Button("Integrate"))
                {
                    LineChainIntegration.Integrate(chain, _integrationTarget);
                }
            }
        }
        
        private const string IntegrationTargetIdKey = "LineChainEditor.IntegrationTargetId";
        private int _integrationTargetId;
        
        private LineChain _integrationTarget;
    }
}