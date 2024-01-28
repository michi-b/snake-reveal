using UnityEditor;
using UnityEngine;

namespace Game
{
    [FilePath("ProjectSettings/GameSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class GameSettings : ScriptableSingleton<GameSettings>
    {
        [SerializeField] private int _isCapturedInDrawnShapeCheckLayer;

        public int IsCapturedInDrawnShapeCheckLayer
        {
            get => _isCapturedInDrawnShapeCheckLayer;
            set
            {
                if (value != _isCapturedInDrawnShapeCheckLayer)
                {
                    _isCapturedInDrawnShapeCheckLayer = value;
                    Save();
                }
            }
        }

        private void Save()
        {
            Save(true);
        }
    }
}