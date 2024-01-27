using UnityEditor;
using UnityEngine;

namespace Game
{
    [FilePath("ProjectSettings/GameSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class GameSettings : ScriptableSingleton<GameSettings>
    {
        [SerializeField] private LayerMask _drawnShapeLayer;

        public LayerMask DrawnShapeLayer
        {
            get => _drawnShapeLayer;
            set
            {
                if (value != _drawnShapeLayer)
                {
                    _drawnShapeLayer = value;
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