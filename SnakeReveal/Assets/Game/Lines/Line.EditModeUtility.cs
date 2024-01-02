using UnityEditor;

namespace Game.Lines
{
    public partial class Line
    {
        public static class EditModeUtility
        {
            private static void RegisterUndo(Line line, string operationName)
            {
                if (line != null)
                {
                    Undo.RegisterFullObjectHierarchyUndo(line, operationName);
                }
            }

            public static void RecordUndoWithNeighbors(Line line, string operationName)
            {
                RegisterUndo(line._previous, operationName);
                RegisterUndo(line, operationName);
                RegisterUndo(line._next, operationName);
            }
        }
    }
}