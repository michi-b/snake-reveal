using UnityEditor;

namespace Game.Editor
{
    public class GameSettingsProvider : SettingsProvider
    {
        private GameSettingsProvider(string path, SettingsScope scopes) : base(path, scopes, new[]
        {
            "Game"
        })
        {
        }

        public override void OnGUI(string searchContext)
        {
            GameSettings instance = GameSettings.instance;

            _areLayersExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_areLayersExpanded, "Is Captured in Drawn Shape Check");
            if (_areLayersExpanded)
            {
                EditorGUI.indentLevel++;
                instance.IsCapturedInDrawnShapeCheckLayer = EditorGUILayout.LayerField("Layer", instance.IsCapturedInDrawnShapeCheckLayer);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new GameSettingsProvider("Project/Game", SettingsScope.Project);
        }

        private bool _areLayersExpanded = true;
    }
}