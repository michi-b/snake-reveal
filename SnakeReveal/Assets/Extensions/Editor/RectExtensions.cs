using UnityEditor;
using UnityEngine;

namespace Extensions.Editor
{
    public static class RectExtensions
    {
        public static Rect TakeSingleLineFromTop(this ref Rect rect) => rect.TakeFromTop(EditorGUIUtility.singleLineHeight);

        public static Rect TakeSingleLineFromBottom(this ref Rect rect) => rect.TakeFromBottom(EditorGUIUtility.singleLineHeight);

        public static Rect TakeIndentFromLeft(this ref Rect rect) => rect.TakeFromLeft(15f);
    }
}