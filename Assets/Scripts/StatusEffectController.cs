using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    private readonly Dictionary<StatusEffectType, float> activeEffects = new();
    private readonly List<StatusEffectType> effectKeys = new();

    private void Update()
    {
        UpdateEffects();
    }

    //상태효과 추가 및 지속시간 갱신
    public void Add(StatusEffectType type, float duration)
    {
        activeEffects[type] = duration;
    }

    //상태효과 보유 여부 확인
    public bool Has(StatusEffectType type)
    {
        return activeEffects.ContainsKey(type);
    }

    //상태효과 제거
    public void Remove(StatusEffectType type)
    {
        activeEffects.Remove(type);
    }

    //상태효과 지속시간 갱신
    private void UpdateEffects()
    {
        if (activeEffects.Count == 0) return;

        effectKeys.Clear();
        effectKeys.AddRange(activeEffects.Keys);

        foreach (StatusEffectType type in effectKeys)
        {
            float remainingTime = activeEffects[type] - Time.deltaTime;

            if (remainingTime <= 0f)
            {
                activeEffects.Remove(type);
                continue;
            }

            activeEffects[type] = remainingTime;
        }
    }
}