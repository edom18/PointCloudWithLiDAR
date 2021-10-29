using UnityEngine;
using UnityEngine.UI;

public class DemuxDebugger : MonoBehaviour
{
    [SerializeField] private PointCloudParticleView _pointCloudParticleView;
    [SerializeField] private RawImage _preview;
    
    private void Update()
    {
        _preview.texture = _pointCloudParticleView.DemuxTexture;
    }
}
