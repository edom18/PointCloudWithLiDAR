using UnityEngine;
using UnityEngine.UI;

public class TextureDebugger : MonoBehaviour
{
    [SerializeField] private PointCloudSource _pointCloudSource;
    [SerializeField] private RawImage _colorPreview;
    [SerializeField] private RawImage _depthPreview;
    
    private void Update()
    {
        if (!_pointCloudSource.IsReady) return;
        
        _colorPreview.texture = _pointCloudSource.ColorTexture;
        _depthPreview.texture = _pointCloudSource.DepthTexture;
    }
}
