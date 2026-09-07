using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Post-processing/Rectangle Vignette")]
public class RectangleVignetteVolumeComponent : VolumeComponent, IPostProcessComponent
{
    public ColorParameter color = new ColorParameter(Color.black);
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public ClampedFloatParameter smoothness = new ClampedFloatParameter(0.2f, 0.01f, 1f);

    public bool IsActive() => intensity.value > 0f;

    public bool IsTileCompatible() => false;
}