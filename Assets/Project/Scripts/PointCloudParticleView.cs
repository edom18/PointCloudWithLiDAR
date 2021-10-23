using UnityEngine;

public class PointCloudParticleView : MonoBehaviour
{
    [SerializeField] private PointCloudSource _pointCloudSource;
    [SerializeField] private PointCloudParticle _pointCloudParticle;

    #region ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void Awake()
    {
        UpdateParticle();
    }

    #endregion ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void UpdateParticle()
    {
        if (!_pointCloudSource.IsReady) return;
        
        Metadata metadata = _pointCloudSource.Metadata;
        _pointCloudParticle.ColorMap = _pointCloudSource.ColorTexture;
        _pointCloudParticle.DepthMap = _pointCloudSource.DepthTexture;
        _pointCloudParticle.IntrinsicsVector = metadata.intrinsic;
        // Vector4 projectionVector = ProjectionUtil.GetVector(metadata.projectionMatrix);
        // _pointCloudParticle.ProjectionVector = projectionVector;

        _pointCloudParticle.UpdateParticles();
    }
}