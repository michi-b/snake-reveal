using Extensions;
using Game.Simulation.Grid;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class HandlesUtility
    {
        public static bool TryGridHandleMove(Vector2Int position, float z, SimulationGrid grid, out Vector2Int newPosition)
        {
            EditorGUI.BeginChangeCheck();
            var oldWorldPosition = grid.GetScenePosition(position).ToVector3(z);
            Vector3 newWorldPosition = Handles.PositionHandle(oldWorldPosition, Quaternion.identity);
            Vector2Int newGridPosition = grid.Round(newWorldPosition);
            if (EditorGUI.EndChangeCheck() && newGridPosition != position)
            {
                newPosition = newGridPosition;
                return true;
            }

            newPosition = position;
            return false;
        }

        public static void DrawWireDisc(Vector3 position, float sizeMultiplier)
        {
            float size = HandleUtility.GetHandleSize(position) * sizeMultiplier;
            Handles.DrawWireDisc(position, Vector3.back, size);
        }
    }
}