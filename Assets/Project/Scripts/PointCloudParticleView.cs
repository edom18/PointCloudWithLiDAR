using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PointCloudParticleView : MonoBehaviour
{
    [SerializeField] private PointCloudSource _pointCloudSource;
    [SerializeField] private PointCloudParticle _pointCloudParticle;
    [SerializeField] private Slider _slider;

    private bool _firstTake = true;

    [SerializeField] private Transform _container;
    [SerializeField] private Button _posResetButton;
    [SerializeField] private Button _rotResetButton;
    [SerializeField] private bool _isRotate = true;
    private bool _isDragging = false;
    private Vector3 _prevPos = Vector2.zero;
    private Vector3 _initPos = Vector3.zero;

    private Vector3 _offsetPos = Vector3.zero;

    #region ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void Awake()
    {
        _initPos = transform.localPosition;

        UpdateParticle();

        _slider.onValueChanged.AddListener((val) =>
        {
            _offsetPos.z = val;
            UpdatePosition();
        });

        _rotResetButton.onClick.AddListener(() => { transform.localRotation = quaternion.identity; });

        _posResetButton.onClick.AddListener(() =>
        {
            _offsetPos = Vector3.zero;
            _slider.value = 0;
            UpdatePosition();
        });
    }

    private void Update()
    {
        UpdateParticle();

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            BeginTouch(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Dragging(Input.mousePosition, _isRotate);
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndTouch();
        }
#else
        if (Input.touchCount == 0) return;

        bool isRotate = Input.touchCount == 1;
        
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            BeginTouch(touch.position);
        }

        if (touch.phase == TouchPhase.Moved)
        {
            Dragging(touch.position, isRotate);
        }

        if (touch.phase == TouchPhase.Ended)
        {
            EndTouch();
        }
#endif
    }

    #endregion ### ------------------------------ MonoBehaviour ------------------------------ ###

    private void UpdatePosition()
    {
        transform.localPosition = _initPos + _offsetPos;
    }

    private void BeginTouch(Vector3 startPos)
    {
        if (EventSystem.current.currentSelectedGameObject != null) return;

        _isDragging = true;
        _prevPos = startPos;
    }

    private void Dragging(Vector3 pos, bool isRotate)
    {
        if (!_isDragging) return;

        Vector3 delta = pos - _prevPos;
        _prevPos = pos;

        if (isRotate)
        {
            transform.Rotate(_container.up, delta.x, Space.World);
            Vector3 right = Vector3.Cross(_container.forward, Vector3.up);
            transform.Rotate(right, delta.y, Space.World);
        }
        else
        {
            _offsetPos += delta * 0.01f;
            UpdatePosition();
        }
    }

    private void EndTouch()
    {
        _isDragging = false;
    }

    private void UpdateParticle()
    {
        if (!_pointCloudSource.IsReady) return;

        Metadata metadata = _pointCloudSource.Metadata;

        if (_firstTake)
        {
            _firstTake = false;
            _pointCloudParticle.Initialize(metadata.depthResolution);
        }

        _pointCloudParticle.ColorMap = _pointCloudSource.ColorTexture;
        _pointCloudParticle.DepthMap = _pointCloudSource.DepthTexture;
        _pointCloudParticle.ConfidenceMap = _pointCloudSource.ConfidenceTexture;
        _pointCloudParticle.Intrinsics = metadata.intrinsic;
        _pointCloudParticle.DepthResolution = metadata.depthResolution;
        _pointCloudParticle.CameraResolution = metadata.cameraResolution;
        _pointCloudParticle.GridPointsScale = new Vector4(
            (float)metadata.cameraResolution.x / (float)metadata.depthResolution.x,
            (float)metadata.cameraResolution.y / (float)metadata.depthResolution.y,
            0, 0);

        _pointCloudParticle.UpdateParticles();
    }
}