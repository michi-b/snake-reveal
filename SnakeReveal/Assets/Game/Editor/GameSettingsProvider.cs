using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class GameSettingsProvider : SettingsProvider
    {
        private GameSettingsProvider(string path, SettingsScope scopes) : base(path, scopes, new[]
        {
            "Game"
        })
        {
            _settings = GameSettings.instance;
            _serializedObject = new SerializedObject(_settings);
            _shapeCaptureLayers = _serializedObject.FindProperty(GameSettings.ShapeCaptureLayersPropertyName);
            _shapeEscapeLayers = _serializedObject.FindProperty(GameSettings.ShapeEscapeLayersPropertyName);
        }

        public override void OnGUI(string searchContext)
        {
            _areLayersExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_areLayersExpanded, "Shape Capture Check");
            if (_areLayersExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_shapeCaptureLayers);
                EditorGUILayout.PropertyField(_shapeEscapeLayers);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            // HACK: normally I would only save on gui change, but it seems to not register
            _serializedObject.ApplyModifiedProperties();
            _settings.Save();
        }

        [SettingsProvider]
        public static SettingsProvider Create() => new GameSettingsProvider("Project/Game", SettingsScope.Project);

        private bool _areLayersExpanded = true;
        private readonly GameSettings _settings;
        private readonly SerializedObject _serializedObject;
        private readonly SerializedProperty _shapeCaptureLayers;
        private readonly SerializedProperty _shapeEscapeLayers;
    }
}