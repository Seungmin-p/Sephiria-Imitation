using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace MyInventory
{
    [RequireComponent(typeof(UIDocument))]
    public class InventoryView : MonoBehaviour
    {
        [Header("UI 템플릿")] 
        //아이템 슬롯, 툴팁, 석판 내부 셀
        [SerializeField] VisualTreeAsset _slotTemplate;
        [SerializeField] VisualTreeAsset _tooltipTemplate; 
        [SerializeField] VisualTreeAsset _tabletCellTemplate;

        [Header("UI 커서")]
        [SerializeField] MouseCursor _mouseCursor;
        
        [Header("등급별 슬롯 배경")]
        [SerializeField] Texture2D[] _slotBackgrounds = new Texture2D[5];
        
        [Header("석판 전용 슬롯 배경")]
        [SerializeField] Texture2D _tabletSlotBackground;
        
        [Header("아티팩트 세트 표시 정보")]
        [SerializeField] ArtifactSetDisplayData[] _artifactSetDisplays;
        
        [Header("아이템 효과 표시 정보")]
        [SerializeField] EffectDisplayData[] _effectDisplays;
        
        [Header("포션 효과 표시 정보")]
        [SerializeField] PotionEffectDisplayData[] _potionEffectDisplays;

        [Header("인벤토리 구성을 위한 슬롯 정보")]
        //인벤토리 한줄에 들어갈 슬롯의 수, 슬롯의 크기, 슬롯별로 할당된 좌+우 마진 크기
        [SerializeField] int _columnCount = 6;
        
        //뷰모델 데이터
        private InventoryViewModel _viewModel;
        
        //인벤토리 레이아웃
        //UI 제어용 변수, 인벤토리 창, 슬롯영역
        private VisualElement _root;
        private VisualElement _inventoryWindow;
        private VisualElement _slotArea;

        //실제 팝업 제어용 인스턴스
        private InventoryTooltipView _tooltipView;

        //슬롯 뷰 들을 순서대로 담아둘 리스트
        private readonly List<ItemSlotView> _slotViews = new List<ItemSlotView>();
        
        //현재 마우스가 올라간 슬롯 인덱스
        private int _hoveredSlotIndex = -1;
        
        //현재 드래그 중인 아이템의 원래 슬롯 인덱스
        private int _movingSlotIndex = -1;
        
        //인벤토리 한 줄의 슬롯 개수
        public int ColumnCount => Mathf.Max(1, _columnCount);

        //바인딩처리
        public void Bind(InventoryViewModel viewModel)
        {
            //혹시 모를 중복 바인딩을 방지하기 위한 언바인드 실행
            Unbind();
            
            //뷰 모델 연결
            _viewModel = viewModel;
            var document = GetComponent<UIDocument>();
            _root = document.rootVisualElement;

            // TextCore가 툴팁 크기를 계산하기 전에 SpriteCharacter와 Sprite Asset을 연결한다.
            document.panelSettings?.textSettings?.defaultSpriteAsset?.UpdateLookupTables();
            
            //슬롯 배경 이미지 연결
            _viewModel.SetSlotBackgrounds(_slotBackgrounds, _tabletSlotBackground);

            //인벤토리 창, 슬롯 영역
            _inventoryWindow = _root.Q<VisualElement>("Inventory");
            _slotArea = _root.Q<VisualElement>("Slot-Area");

            //아이템 설명(툴팁) 팝업 초기화
            if (InstantiateAndAttach(_tooltipTemplate) is VisualElement tooltipRoot)
                _tooltipView = new InventoryTooltipView(tooltipRoot, _tabletCellTemplate, _artifactSetDisplays, _effectDisplays, _potionEffectDisplays);
            
            //슬롯 및 UI 이벤트 설정
            GenerateSlotsUI();

            //인벤토리창은 시작할땐 기본적으로 숨김처리
            if (_inventoryWindow != null)
            {
                _inventoryWindow.style.display = DisplayStyle.None;
            }
        }
        
        //세 팝업들을 초기화 하기 위한 메소드
        private VisualElement InstantiateAndAttach(VisualTreeAsset template)
        {
            if (template == null || _root == null) return null;

            //툴팁 인스턴스 생성 이후, UI Toolkit이 임의로 씌운 껍데기(TemplateContainer)를 벗겨내고 실제 루트 데이터 가져오기
            TemplateContainer container = template.Instantiate();
            VisualElement realRoot = container.childCount > 0 ? container[0] : null;

            if (realRoot != null)
            {
                //TemplateContainer에 연결된 스타일시트를 실제 루트에 전달
                for (int i = 0; i < container.styleSheets.count; i++)
                {
                    StyleSheet styleSheet = container.styleSheets[i];

                    if (!realRoot.styleSheets.Contains(styleSheet))
                        realRoot.styleSheets.Add(styleSheet);
                }
                
                //root 추가 및 뷰 등록
                _root.Add(realRoot);
            }

            return realRoot;
        }

        //슬롯 배치
        private void GenerateSlotsUI()
        {
            if (_slotArea == null || _slotTemplate == null) return;

            //기존 이벤트가 있다면 모두 제거
            foreach (var slotView in _slotViews)
            {
                slotView.ClearSubscribedEvents();
            }
            
            //시작할 때 깔끔하게 슬롯 정보 클리어
            _slotArea.Clear();
            _slotViews.Clear();

            const float slotGap = 7f;
            int lastRow = (_viewModel.Capacity - 1) / _columnCount;
            
            //가방 크기만큼 반복
            for (int i = 0; i < _viewModel.Capacity; i++)
            {
                //슬롯 인스턴스를 가져와서 영역에 추가
                VisualElement slotInstance = _slotTemplate.Instantiate();

                //마지막 열과 마지막 행을 제외하고 간격 적용
                slotInstance.style.marginRight = (i + 1) % _columnCount == 0 ? 0f : slotGap;
                slotInstance.style.marginBottom = i / _columnCount == lastRow ? 0f : slotGap;

                _slotArea.Add(slotInstance);

                //슬롯 인스턴스의 진짜 데이터를 가져와서 유저 데이터에 저장하고, 피킹모드를 명확하게 지정
                VisualElement realSlotRoot = slotInstance[0];
                realSlotRoot.userData = i;
                realSlotRoot.pickingMode = PickingMode.Position;

                //ItemSlotView 생성 및 연결
                ItemSlotView slotView = new ItemSlotView(
                    realSlotRoot,
                    _root,
                    i,
                    (fromIdx, toIdx) => _viewModel.RequestSwapSlots(fromIdx, toIdx),
                    index => _movingSlotIndex = index,
                    index =>
                    {
                        if (_movingSlotIndex == index)
                            _movingSlotIndex = -1;
                    }
                );

                //툴팁 호출 이벤트 연결
                slotView.OnPointerEnterSlot += (item) =>
                {
                    _hoveredSlotIndex = slotView.Index;
                    _tooltipView?.Show(item);
                };
                
                //툴팁 소멸 이벤트 연결
                slotView.OnPointerExitSlot += () =>
                {
                    if (_hoveredSlotIndex == slotView.Index)
                        _hoveredSlotIndex = -1;

                    _tooltipView?.Hide();
                };

                //슬롯 뷰에 슬롯 뷰 모델 바인딩처리를 진행하고 슬롯 뷰 목록에 추가
                slotView.Bind(_viewModel.Slots[i]);
                _slotViews.Add(slotView);
            }
        }

        //I키가 입력되면 실행
        public void ToggleWindow()
        {
            //인벤토리 창 활성화, 만약 이미 활성화 되어있다면 비활성화
            if (_inventoryWindow == null) return;
            bool isVisible = _inventoryWindow.style.display == DisplayStyle.Flex;
            _inventoryWindow.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
            _mouseCursor?.SetUIWindowOpen(this, !isVisible);
            
            //인벤토리가 닫히면 툴팁도 닫아줌
            if (isVisible) _tooltipView?.Hide();
        }
        
        //현재 선택된 석판 회전
        public void RotateCurrentTablet()
        {
            //인벤토리가 열려 있지 않으면 회전하지 않음
            if (_viewModel == null || _inventoryWindow == null || _inventoryWindow.style.display != DisplayStyle.Flex)
                return;

            //드래그 중인 아이템을 우선 사용, 없다면 마우스가 올라간 슬롯 사용
            int index = _movingSlotIndex >= 0 ? _movingSlotIndex : _hoveredSlotIndex;

            //올바른 슬롯인지 확인
            if (index < 0 || index >= _viewModel.Slots.Count) return;

            //회전 가능한 석판인지 확인
            if (_viewModel.Slots[index].Item is not TabletItem tablet || tablet.TabletData.CannotRotate)
                return;

            //석판 회전 요청
            _viewModel.RequestRotateTablet(index);
            
            //현재 표시 중인 석판 툴팁 갱신
            if (_hoveredSlotIndex == index && _tooltipView?.IsVisible == true)
                _tooltipView.Show(tablet);
        }
        
        //삭제 시 언바인드
        private void OnDestroy()
        {
            Unbind();
        }
        
        private void Unbind()
        {
            _mouseCursor?.SetUIWindowOpen(this, false);

            //뷰 모델의 언바인드 진행
            if (_viewModel != null) _viewModel.UnbindFromModel();

            _viewModel = null;
        }
    }
}
