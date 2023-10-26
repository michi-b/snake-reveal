using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = Names.GameName + "/Game Arguments")]
    public class GameArguments : ScriptableObject
    {
        [SerializeField] private bool _playOnEnable;

        public bool PlayOnEnable => _playOnEnable;
    }
}