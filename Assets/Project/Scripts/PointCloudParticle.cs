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
    [SerializeField] private float _depthOffset = 0.03f;
    [SerializeField] private float _maxFar = 3.88f;

    public Texture ColorMap { get; set; }
    public Texture DepthMap { get; set; }
    public Vector4 ProjectionVector { get; set; } = new Vector4(0, 0, 1, 1);
    public Vector4 IntrinsicsVector { get; set; } = new Vector4(1, 1, 0, 0);

    private int _kernelId = 0;
    private ComputeBuffer _particleBuffer = null;
    private ComputeBuffer _argBuffer = null;
    private uint[] _args = new uint[] {0, 0, 0, 0, 0};

    public bool CanUpdate => (ColorMap != null && DepthMap != null);
    public bool NeedsDraw { get; set; } = true;

    #region ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        if (CanUpdate && NeedsDraw)
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

    private void Initialize()
    {
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
                scale = _scale,
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
        _computeShader.SetFloat("_Scale", _scale);
        _computeShader.SetFloat("_MaxFar", _maxFar);
        _computeShader.SetFloat("_DepthOffset", _depthOffset);
        _computeShader.SetTexture(_kernelId, "_ColorMap", ColorMap);
        _computeShader.SetTexture(_kernelId, "_DepthMap", DepthMap);
        _computeShader.SetVector("_ProjectionVector", ProjectionVector);
        _computeShader.SetVector("_IntrinsicsVector", IntrinsicsVector);
        _computeShader.SetMatrix("_TransformMatrix", transform.localToWorldMatrix);
        _computeShader.Dispatch(_kernelId, _width / 8, _height / 8, 1);
    }

    private void DrawParticles()
    {
        Graphics.DrawMeshInstancedIndirect(_particleMesh, 0, _particleMat, new Bounds(transform.position, Vector3.one * 32f), _argBuffer);
    }
}