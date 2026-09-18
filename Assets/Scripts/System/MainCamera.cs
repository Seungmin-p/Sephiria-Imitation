// 시네머신 적용 전 기존 카메라 로직
//
// using System.Collections;
// using UnityEngine;
//
// [RequireComponent(typeof(Camera))]
// public class MainCamera : MonoBehaviour
// {
//     //카메라 상태
//     private enum CameraState
//     {
//         Follow,
//         MoveToFixed,
//         Fixed,
//         ReturnToFollow
//     }
//
//     [Header("플레이어 추적")]
//     [SerializeField] private Transform target;
//     [SerializeField] private float smoothSpeed = 1f; //카메라가 따라가는 부드러운 정도 (낮을수록 부드러움)
//     [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); //플레이어와의 거리 유지
//
//     [Header("마을용 카메라 제한 범위")]
//     [SerializeField] private BoxCollider2D cameraBounds;
//
//     [Header("피격 흔들림")]
//     [SerializeField] private float hitShakeDuration = 0.2f;
//     [SerializeField] private float hitShakeDistance = 0.25f;
//
//     private Camera cameraComponent;
//
//     private Vector3 followPosition;
//     private Vector3 shakeOffset;
//
//     private CameraState currentState = CameraState.Follow;
//     private CameraFixedArea currentFixedArea;
//
//     private Vector3 fixedPosition;
//     private Vector3 transitionStartPosition;
//
//     private float transitionDuration;
//     private float transitionTimer;
//
//     private Coroutine shakeCoroutine;
//
//     private void Awake()
//     {
//         cameraComponent = GetComponent<Camera>();
//     }
//
//     private void Start()
//     {
//         //플레이어를 따라갈 카메라 좌표 지정
//         if (target != null)
//             followPosition = ClampPosition(target.position + offset);
//     }
//
//     private void LateUpdate()
//     {
//         if (target == null) return;
//
//         //플레이어의 현재 위치에 오프셋을 더한 목표 좌표 계산
//         Vector3 targetPosition = ClampPosition(target.position + offset);
//
//         //카메라 상태에 따른 로직 실행
//         switch (currentState)
//         {
//             //플레이어 추적 진행
//             case CameraState.Follow:
//                 followPosition = Vector3.Lerp(followPosition, targetPosition, smoothSpeed);
//                 followPosition = ClampPosition(followPosition);
//                 break;
//
//             //고정 위치로 이동 진행
//             case CameraState.MoveToFixed:
//                 UpdateTransition(fixedPosition, CameraState.Fixed);
//                 break;
//
//             //위치 고정
//             case CameraState.Fixed:
//                 followPosition = fixedPosition;
//                 break;
//
//             //고정 해제 진행
//             case CameraState.ReturnToFollow:
//                 UpdateTransition(targetPosition, CameraState.Follow);
//                 break;
//         }
//
//         //흔들림 효과가 있는 경우 흔들림처리
//         transform.position = followPosition + shakeOffset;
//     }
//     
//     //카메라 이동 범위 제한
//     private Vector3 ClampPosition(Vector3 position)
//     {
//         //제한이 없으면 입력된 그대로 반환
//         if (cameraBounds == null) return position;
//
//         //범위 확보
//         Bounds bounds = cameraBounds.bounds;
//
//         //현재 카메라의 가로, 세로범위의 절반을 각각 구함
//         float halfHeight = cameraComponent.orthographicSize;
//         float halfWidth = halfHeight * cameraComponent.aspect;
//
//         //카메라가 이동할 수 있는 최대 좌표들을 구함
//         float minX = bounds.min.x + halfWidth;
//         float maxX = bounds.max.x - halfWidth;
//         float minY = bounds.min.y + halfHeight;
//         float maxY = bounds.max.y - halfHeight;
//
//         //Clamp와 최대 좌표들을 이용해서 카메라 이동 범위 제한
//         position.x = minX <= maxX ? Mathf.Clamp(position.x, minX, maxX) : bounds.center.x;
//         position.y = minY <= maxY ? Mathf.Clamp(position.y, minY, maxY) : bounds.center.y;
//
//         return position;
//     }
//
//     //고정영역 진입 시
//     public void EnterFixedArea(CameraFixedArea fixedArea, Vector3 targetPosition, float duration)
//     {
//         currentFixedArea = fixedArea;
//         
//         //고정 위치 설정
//         fixedPosition = new Vector3(targetPosition.x, targetPosition.y, followPosition.z);
//
//         StartTransition(CameraState.MoveToFixed, duration);
//     }
//
//     //고정 위치 이동을 위한 초기화 작업
//     private void StartTransition(CameraState nextState, float duration)
//     {
//         transitionStartPosition = followPosition;
//         transitionDuration = Mathf.Max(0f, duration);
//         transitionTimer = 0f;
//         currentState = nextState;
//     }
//
//     //카메라 이동 작업 진행
//     private void UpdateTransition(Vector3 targetPosition, CameraState nextState)
//     {
//         transitionTimer += Time.deltaTime;
//
//         //진행도 계산
//         float progress = transitionDuration <= 0f ? 1f : Mathf.Clamp01(transitionTimer / transitionDuration);
//
//         //천천히 가속 하다가 빠르게 이동하고, 마지막은 또 천천히 감속하기 위한 계산식
//         float smoothProgress = progress * progress * (3f - 2f * progress);
//
//         //진행도 가속 계산식을 이용해서, 카메라 위치 조정
//         followPosition = Vector3.Lerp(transitionStartPosition, targetPosition, smoothProgress);
//
//         //아직 전부 진행하지 않았으면 패스
//         if (progress < 1f)
//             return;
//
//         //전부 진행했으면 위치 고정 및 상태변경
//         followPosition = targetPosition;
//         currentState = nextState;
//     }
//     
//     //고정 영역 탈출 시
//     public void ExitFixedArea(CameraFixedArea fixedArea, float duration)
//     {
//         //현재 고정 영역과 영역이 다르다면 패스
//         if (currentFixedArea != fixedArea) return;
//
//         //현재 영역 해제 후, 고정 해제 작업 시작
//         currentFixedArea = null;
//         StartTransition(CameraState.ReturnToFollow, duration);
//     }
//
//     //카메라 흔들림(피격 연출)
//     public void PlayHitShake()
//     {
//         if (shakeCoroutine != null)
//             StopCoroutine(shakeCoroutine);
//
//         shakeCoroutine = StartCoroutine(HitShake());
//     }
//
//     //흔들림 처리
//     private IEnumerator HitShake()
//     {
//         float elapsedTime = 0f;
//
//         while (elapsedTime < hitShakeDuration)
//         {
//             elapsedTime += Time.unscaledDeltaTime;
//
//             float progress = Mathf.Clamp01(elapsedTime / hitShakeDuration);
//
//             float damping = (1f - progress) * (1f - progress);
//             float shake = Mathf.Sin(progress * Mathf.PI * 6f) * hitShakeDistance * damping;
//
//             shakeOffset = new Vector3(shake, 0f, 0f);
//
//             yield return null;
//         }
//
//         shakeOffset = Vector3.zero;
//         shakeCoroutine = null;
//     }
// }