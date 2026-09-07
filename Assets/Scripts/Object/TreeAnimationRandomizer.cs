using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TreeAnimationRandomizer : MonoBehaviour
{
    //시작 딜레이
    private const float MinStartDelay = 0f;
    private const float MaxStartDelay = 0.5f;

    //애니메이션 속도
    private const float MinAnimationSpeed = 0.8f;
    private const float MaxAnimationSpeed = 1.2f;

    private static readonly int CycleOffsetHash = Animator.StringToHash("CycleOffset");

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        //0~1의 랜덤값을 지정해줘서, 나무들의 애니메이션 시작 프레임을 랜덤하게 변경
        animator.SetFloat(CycleOffsetHash, Random.value);
        animator.speed = 0f;
    }

    private IEnumerator Start()
    {
        //애니메이션 시작 딜레이 랜덤 지정
        float startDelay = Random.Range(MinStartDelay, MaxStartDelay);

        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        //애니메이션 재생 속도 랜덤 지정
        animator.speed = Random.Range(
            MinAnimationSpeed,
            MaxAnimationSpeed
        );
    }
}