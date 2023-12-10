using UnityEditor;

namespace Game.Player.Editor
{
    [CustomEditor(typeof(PlayerActor))]
    public class PlayerActorEditor : UnityEditor.Editor
    {
        private bool _applyGridPosition;
        private SimulationGrid _grid;

        private void OnEnable()
        {
            _applyGridPosition = EditorPrefs.GetBool("Game.Player.Editor.PlayerActorEditor.ApplyGridPosition");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            bool changed = EditorGUI.EndChangeCheck();

            if (_grid == null)
            {
                _grid = FindAnyObjectByType<SimulationGrid>();
            }

            if (_grid == null)
            {
                EditorGUILayout.HelpBox("Missing grid", MessageType.Error);
                return;
            }

            EditorGUILayout.Space();

            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                _applyGridPosition = EditorGUILayout.Toggle("Apply Grid Position", _applyGridPosition);
                if (scope.changed)
                {
                    EditorPrefs.SetBool("Game.Player.Editor.PlayerActorEditor.ApplyGridPosition", _applyGridPosition);
                }
            }

            if (_applyGridPosition && changed)
            {
                var playerActor = (PlayerActor)target;
                playerActor.ApplyPosition(_grid);
            }
        }
    }
}