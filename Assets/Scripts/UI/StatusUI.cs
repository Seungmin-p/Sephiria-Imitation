using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class StatusUI : MonoBehaviour
{
    [Header("플레이어 스탯")]
    [SerializeField] PlayerStats playerStats;
    
    [Header("UI 커서")]
    [SerializeField] MouseCursor mouseCursor;
    
    //HP, MP
    private Label hpStat;
    private Label mpStat;

    //스탯 UI
    private VisualElement statusWindow;

    //탭 및 영역
    private VisualElement commonTab;
    private VisualElement professTab;
    private VisualElement commonStatusArea;
    private VisualElement professStatusArea;

    //탭 이미지
    private StyleBackground activeTabBackground;
    private StyleBackground inactiveTabBackground;

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        //UI 요소 확보
        statusWindow = root.Q<VisualElement>("Status");
        commonTab = root.Q<VisualElement>("StatusCommonTab");
        professTab = root.Q<VisualElement>("StatusProfessTab");
        commonStatusArea = root.Q<VisualElement>("CommonStatusArea");
        professStatusArea = root.Q<VisualElement>("ProfessStatusArea");
        
        hpStat = root.Q<Label>("HPStat");
        mpStat = root.Q<Label>("MPStat");
        
        statusWindow.dataSource = playerStats;

        //탭 활성화, 비활성화 이미지 저장
        activeTabBackground = commonTab.style.backgroundImage;
        inactiveTabBackground = professTab.style.backgroundImage;

        //탭 클릭 이벤트 등록
        commonTab.RegisterCallback<ClickEvent>(OnCommonTabClick);
        professTab.RegisterCallback<ClickEvent>(OnProfessTabClick);

        //초기 상태
        SelectTab(true);
        statusWindow.style.display = DisplayStyle.None;
        
        //스탯 변경 이벤트 구독
        playerStats.OnHealthChanged += UpdateHealth;
        playerStats.OnManaChanged += UpdateMana;
    }

    private void OnDisable()
    {
        //이벤트 해제
        commonTab?.UnregisterCallback<ClickEvent>(OnCommonTabClick);
        professTab?.UnregisterCallback<ClickEvent>(OnProfessTabClick);
        
        playerStats.OnHealthChanged -= UpdateHealth;
        playerStats.OnManaChanged -= UpdateMana;

        //UI 비활성화
        if (statusWindow != null)
            statusWindow.style.display = DisplayStyle.None;

        mouseCursor?.SetUIWindowOpen(this, false);
    }

    //스탯 UI 열기 및 닫기
    public void ToggleWindow()
    {
        if (statusWindow == null) return;

        bool isVisible = statusWindow.style.display == DisplayStyle.Flex;

        statusWindow.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
        mouseCursor?.SetUIWindowOpen(this, !isVisible);
        
        //스탯을 열 때 HP, MP 갱신
        if (!isVisible)
        {
            UpdateHealth();
            UpdateMana();
        }
    }

    //스탯 UI 입력
    public void OnStatus(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        ToggleWindow();
    }

    //공통 능력치 클릭
    private void OnCommonTabClick(ClickEvent evt)
    {
        SelectTab(true);
    }

    //전문 능력치 클릭
    private void OnProfessTabClick(ClickEvent evt)
    {
        SelectTab(false);
    }

    //탭 전환
    private void SelectTab(bool isCommon)
    {
        //능력치 영역 표시 전환
        commonStatusArea.style.display = isCommon ? DisplayStyle.Flex : DisplayStyle.None;
        professStatusArea.style.display = isCommon ? DisplayStyle.None : DisplayStyle.Flex;

        //탭 이미지 교체
        commonTab.style.backgroundImage = isCommon ? activeTabBackground : inactiveTabBackground;
        professTab.style.backgroundImage = isCommon ? inactiveTabBackground : activeTabBackground;
    }
    
    //HP 갱신
    private void UpdateHealth()
    {
        if (statusWindow.style.display != DisplayStyle.Flex) return;

        hpStat.text = $"{Mathf.CeilToInt(playerStats.CurrentHealth)}/{Mathf.CeilToInt(playerStats.MaxHealth)}";
    }
    
    //MP 갱신
    private void UpdateMana()
    {
        if (statusWindow.style.display != DisplayStyle.Flex) return;

        mpStat.text = $"{Mathf.CeilToInt(playerStats.CurrentMana)}/{Mathf.CeilToInt(playerStats.MaxMana)}";
    }
}