using System.Collections.Generic;
using UnityEditor;

namespace Game.Editor
{
    public class GameSettingsProvider : SettingsProvider
    {
        private GameSettingsProvider(string path, SettingsScope scopes) : base(path, scopes, new string[]
        {
            "Game"
        })
        {
        }

        public override void OnGUI(string searchContext)
        {
            GameSettings instance = GameSettings.instance;
            instance.DrawnShapeLayer = EditorGUILayout.LayerField("Drawn Shape Layer", instance.DrawnShapeLayer);
        }

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new GameSettingsProvider("Project/Game", SettingsScope.Project);
        }
    }
}