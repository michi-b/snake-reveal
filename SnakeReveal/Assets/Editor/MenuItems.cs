using Extensions;
using Game.Lines;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class Menu
    {
        private const string MenuRoot = "SnakeReveal/";
        private const string LineGameObjectDisplayRoot = MenuRoot + "Line Display/";

        [MenuItem(LineGameObjectDisplayRoot + "Show All", false, 0)]
        public static void ShowAllLines()
        {
            SetAllLinesVisibleInSceneView(true);
        }

        [MenuItem(LineGameObjectDisplayRoot + "Hide All", false, 1)]
        public static void HideAllLines()
        {
            SetAllLinesVisibleInSceneView(false);
        }

        private static void SetAllLinesVisibleInSceneView(bool visible)
        {
            string operationName = $"{nameof(SetAllLinesVisibleInSceneView)}: {visible}";
            foreach (Line line in Object.FindObjectsByType<Line>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                GameObject gameObject = line.gameObject;
                Undo.RegisterFullObjectHierarchyUndo(gameObject, operationName);
                gameObject.SetVisibleInSceneView(visible);
            }
        }
    }
}