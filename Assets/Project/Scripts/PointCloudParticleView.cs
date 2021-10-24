using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PointCloudParticleView : MonoBehaviour
{
    [SerializeField] private PointCloudSource _pointCloudSource;
    [SerializeField] private PointCloudParticle _pointCloudParticle;
    [SerializeField] private Slider _slider;

    private bool _firstTake = true;
    
    private bool _isDragging = false;
    private Vector3 _prevPos = Vector2.zero;
    private Vector3 _initPos = Vector3.zero;

    #region ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void Awake()
    {
        _initPos = transform.localPosition;
        
        UpdateParticle();
        
        _slider.onValueChanged.AddListener((val) =>
        {
            transform.localPosition = _initPos + new Vector3(0, 0, val);
        });
    }

    private void Update()
    {
        UpdateParticle();

        if (Input.GetMouseButtonDown(0))
        {
            BeginTouch(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Dragging(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndTouch();
        }
    }

    #endregion ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void BeginTouch(Vector3 startPos)
    {
        if (EventSystem.current.currentSelectedGameObject != null) return;
        
        _isDragging = true;
        _prevPos = startPos;
    }

    private void Dragging(Vector3 pos)
    {
        if (!_isDragging) return;

        Vector3 delta = pos - _prevPos;

        transform.Rotate(Vector3.up, delta.x);
        transform.Rotate(Vector3.right, -delta.y);

        _prevPos = pos;
    }
    
    private void EndTouch()
    {
        _isDragging = false;
    }

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