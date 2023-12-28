using UnityEditor;

namespace Editor
{
    public static class SerializedPropertyExtensions
    {
        public static SerializedProperty FindDirectChild(this SerializedProperty target, string path)
        {
            SerializedProperty iterator = target.Copy();
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