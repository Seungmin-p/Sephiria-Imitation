using UnityEngine;

public class BreakableObject : MonoBehaviour, IDamageable
{
    [System.Serializable]
    private struct FragmentData
    {
        //파괴 후 떨어질 조각 프리팹과, 생성할 개수
        public FragmentEffect prefab;
        [Min(1)] public int count;
    }

    [Header("파괴 전/후 오브젝트")]
    [SerializeField] private GameObject breakBefore;
    [SerializeField] private GameObject breakAfter;

    [Header("파괴 파편")]
    [SerializeField] private FragmentData[] fragments;

    private bool isBroken;
    
    //count 기본값 1로 설정
    private void OnValidate()
    {
        if (fragments == null) return;

        for (int i = 0; i < fragments.Length; i++)
        {
            FragmentData fragmentData = fragments[i];

            if (fragmentData.count < 1)
            {
                fragmentData.count = 1;
                fragments[i] = fragmentData;
            }
        }
    }

    private void Awake()
    {
        //시작 직후엔 파괴 전 오브젝트 활성화, 파괴 후 오브젝트는 비활성화
        if (breakBefore != null) breakBefore.SetActive(true);
        if (breakAfter != null) breakAfter.SetActive(false);
    }

    //피격(파괴) 처리
    public DamageResult TakeDamage(AttackData attackData)
    {
        //이미 파괴되었거나 플레이어의 공격이 아니라면 패스
        if (isBroken || attackData.source != DamageSource.Player)
        {
            return new DamageResult(
                0f,
                false,
                DamageResult.HitResultType.Ignored,
                attackData.element,
                attackData.source
            );
        }

        //패스되지 않았다면 파괴처리 진행
        Break();

        //피격 이벤트를 위한 ObjectHit 판정 반환
        return new DamageResult(
            0f,
            false,
            DamageResult.HitResultType.ObjectHit,
            attackData.element,
            attackData.source
        );
    }

    //오브젝트 파괴
    private void Break()
    {
        isBroken = true;

        //파편 생성
        SpawnFragments();

        //파괴 전 오브젝트 비활성화
        if (breakBefore != null)
            breakBefore.SetActive(false);

        //파괴 후 오브젝트 활성화
        if (breakAfter != null)
            breakAfter.SetActive(true);
    }

    //파괴 파편 생성
    private void SpawnFragments()
    {
        if (fragments == null) return;

        //현재 위치를 기준으로 스폰 진행
        Vector3 spawnPosition = transform.position;

        foreach (FragmentData fragmentData in fragments)
        {
            if (fragmentData.prefab == null) continue;

            for (int i = 0; i < fragmentData.count; i++)
            {
                //랜덤한 방향 설정
                float randomAngle = Random.Range(0f, 360f);
                Vector2 direction = Quaternion.Euler(0f, 0f, randomAngle) * Vector2.right;

                //파편 스폰 진행, 세부 위치는 파편 고유 설정값에 따라 달라짐
                FragmentEffect fragment = Instantiate(fragmentData.prefab, spawnPosition, Quaternion.identity);
                fragment.Initialize(direction);
            }
        }
    }
}