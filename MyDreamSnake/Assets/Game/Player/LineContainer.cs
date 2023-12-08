using Game.Lines;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Player
{
    public class LineContainer : MonoBehaviour
    {
        [SerializeField] private Grid _grid;

        public void Place(Line line, int2 start, int2 end)
        {
            var thisTransform = transform;
            line.transform.parent = thisTransform;
            line.Place(_grid, start, end, thisTransform.position.z);
        }
    }
}