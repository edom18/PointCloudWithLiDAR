using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class PointCloudParticle : MonoBehaviour
{
    private struct Particle
    {
        public float scale;
        public Vector3 position;
        public Vector4 color;
    }

    [SerializeField] private int _width = 256;
    [SerializeField] private int _height = 256;
    [SerializeField] private float _scale = 0.05f;
    [SerializeField] private ComputeShader _computeShader = null;
    [SerializeField] private Material _particleMat;
    [SerializeField] private Mesh _particleMesh;

    public Texture ColorMap { get; set; }
    public Texture DepthMap { get; set; }
    public Vector4 IntrinsicsVector { get; set; } = new Vector4(1, 1, 0, 0);
    public int[] DepthResolution { get; set; }
    public Vector2Int CameraResolution { get; set; }
    
    private int _kernelId = 0;
    private ComputeBuffer _particleBuffer = null;
    private ComputeBuffer _argBuffer = null;
    private uint[] _args = new uint[] {0, 0, 0, 0, 0};

    private bool CanUpdate => (ColorMap != null && DepthMap != null);
    private bool _hasInitialized = false;

    #region ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void Update()
    {
        if (CanUpdate)
        {
            DrawParticles();
        }
    }

    private void OnDestroy()
    {
        _particleBuffer?.Release();
        _argBuffer?.Release();
    }

    #endregion ### ------------------------------ MonoBehaviour ------------------------------ ###

    public void Initialize(int[] cameraResolution)
    {
        if (_hasInitialized) return;

        _hasInitialized = true;

        _width = cameraResolution[0];
        _height = cameraResolution[1];

        Debug.Log($"Created with size {_width} x {_height}");
        
        // Recreate a material because it's shared to other particle views.
        _particleMat = Instantiate(_particleMat);

        // Recreate a shader because it's shared to other particle views.
        _computeShader = Instantiate(_computeShader);

        _kernelId = _computeShader.FindKernel("Update");

        Particle[] particles = new Particle[_width * _height];

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i] = new Particle
            {
                scale = 0,
                position = Random.insideUnitSphere,
                color = Vector4.one,
            };
        }

        _particleBuffer = new ComputeBuffer(particles.Length, Marshal.SizeOf<Particle>());
        _particleBuffer.SetData(particles);

        _computeShader.SetBuffer(_kernelId, "_ParticleBuffer", _particleBuffer);
        _particleMat.SetBuffer("_ParticleBuffer", _particleBuffer);

        _args[0] = _particleMesh.GetIndexCount(0);
        _args[1] = (uint)particles.Length;
        _args[2] = _particleMesh.GetIndexStart(0);
        _args[3] = _particleMesh.GetBaseVertex(0);

        _argBuffer = new ComputeBuffer(1, sizeof(uint) * _args.Length, ComputeBufferType.IndirectArguments);
        _argBuffer.SetData(_args);
    }

    public void UpdateParticles()
    {
        _computeShader.SetInt("_Width", _width);
        _computeShader.SetInt("_Height", _height);
        _computeShader.SetTexture(_kernelId, "_ColorMap", ColorMap);
        _computeShader.SetTexture(_kernelId, "_DepthMap", DepthMap);
        _computeShader.SetInts("_DepthResolution", DepthResolution);
        _computeShader.SetVector("_IntrinsicsVector", IntrinsicsVector);
        Vector4 gridScale = new Vector4(
            (float)CameraResolution.x / (float)DepthResolution[0],
            (float)CameraResolution.y / (float)DepthResolution[1],
            0, 0);
        _computeShader.SetVector("_GridPointsScale", gridScale);
        _computeShader.SetMatrix("_TransformMatrix", transform.localToWorldMatrix);
        _computeShader.Dispatch(_kernelId, _width / 8, _height / 8, 1);
    }

    private void DrawParticles()
    {
        Graphics.DrawMeshInstancedIndirect(_particleMesh, 0, _particleMat, new Bounds(transform.position, new Vector3(1000f, 300f, 1000f)), _argBuffer);
    }
}