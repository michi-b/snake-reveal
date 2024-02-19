using System;
using UnityEditor;

namespace Game.Enemies.Editor
{
    [CustomEditor(typeof(MaintainSpeed))]
    public class MaintainSpeedEditor : SetStartVelocityEditor
    {
        private SerializedProperty _startVelocity;
        private SerializedProperty _speed;
        private SerializedProperty _applySpeedThreshold;

        public override void OnSceneGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnSceneGUI();
            if (EditorGUI.EndChangeCheck())
            {
                var maintainSpeed = (MaintainSpeed)target;
                Undo.RecordObject(maintainSpeed, "Change Maintain Speed");
                maintainSpeed.StartVelocity = maintainSpeed.StartVelocity.normalized * maintainSpeed.Speed;
            }
        }

        protected void OnEnable()
        {
            _startVelocity = serializedObject.FindProperty(SetStartVelocity.StartVelocityPropertyName);
            _speed = serializedObject.FindProperty(MaintainSpeed.SpeedPropertyName);
            _applySpeedThreshold = serializedObject.FindProperty(MaintainSpeed.EnforceSpeedThresholdPropertyName);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_speed);
            if (EditorGUI.EndChangeCheck())
            {
                _startVelocity.vector2Value = _startVelocity.vector2Value.normalized * _speed.floatValue;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_startVelocity);
            if (EditorGUI.EndChangeCheck())
            {
                _speed.floatValue = _startVelocity.vector2Value.magnitude;
            }

            EditorGUILayout.PropertyField(_applySpeedThreshold);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}