using UnityEditor;

namespace Editor
{
    public static class SerializedObjectExtensions
    {
        public static SerializedProperty FindDirectChild(this SerializedObject target, string path)
        {
            SerializedProperty iterator = target.GetIterator();
            iterator.Next(true);
            do
            {
                if (iterator.name == path)
                {
                    return iterator;
                }
            } while (iterator.Next(false));

            return null;
        }
    }
}