using UnityEngine;

namespace Extensions
{
    public static class RectExtensions
    {
        public static Rect TakeFromLeft(this ref Rect rect, float width)
        {
            var result = new Rect(rect.x, rect.y, width, rect.height);
            rect.x += width;
            rect.width -= width;
            return result;
        }

        public static Rect TakeFromRight(this ref Rect rect, float width)
        {
            var result = new Rect(rect.xMax - width, rect.y, width, rect.height);
            rect.width -= width;
            return result;
        }
    }
}