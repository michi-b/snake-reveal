using Editor;
using Extensions;
using UnityEditor;
using UnityEngine;

namespace Game.Enemies.Editor
{
    [CustomEditor(typeof(SetStartVelocity))]
    public class SetStartVelocityEditor : UnityEditor.Editor
    {
        public void OnSceneGUI()
        {
            if (!Application.isPlaying)
            {
                var setStartVelocity = (SetStartVelocity) target;
                Vector3 arrowEnd = setStartVelocity.transform.position + setStartVelocity.StartVelocity.ToVector3(0f);
                EditorGUI.BeginChangeCheck();
                arrowEnd = Handles.PositionHandle(arrowEnd, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(setStartVelocity, "Move Start Velocity");
                    setStartVelocity.StartVelocity = (Vector2)(arrowEnd - setStartVelocity.transform.position);
                }
            }
        }
    }
}