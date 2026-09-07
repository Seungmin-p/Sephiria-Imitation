using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Post-processing/Glitch")]
public class GlitchVolumeComponent : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter splitAmount = new ClampedFloatParameter(0f, 0f, 1f);

    public bool IsActive() => splitAmount.value > 0f;

    public bool IsTileCompatible() => false;
}