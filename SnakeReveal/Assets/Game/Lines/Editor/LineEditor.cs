using Editor;
using Game.Simulation.Grid;
using UnityEditor;
using UnityEngine;

namespace Game.Lines.Editor
{
    [CanEditMultipleObjects, CustomEditor(typeof(Line))]
    public class LineEditor : UnityEditor.Editor
    {
        protected virtual void OnEnable()
        {
            Tools.hidden = true;
        }

        protected virtual void OnDisable()
        {
            Tools.hidden = false;
        }

        protected void OnSceneGUI()
        {
            DrawHandles((Line)target);
        }

        private void DrawHandles(Line line)
        {
            SimulationGrid grid = line.Grid;
            if (grid == null)
            {
                return;
            }

            float z = line.transform.position.z;

            if (HandlesUtility.TryGridHandleMove(line.Start, z, grid, out Vector2Int newStart))
            {
                line.RegisterUndoWithNeighbors("Move Line Start Grid Handle");
                line.Start = newStart;
            }

            if (HandlesUtility.TryGridHandleMove(line.End, z, grid, out Vector2Int newEnd))
            {
                line.RegisterUndoWithNeighbors("Move Line End Grid Handle");
                line.End = newEnd;
            }

            Vector2Int centerPosition = (line.Start + line.End) / 2;
            if (HandlesUtility.TryGridHandleMove(centerPosition, z, grid, out Vector2Int newCenter))
            {
                line.RegisterUndoWithNeighbors("Move Line Center Grid Handle");
                Vector2Int delta = newCenter - centerPosition;
                line.Start += delta;
                line.End += delta;
            }
        }
    }
}