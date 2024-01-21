using System;
using System.Collections.Generic;
using System.Linq;
using Game.Grid;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Game.Quads
{
    public class QuadContainer : MonoBehaviour
    {
        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private QuadCache _cache;
        [SerializeField] private List<Quad> _quads;

        public void AddRange(List<QuadData> quads)
        {
            foreach (QuadData quadData in quads)
            {
                Add(quadData);
            }
        }

        private void Add(QuadData quadData)
        {
            Quad quad = GetNewQuad(quadData);
            _quads.Add(quad);
        }

        private Quad GetNewQuad(QuadData quadData)
        {
            Quad quad = _cache.Get();
            quad.transform.parent = transform;
            quad.Place(quadData);
            return quad;
        }

        public void EditModeSetQuads(List<QuadData> newQuads)
        {
            Undo.RegisterFullObjectHierarchyUndo(this, "Edit Mode Set Quads");
            foreach (Quad quad in _quads)
            {
                Undo.DestroyObjectImmediate(quad.gameObject);
            }
            _quads.Clear();

            foreach (QuadData quadData in newQuads)
            {
                var quad = PrefabUtility.InstantiatePrefab(_cache.Prefab).GetComponent<Quad>();
                quad.transform.parent = transform;
                Undo.RegisterCreatedObjectUndo(quad.gameObject, "Create Quad");
                Undo.RegisterFullObjectHierarchyUndo(quad.gameObject, "Initialize Quad");
                quad.Initialize(_grid);
                quad.Place(quadData);
                _quads.Add(quad);
            }
        }

        protected virtual void Reset()
        {
                _grid = SimulationGrid.EditModeFind();
                _cache = FindObjectsByType<QuadCache>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        }
    }
}