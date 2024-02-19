using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    [FilePath("ProjectSettings/GameSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class GameSettings : ScriptableSingleton<GameSettings>
    {
        [SerializeField] private LayerMask _captureEnterLayers;

        [SerializeField] private LayerMask _captureMaintainLayers;

        public const string ShapeCaptureLayersPropertyName = nameof(_captureEnterLayers);
        public const string ShapeEscapeLayersPropertyName = nameof(_captureMaintainLayers);

        public LayerMask IsCapturedInDrawnCheckEnterLayers => _captureEnterLayers;

        public LayerMask CaptureMaintainLayers => _captureMaintainLayers;

        public void Save()
        {
            Save(true);
        }
    }
}