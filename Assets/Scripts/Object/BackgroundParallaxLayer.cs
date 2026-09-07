using UnityEngine;

//스크립트 실행 순서 지정, 이로 인해 LateUpdate 타이밍이 의도와 맞게 동작
[DefaultExecutionOrder(1000)]
public class BackgroundParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform mainCamera; //메인 카메라
    [SerializeField] private Transform referencePoint; //기준 위치
    [SerializeField] private Vector2 followRatio = Vector2.one; //이동 비율

    private Vector3 parallaxTransformPosition;

    private void Awake()
    {
        //초기 배경 위치값 저장
        parallaxTransformPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (mainCamera == null || referencePoint == null)
            return;

        //메인 카메라의 위치에서 기준 위치를 빼서 실제로 배경을 이동시킬 거리를 구함
        Vector3 cameraMovement = mainCamera.position - referencePoint.position;

        //이동 거리에 이동 비율을 곱해서 위치 적용
        transform.position = new Vector3(
            parallaxTransformPosition.x + cameraMovement.x * followRatio.x,
            parallaxTransformPosition.y + cameraMovement.y * followRatio.y,
            parallaxTransformPosition.z
        );
    }
}