using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Extensions
{
    public static class RectExtensions
    {

        
        public static Rect TakeFromTop(this ref Rect rect, float height)
        {
            var result = new Rect(rect.x, rect.y, rect.width, height);
            rect.y += height;
            rect.height -= height;
            return result;
        }


        public static Rect TakeFromBottom(this ref Rect rect, float height)
        {
            var result = new Rect(rect.x, rect.yMax - height, rect.width, height);
            rect.height -= height;
            return result;
        }

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