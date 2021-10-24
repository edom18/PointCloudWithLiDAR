using System;
using UnityEngine;

public class PointCloudParticleView : MonoBehaviour
{
    [SerializeField] private PointCloudSource _pointCloudSource;
    [SerializeField] private PointCloudParticle _pointCloudParticle;

    private bool _firstTake = true;

    #region ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void Awake()
    {
        UpdateParticle();
    }

    private void Update()
    {
        UpdateParticle();
    }

    #endregion ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void UpdateParticle()
    {
        if (!_pointCloudSource.IsReady) return;

        if (_firstTake)
        {
            _firstTake = false;
            _pointCloudParticle.Initialize(_pointCloudSource.CameraResolution);
        }
        
        Metadata metadata = _pointCloudSource.Metadata;
        _pointCloudParticle.ColorMap = _pointCloudSource.ColorTexture;
        _pointCloudParticle.DepthMap = _pointCloudSource.DepthTexture;
        _pointCloudParticle.IntrinsicsVector = metadata.intrinsic;
        _pointCloudParticle.DepthResolution = _pointCloudSource.CameraResolution;
        _pointCloudParticle.CameraResolution = metadata.cameraResolution;

        _pointCloudParticle.UpdateParticles();
    }
}