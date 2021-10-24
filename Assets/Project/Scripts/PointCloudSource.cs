using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PointCloudSource : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private ARCameraManager _cameraManager;
    [SerializeField] private AROcclusionManager _occlusionManager;

    [Space] [SerializeField] private float _minDepth = 0.2f;
    [SerializeField] private float _maxDepth = 3.2f;
    [SerializeField] private Shader _makeRGBShader;

    private Metadata _metadata;
    private Material _makeRGBMaterial;
    private Matrix4x4 _projectionMatrix;

    public RenderTexture ColorTexture { get; private set; }
    public Texture DepthTexture { get; private set; }
    public Metadata Metadata => _metadata;
    public bool IsReady => (ColorTexture != null) && (DepthTexture != null);
    public int[] CameraResolution { get; } = new int[2];

    private int _width = 1024;
    private int _height = 1024;

    private bool _firstTake = true;


    private void UpdateMetadata()
    {
        _cameraManager.TryGetIntrinsics(out XRCameraIntrinsics intrinsics);

        _metadata.position = _camera.transform.position;
        _metadata.rotation = _camera.transform.rotation;
        _metadata.projectionMatrix = _projectionMatrix;
        _metadata.depthRange = new Vector2(_minDepth, _maxDepth);

        _metadata.intrinsic = new Vector4(
            intrinsics.focalLength.x,
            intrinsics.focalLength.y,
            intrinsics.principalPoint.x,
            intrinsics.principalPoint.y
        );

        _metadata.cameraResolution = intrinsics.resolution;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (args.textures.Count == 0) return;

        for (int i = 0; i < args.textures.Count; i++)
        {
            int id = args.propertyNameIds[i];
            Texture2D tex = args.textures[i];

            if (id == ShaderID.TextureY)
            {
                if (_firstTake)
                {
                    _firstTake = false;

                    _width = tex.width;
                    _height = tex.height;
                    CreateRenderTextures();
                }

                _makeRGBMaterial.SetTexture(ShaderID.TextureY, tex);
                
                if (IsReady) UpdateMetadata();
            }
            else if (id == ShaderID.TextureCbCr)
            {
                _makeRGBMaterial.SetTexture(ShaderID.TextureCbCr, tex);
                
                if (IsReady) UpdateMetadata();
            }
        }
    }

    /// <summary>
    /// This method is registered to ARFoundation a callback.
    /// This receives an ARFoundation occlusion data per update.
    /// </summary>
    /// <param name="args">An argument that stores an occlusion info.</param>
    private void OnOcclusionFrameReceived(AROcclusionFrameEventArgs args)
    {
        for (int i = 0; i < args.textures.Count; i++)
        {
            int id = args.propertyNameIds[i];
            Texture2D tex = args.textures[i];

            if (id == ShaderID.EnvironmentDepth)
            {
                DepthTexture = tex;

                CameraResolution[0] = tex.width;
                CameraResolution[1] = tex.height;
                
                if (IsReady) UpdateMetadata();
            }
        }
    }

    #region ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void Awake()
    {
        CreateMaterials();
    }

    private void Start()
    {
        _cameraManager.frameReceived += OnCameraFrameReceived;
        _occlusionManager.frameReceived += OnOcclusionFrameReceived;

        Debug.Log($"Started a camera capture. {_cameraManager.name}");
    }

    private void Update()
    {
        Vector2 range = new Vector2(_minDepth, _maxDepth);
        _makeRGBMaterial.SetVector(ShaderID.DepthRange, range);

        // Update the render texture.
        Graphics.Blit(null, ColorTexture, _makeRGBMaterial, 0);
    }

    private void OnDestroy()
    {
        Destroy(_makeRGBMaterial);
        ReleaseRenderTextures();
    }

    #endregion ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void CreateMaterials()
    {
        _makeRGBMaterial = new Material(_makeRGBShader);
    }

    private void CreateRenderTextures()
    {
        ReleaseRenderTextures();

        ColorTexture = new RenderTexture(_width, _height, 0);
        ColorTexture.Create();
    }

    private void ReleaseRenderTextures()
    {
        if (ColorTexture != null) ColorTexture.Release();
        if (DepthTexture != null) Destroy(DepthTexture);
    }
}