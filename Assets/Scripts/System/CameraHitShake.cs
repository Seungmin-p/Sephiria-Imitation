using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraHitShake : CinemachineExtension
{
    [Header("피격 흔들림")]
    [SerializeField] private float hitShakeDuration = 0.2f;
    [SerializeField] private float hitShakeDistance = 0.25f;

    private Vector3 shakeOffset;
    private Coroutine shakeCoroutine;

    //카메라 흔들림(피격 연출)
    public void PlayHitShake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(HitShake());
    }

    //흔들림 처리
    private IEnumerator HitShake()
    {
        float elapsedTime = 0f;

        while (elapsedTime < hitShakeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsedTime / hitShakeDuration);

            float damping = (1f - progress) * (1f - progress);
            float shake = Mathf.Sin(progress * Mathf.PI * 6f) * hitShakeDistance * damping;

            shakeOffset = new Vector3(shake, 0f, 0f);

            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeCoroutine = null;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Finalize)
            state.PositionCorrection += shakeOffset;
    }
}