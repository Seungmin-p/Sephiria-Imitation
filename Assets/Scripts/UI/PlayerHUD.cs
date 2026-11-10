using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UIImage = UnityEngine.UI.Image;

public class PlayerHUD : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] UIDocument uiDocument;
    [SerializeField] PlayerStats playerStats;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] VisualTreeAsset dashSlotTemplate;
    
    [Header("아이콘 이미지")]
    [SerializeField] Texture2D inventoryDefaultTexture;
    [SerializeField] Texture2D inventoryHoverTexture;
    [SerializeField] Texture2D statsDefaultTexture;
    [SerializeField] Texture2D statsHoverTexture;
    
    [Header("미니 대시 UI")]
    [SerializeField] RectTransform miniDashBackground;
    [SerializeField] UIImage miniDashSlotPrefab;

    //체력 및 마나
    private VisualElement hp;
    private VisualElement mp;
    private Label hpText;
    private Label mpText;
    
    //대시
    private class DashSlot
    {
        //메인 대시 아이콘
        public VisualElement Background;
        public VisualElement FillMask;
        
        //플레이어 아래에 나오는 미니 대시 아이콘
        public UIImage MiniIcon;
    }
    private VisualElement dashArea;
    private readonly List<DashSlot> dashSlots = new();
    
    //아이콘 UI
    private VisualElement inventoryIcon;
    private VisualElement statsIcon;
    
    //UI 갱신하면서 시작
    private void Start()
    {
        UpdateHealth();
        UpdateMana();
        UpdateDash();
    }

    private void OnEnable()
    {
        //UI 요소 확보
        VisualElement root = uiDocument.rootVisualElement;

        //체력, 마나, 대시, 아이콘 UI 확보
        hp = root.Q<VisualElement>("HP");
        mp = root.Q<VisualElement>("MP");
        hpText = root.Q<Label>("HPText");
        mpText = root.Q<Label>("MPText");
        dashArea = root.Q<VisualElement>("DashArea");
        inventoryIcon = root.Q<VisualElement>("InventoryIcon");
        statsIcon = root.Q<VisualElement>("StatsIcon");

        //호버 이벤트 등록
        inventoryIcon.RegisterCallback<PointerEnterEvent>(OnIconEnter);
        inventoryIcon.RegisterCallback<PointerLeaveEvent>(OnIconLeave);

        statsIcon.RegisterCallback<PointerEnterEvent>(OnIconEnter);
        statsIcon.RegisterCallback<PointerLeaveEvent>(OnIconLeave);

        //이벤트 구독
        playerStats.OnHealthChanged += UpdateHealth;
        playerStats.OnManaChanged += UpdateMana;
        playerMovement.OnDashChanged += UpdateDash;
    }

    private void OnDisable()
    {
        //이벤트 구독 해제
        playerStats.OnHealthChanged -= UpdateHealth;
        playerStats.OnManaChanged -= UpdateMana;
        playerMovement.OnDashChanged -= UpdateDash;
        
        //아이콘 호버 이벤트 해제
        inventoryIcon.UnregisterCallback<PointerEnterEvent>(OnIconEnter);
        inventoryIcon.UnregisterCallback<PointerLeaveEvent>(OnIconLeave);
        statsIcon.UnregisterCallback<PointerEnterEvent>(OnIconEnter);
        statsIcon.UnregisterCallback<PointerLeaveEvent>(OnIconLeave);

        //아이콘 기본 이미지 복구
        inventoryIcon.style.backgroundImage = inventoryDefaultTexture;
        statsIcon.style.backgroundImage = statsDefaultTexture;
    }

    //체력 갱신
    private void UpdateHealth()
    {
        UpdateStat(hp, hpText, playerStats.CurrentHealth, playerStats.MaxHealth);
    }

    //마나 갱신
    private void UpdateMana()
    {
        UpdateStat(mp, mpText, playerStats.CurrentMana, playerStats.MaxMana);
    }

    //체력 or 마나 갱신
    private void UpdateStat(VisualElement bar, Label text, float current, float max)
    {
        float progress = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        bar.style.width = Length.Percent(progress * 100f);
        text.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }
    
    //대시 업데이트
    private void UpdateDash()
    {
        SetupDashSlots();

        int current = playerMovement.CurrentDashCount;
        int maxCount = playerMovement.MaxDashCount;
        float rechargeProgress = playerMovement.DashRechargeProgress;

        for (int i = 0; i < dashSlots.Count; i++)
        {
            //현재 개수만큼은 1f, 충전중인 대시에 대해서는 충전 비율
            float progress = i < current ? 1f : i == current ? rechargeProgress : 0f;

            //대시 충전은 width 0~17까지
            dashSlots[i].FillMask.style.width = 17f * progress;

            //미니 대시 충전 비율 갱신
            dashSlots[i].MiniIcon.fillAmount = progress;
        }

        //모든 대시가 충전되었다면 미니 UI 숨김
        miniDashBackground.gameObject.SetActive(current < maxCount);
    }
    
    //대시 슬롯 개수 조정
    private void SetupDashSlots()
    {
        int maxCount = Mathf.Max(0, playerMovement.MaxDashCount);

        //부족한 슬롯 생성
        while (dashSlots.Count < maxCount)
        {
            VisualElement background = dashSlotTemplate.Instantiate().Q<VisualElement>("DashBackground");
            UIImage miniIcon = Instantiate(miniDashSlotPrefab, miniDashBackground);

            DashSlot slot = new()
            {
                Background = background,
                FillMask = background.Q<VisualElement>("DashFillMask"),
                MiniIcon = miniIcon
            };

            dashArea.Add(slot.Background);
            dashSlots.Add(slot);
        }

        //초과한 슬롯 제거
        while (dashSlots.Count > maxCount)
        {
            DashSlot slot = dashSlots[^1];

            //UI 계층 및 C# 리스트에서 전부 제거
            slot.Background.RemoveFromHierarchy();
            Destroy(slot.MiniIcon.gameObject);
            dashSlots.RemoveAt(dashSlots.Count - 1);
        }
    }
    
    //아이콘에 마우스 진입
    private void OnIconEnter(PointerEnterEvent evt)
    {
        if (evt.currentTarget == inventoryIcon)
            inventoryIcon.style.backgroundImage = new StyleBackground(inventoryHoverTexture);
        else if (evt.currentTarget == statsIcon)
            statsIcon.style.backgroundImage = new StyleBackground(statsHoverTexture);
    }

    //아이콘에서 마우스 이탈
    private void OnIconLeave(PointerLeaveEvent evt)
    {
        if (evt.currentTarget == inventoryIcon)
            inventoryIcon.style.backgroundImage = new StyleBackground(inventoryDefaultTexture);
        else if (evt.currentTarget == statsIcon)
            statsIcon.style.backgroundImage = new StyleBackground(statsDefaultTexture);
    }
}