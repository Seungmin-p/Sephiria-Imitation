using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CombatFeedbackManager : MonoBehaviour
{
    public enum EffectType
    {
        Guard,
        PerfectGuard,
        GuardBreak,
        PlayerDeath
    }
    
    [System.Serializable]
    private class CombatEffectData
    {
        public EffectType type;
        public GameObject prefab;
        public float positionOffset;
        public bool rotateWithDirection;
        public float angleOffset;
    }
    
    [Header("UI")]
    [SerializeField] private UIDocument combatUIDocument;
    [SerializeField] private VisualTreeAsset damageFontTemplate;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("몬스터 데미지 폰트")]
    [SerializeField] private float damageFontDuration = 0.6f;
    [SerializeField] private Vector2 damageFontMoveOffset = new Vector2(35f, -45f);
    [SerializeField, Range(0f, 1f)] private float damageFontFadeStart = 0.25f;

    [Header("플레이어 데미지 폰트")]
    [SerializeField] private float playerDamageFontDuration = 0.6f;
    [SerializeField] private Vector2 playerDamageRiseOffset = new Vector2(15f, -20f);
    [SerializeField] private Vector2 playerDamageFallOffset = new Vector2(30f, 15f);
    [SerializeField, Range(0f, 1f)] private float playerDamagePeakTime = 0.3f;
    [SerializeField] private float playerDamageEndScale = 0.6f;
    
    [Header("이펙트")]
    [SerializeField] private CombatEffectData[] combatEffects;

    private VisualElement combatUIBase;
    private readonly Dictionary<EffectType, CombatEffectData> effectDictionary = new();

    private void Awake()
    {
        combatUIBase = combatUIDocument.rootVisualElement.Q<VisualElement>("CombatUIBase");
        
        foreach (CombatEffectData effect in combatEffects)
            effectDictionary[effect.type] = effect;
    }

    private void OnEnable()
    {
        AttackHitbox.OnDamageResolved += ShowDamage;
        PlayerDeath.OnDeathStarted += ShowPlayerDeathEffect;
    }

    private void OnDisable()
    {
        AttackHitbox.OnDamageResolved -= ShowDamage;
        PlayerDeath.OnDeathStarted -= ShowPlayerDeathEffect;
    }

    //데미지 폰트 표시
    private void ShowDamage(Vector2 worldPosition, Vector2 hitDirection, DamageResult result)
    {
        //무시된 공격이라면 패스
        if (result.hitResultType == DamageResult.HitResultType.Ignored)
            return;
        
        //오브젝트 공격 또한 패스
        if (result.hitResultType == DamageResult.HitResultType.ObjectHit)
            return;
        
        //이펙트 출력
        ShowDamageEffect(worldPosition, hitDirection, result);
        
        //결과에 따른 폰트 출력
        switch (result.hitResultType)
        {
            case DamageResult.HitResultType.Evade:
                ShowCombatText(worldPosition, "EVADE", new Color(100f / 255f, 1f, 210f / 255f));
                return;

            case DamageResult.HitResultType.Guard:
                return;

            case DamageResult.HitResultType.PerfectGuard:
                ShowCombatText(worldPosition, "PERFECT GUARD!", Color.cyan);
                return;

            case DamageResult.HitResultType.GuardBreak:
                ShowCombatText(worldPosition, "GUARD BREAK", Color.red);
                return;
        }
        TemplateContainer damageFont = CreateCombatPopup(worldPosition);

        VisualElement criticalIcon = damageFont.Q<VisualElement>("CriticalIcon");
        Label damageText = damageFont.Q<Label>("DamageFontText");

        damageText.text = Mathf.RoundToInt(result.damage).ToString();

        criticalIcon.style.display = result.isCritical ? DisplayStyle.Flex : DisplayStyle.None;

        ApplyDamageStyle(damageText, result);

        if (result.source == DamageSource.Enemy)
            StartCoroutine(AnimatePlayerDamageFont(damageFont, worldPosition));
        else
            StartCoroutine(AnimateDamageFont(damageFont, worldPosition));
    }
    
    //이펙트 출력
    private void ShowDamageEffect(Vector2 worldPosition, Vector2 hitDirection, DamageResult result)
    {
        switch (result.hitResultType)
        {
            case DamageResult.HitResultType.Guard:
                SpawnEffect(EffectType.Guard, worldPosition, hitDirection);
                break;

            case DamageResult.HitResultType.PerfectGuard:
                SpawnEffect(EffectType.PerfectGuard, worldPosition, hitDirection);
                break;

            case DamageResult.HitResultType.GuardBreak:
                SpawnEffect(EffectType.Guard, worldPosition, hitDirection);
                SpawnEffect(EffectType.GuardBreak, worldPosition, hitDirection);
                break;
        }
    }
    
    //플레이어 사망 이펙트 출력
    private void ShowPlayerDeathEffect(Vector2 worldPosition, Vector2 hitDirection)
    {
        SpawnEffect(EffectType.PlayerDeath, worldPosition, hitDirection);
    }
    
    //지정한 위치에 이펙트 생성
    private void SpawnEffect(EffectType type, Vector2 worldPosition, Vector2 hitDirection)
    {
        //관련 이펙트를 확인하지 못하면 패스
        if (!effectDictionary.TryGetValue(type, out CombatEffectData effect))
            return;

        //출력 방향 및 위치 확보
        Vector2 direction = hitDirection.normalized;
        Vector2 effectPosition = worldPosition + direction * effect.positionOffset;

        Quaternion rotation = Quaternion.identity;

        //각도 보정이 필요하다면 보정 진행
        if (effect.rotateWithDirection)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rotation = Quaternion.Euler(0f, 0f, angle + effect.angleOffset);
        }

        Instantiate(effect.prefab, effectPosition, rotation);
    }
    
    //데미지 폰트 스타일 적용
    private void ApplyDamageStyle(Label damageText, DamageResult result)
    {
        if (result.source == DamageSource.Enemy)
        {
            damageText.style.color = Color.red;
            return;
        }

        damageText.style.color = result.element switch
        {
            ElementType.Fire => new Color(1f, 150f / 255f, 40f / 255f),
            ElementType.Ice => new Color(100f / 255f, 210f / 255f, 1f),
            ElementType.Electric => new Color(1f, 225f / 255f, 40f / 255f),
            _ => Color.white
        };
    }

    //전투 텍스트 표시
    private void ShowCombatText(Vector2 worldPosition, string text, Color color)
    {
        TemplateContainer combatText = CreateCombatPopup(worldPosition);

        VisualElement criticalIcon = combatText.Q<VisualElement>("CriticalIcon");
        Label damageText = combatText.Q<Label>("DamageFontText");

        criticalIcon.style.display = DisplayStyle.None;
        damageText.text = text;
        damageText.style.color = color;

        StartCoroutine(AnimateDamageFont(combatText, worldPosition));
    }

    //전투 UI 생성 및 위치 설정
    private TemplateContainer CreateCombatPopup(Vector2 worldPosition)
    {
        TemplateContainer combatPopup = damageFontTemplate.Instantiate();

        combatPopup.style.position = Position.Absolute;

        combatUIBase.Add(combatPopup);

        Vector2 panelPosition = WorldToPanelPosition(worldPosition);
        combatPopup.style.left = panelPosition.x;
        combatPopup.style.top = panelPosition.y;

        return combatPopup;
    }
    
    private Vector2 WorldToPanelPosition(Vector2 worldPosition)
    {
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        screenPosition.y = Screen.height - screenPosition.y;

        return RuntimePanelUtils.ScreenToPanel(
            combatUIDocument.rootVisualElement.panel,
            screenPosition
        );
    }

    //적 피격 데미지 폰트 애니메이션
    private IEnumerator AnimateDamageFont(VisualElement damageFont, Vector2 worldPosition)
    {
        float elapsedTime = 0f;

        while (elapsedTime < damageFontDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsedTime / damageFontDuration);

            //현재 카메라 기준으로 원래 타격 월드 좌표를 다시 Panel 좌표로 변환
            Vector2 panelPosition = WorldToPanelPosition(worldPosition);

            damageFont.style.left = panelPosition.x;
            damageFont.style.top = panelPosition.y;

            //데미지 폰트 자체의 이동 연출
            float moveX = Mathf.Lerp(0f, damageFontMoveOffset.x, progress);
            float moveY = Mathf.Lerp(0f, damageFontMoveOffset.y, progress);

            damageFont.style.translate = new Translate(
                new Length(moveX, LengthUnit.Pixel),
                new Length(moveY, LengthUnit.Pixel)
            );

            //Fade
            float fadeProgress = Mathf.InverseLerp(damageFontFadeStart, 1f, progress);
            damageFont.style.opacity = 1f - fadeProgress;

            yield return null;
        }

        damageFont.RemoveFromHierarchy();
    }

    //플레이어 피격 데미지 폰트 애니메이션
    private IEnumerator AnimatePlayerDamageFont(VisualElement damageFont, Vector2 worldPosition)
    {
        float elapsedTime = 0f;

        while (elapsedTime < playerDamageFontDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsedTime / playerDamageFontDuration);

            //원래 피격 월드 위치 추적
            Vector2 panelPosition = WorldToPanelPosition(worldPosition);

            damageFont.style.left = panelPosition.x;
            damageFont.style.top = panelPosition.y;

            Vector2 moveOffset;

            if (progress < playerDamagePeakTime)
            {
                float riseProgress = progress / playerDamagePeakTime;
                moveOffset = Vector2.Lerp(Vector2.zero, playerDamageRiseOffset, riseProgress);
            }
            else
            {
                float fallProgress = Mathf.InverseLerp(playerDamagePeakTime, 1f, progress);
                moveOffset = Vector2.Lerp(playerDamageRiseOffset, playerDamageFallOffset, fallProgress);
            }

            damageFont.style.translate = new Translate(
                new Length(moveOffset.x, LengthUnit.Pixel),
                new Length(moveOffset.y, LengthUnit.Pixel)
            );

            float scale = Mathf.Lerp(1f, playerDamageEndScale, progress);
            damageFont.style.scale = new Scale(new Vector2(scale, scale));

            damageFont.style.opacity = 1f - progress;

            yield return null;
        }

        damageFont.RemoveFromHierarchy();
    }
}