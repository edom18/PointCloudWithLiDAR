using UnityEngine;

public enum TextureSize
{
    T64 = 1 << 6,
    T128 = 1 << 7,
    T256 = 1 << 8,
    T512 = 1 << 9,
    T1024 = 1 << 10,
    T2048 = 1 << 11,
}

static class ShaderID
{
    public static readonly int ColorTexture = Shader.PropertyToID("_ColorTexture");
    public static readonly int DepthTexture = Shader.PropertyToID("_DepthTexture");
    public static readonly int DepthOffset = Shader.PropertyToID("_DepthOffset");
    public static readonly int InverseViewMatrix = Shader.PropertyToID("_InverseViewMatrix");
    public static readonly int ProjectionMatrix = Shader.PropertyToID("_ProjectionMatrix");
    public static readonly int ProjectionVector = Shader.PropertyToID("_ProjectionVector");
    public static readonly int TextureY = Shader.PropertyToID("_textureY");
    public static readonly int TextureCbCr = Shader.PropertyToID("_textureCbCr");
    public static readonly int HumanStencil = Shader.PropertyToID("_HumanStencil");
    public static readonly int EnvironmentDepth = Shader.PropertyToID("_EnvironmentDepth");
    public static readonly int DepthRange = Shader.PropertyToID("_DepthRange");
    public static readonly int AspectFix = Shader.PropertyToID("_AspectFix");
}

public struct Metadata
{
    public Vector3 position;
    public Quaternion rotation;
    public Matrix4x4 projectionMatrix;
    public Vector2 depthRange;
    public Vector4 intrinsic;
    public Vector2Int cameraResolution;
}

static class ProjectionUtil
{
    public static Vector4 GetVector(in Matrix4x4 m) => new Vector4(m[0, 2], m[1, 2], m[0, 0], m[1, 1]);
}