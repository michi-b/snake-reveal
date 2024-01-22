using System.Diagnostics;
using Extensions;
using Game.Grid;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
#endif

namespace Game.Quads
{
    [DebuggerDisplay("{ToString(),nq}")]
    public class Quad : MonoBehaviour
    {
        private const int BottomLeftVertexIndex = 0;
        private const int TopLeftVertexIndex = 1;
        private const int TopRightVertexIndex = 2;
        private const int BottomRightVertexIndex = 3;

        [SerializeField] private SimulationGrid _grid;
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private QuadData _data;

        [ContextMenuItem("Personalize", nameof(PersonalizeMesh))]
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Vector3[] _vertices;

        public SimulationGrid Grid => _grid;

        public Vector2Int TopRight
        {
            set
            {
                if (value != TopRight)
                {
                    _data.TopRight = value;
                    Apply();
                }
            }
            get => _data.TopRight;
        }

        public Vector2Int TopLeft => new(BottomLeft.x, TopRight.y);

        public Vector2Int BottomLeft
        {
            set
            {
                if (value != BottomLeft)
                {
                    _data.BottomLeft = value;
                    Apply();
                }
            }
            get => _data.BottomLeft;
        }

        public Vector2Int BottomRight => new(TopRight.x, BottomLeft.y);

        public Vector2Int Size
        {
            set
            {
                if (value != Size)
                {
                    _data.Size = value;
                    Apply();
                }
            }
            get => _data.Size;
        }

#if UNITY_EDITOR
        public void PersonalizeMesh()
        {
            Undo.RecordObject(this, "Personalize Mesh");
            _mesh = Instantiate(_mesh);
            _mesh.name = gameObject.name;
            _meshFilter.sharedMesh = _mesh;
        }
#endif

        protected virtual void Reset()
        {
            _grid = SimulationGrid.EditModeFind()!;
            _collider = gameObject.GetComponent<BoxCollider2D>();
            _data = new QuadData(_grid.Round(transform.position));
            Apply();
        }

        [ContextMenu("Initialize", false)]
        public virtual void Initialize(SimulationGrid grid)
        {
            _grid = grid;
            _vertices = new Vector3[4];
            _mesh = new Mesh(){vertices = _vertices, indexFormat = IndexFormat.UInt16};
            _mesh.vertices = _vertices;
            int[] triangles = new int[6];
            triangles[0] = BottomLeftVertexIndex;
            triangles[1] = TopLeftVertexIndex;
            triangles[2] = TopRightVertexIndex;
            triangles[3] = TopRightVertexIndex;
            triangles[4] = BottomRightVertexIndex;
            triangles[5] = BottomLeftVertexIndex;
            // todo: look into lower level mesh api SetIndexBufferParams & SetVertexBufferData for maximum performance
            _mesh.triangles = triangles;
            _meshFilter.sharedMesh = _mesh;
            _mesh.RecalculateBounds(MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds);
        }

        public void Place(QuadData quadData)
        {
            _data = quadData;
            Apply();
        }

        public void Apply()
        {
            transform.position = BottomLeft.GetScenePosition(_grid).ToVector3(transform.position.z);
            Vector2 sceneSize = Size * Grid.SceneCellSize;
            _collider.offset = sceneSize * 0.5f;
            _collider.size = sceneSize;

            _vertices[TopLeftVertexIndex] = new Vector3(0f, sceneSize.y, 0f);
            _vertices[TopRightVertexIndex] = sceneSize;
            _vertices[BottomRightVertexIndex] = new Vector3(sceneSize.x, 0f, 0f);

            // todo: look into lower level mesh api SetVertexBufferParams & SetVertexBufferData for maximum performance
            _mesh.vertices = _vertices;
            _mesh.RecalculateBounds(MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds);
        }

        public void Move(Vector2Int delta)
        {
            _data.Move(delta);
            Apply();
        }

        public void Set(QuadData quadData)
        {
            _data = quadData;
            Apply();
        }

        public override string ToString()
        {
            return _data.ToString();
        }

#if UNITY_EDITOR
        public void RegisterUndo(string operationName)
        {
            Undo.RecordObject(_mesh, operationName + " - Mesh");
            Undo.RegisterFullObjectHierarchyUndo(gameObject, operationName + " - Quad");
        }
#endif
    }
}