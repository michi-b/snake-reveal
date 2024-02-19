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
            GameSettings instance = GameSettings.instance;
            _serializedObject = new SerializedObject(instance);
            _shapeCaptureLayers = _serializedObject.FindProperty(GameSettings.ShapeCaptureLayersPropertyName);
            _shapeEscapeLayers = _serializedObject.FindProperty(GameSettings.ShapeEscapeLayersPropertyName);
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUI.BeginChangeCheck();
            _areLayersExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_areLayersExpanded, "Shape Capture Check");
            if (_areLayersExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_shapeCaptureLayers);
                EditorGUILayout.PropertyField(_shapeEscapeLayers);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                GameSettings.instance.Save();
            }
        }

        [SettingsProvider]
        public static SettingsProvider Create() => new GameSettingsProvider("Project/Game", SettingsScope.Project);

        private bool _areLayersExpanded = true;
        private readonly SerializedObject _serializedObject;
        private readonly SerializedProperty _shapeCaptureLayers;
        private readonly SerializedProperty _shapeEscapeLayers;
    }
}