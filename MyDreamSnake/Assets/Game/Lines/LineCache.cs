using System.Globalization;
using UnityEngine;

namespace Game.Lines
{
    //todo: implement actual caching (this is currently just instantiate and destroy)
    public class LineCache : MonoBehaviour
    {
        [SerializeField] private Line _linePrefab;

        private int _currentLineIndex = -1;

        public Line Get()
        {
            var newLine = Instantiate(_linePrefab);
            newLine.gameObject.name = "Line" + (++_currentLineIndex).ToString(CultureInfo.InvariantCulture);
            return newLine;
        }

        public void Return(Line line)
        {
            Destroy(line.gameObject);
        }
    }
}