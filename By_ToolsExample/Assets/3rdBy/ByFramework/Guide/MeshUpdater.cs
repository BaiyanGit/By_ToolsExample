namespace _3rdBy.MetaFramework.Guide
{
    using UnityEngine;

    /// <summary>
    /// 网格变形更新meshCollider
    /// </summary>
    public class MeshUpdater : MonoBehaviour
    {
        public bool updateOnce = true;

        private MeshCollider _meshCollider;
        private SkinnedMeshRenderer _skinnedMeshRenderer;

        private void Awake()
        {
            _meshCollider = GetComponent<MeshCollider>();
            _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        }

        private void Start()
        {
            UpdateMesh();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!updateOnce)
            {
                UpdateMesh();
            }
        }

        /// <summary>
        /// 更新mesh
        /// </summary>
        private void UpdateMesh()
        {
            Mesh colliderMesh = new Mesh();
            _skinnedMeshRenderer.BakeMesh(colliderMesh);

            _meshCollider.sharedMesh = null;
            _meshCollider.sharedMesh = colliderMesh;
        }
    }
}