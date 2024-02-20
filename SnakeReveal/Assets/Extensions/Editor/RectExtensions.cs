using UnityEditor;
using UnityEngine;

namespace Extensions.Editor
{
    public static class RectExtensions
    {
        public static Rect TakeSingleLineFromTop(this ref Rect rect)
        {
            return rect.TakeFromTop(EditorGUIUtility.singleLineHeight);
        }
        
        public static Rect TakeSingleLineFromBottom(this ref Rect rect)
        {
            return rect.TakeFromBottom(EditorGUIUtility.singleLineHeight);
        }
        
        public static Rect TakeIndentFromLeft(this ref Rect rect)
        {
            return rect.TakeFromLeft(15f);
        }
    }
}