using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace MyInventory
{
    public class ItemSlotView
    {
        //각종 UI 처리용
        private readonly VisualElement _rootElement;
        private readonly VisualElement _screenRoot;
        
        //뷰 모델, 인덱스
        private ItemSlotViewModel _viewModel;
        private readonly int _index;
        
        //드래그 조작기
        private readonly ItemDragManipulator _dragManipulator;
        
        //현재 슬롯 인덱스
        public int Index => _index;
        
        //아이템 이동
        private readonly Action<int, int> _onDropRequest; 

        //마우스가 슬롯 영역에 들어오고 나가는 부분에 대한 판정용 이벤트 
        public event Action<Item> OnPointerEnterSlot;
        public event Action OnPointerExitSlot;

        public ItemSlotView(
            VisualElement slotVisualElement,
            VisualElement screenRoot,
            int index,
            Action<int, int> onDropRequest,
            Action<int> onDragStarted,
            Action<int> onDragEnded)
        {
            //전달받은 각종 데이터 저장
            _rootElement = slotVisualElement;
            _screenRoot = screenRoot;
            _index = index;
            _onDropRequest = onDropRequest;
    
            //ItemDragManipulator 인스턴스 확보
            _dragManipulator = new ItemDragManipulator(_screenRoot, _index, () => _viewModel?.HasItem ?? false, _onDropRequest, onDragStarted, onDragEnded);
            
            //드래그 조작기 등록
            _rootElement.AddManipulator(_dragManipulator);

            //진입, 이탈 감지용
            _rootElement.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            _rootElement.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }
        
        //인벤토리 뷰를 통해 호출되는 뷰모델 바인딩
        public void Bind(ItemSlotViewModel viewModel)
        {
            _viewModel = viewModel;
            
            //런타임 바인딩을 위해, 루트 엘리먼트의 데이터 소스를 명확하게 지정
            _rootElement.dataSource = viewModel;
            
            //슬롯 데이터가 변경될 때 마다 렌더 실행
            _viewModel.OnStateChanged += Render;
            
            Render();
        }
        
        //슬롯의 뷰 모델 데이터가 변하면
        private void Render()
        {
            if (_viewModel == null) return;

            //아이템 이미지 영역 가져오기
            VisualElement itemIcon = _rootElement.Q<VisualElement>("Item-Icon");
            
            //드래그 중이라면 고스트 아이콘의 회전 상태도 업데이트
            _dragManipulator.UpdateGhostRotation(_viewModel.IconRotation);
            
            //슬롯에 아이템이 있다면
            if (_viewModel.HasItem)
            {
                float iconScale = _viewModel.ItemData.IconScale;
                itemIcon.style.scale = new Scale(new Vector2(iconScale, iconScale));
            }
            //아이템이 없다면
            else
            {
                //아이콘 크기 초기화
                itemIcon.style.scale = new Scale(Vector2.one);
            }
        }

        //마우스가 슬롯 영역에 들어오면 실행
        private void OnPointerEnter(PointerEnterEvent evt)
        {
            //아이템이 있다면
            if (_viewModel != null && _viewModel.HasItem)
            {
                //툴팁 출력을 위한 이벤트 호출
                OnPointerEnterSlot?.Invoke(_viewModel.Item);
            }
        }

        //마우스가 슬롯을 빠져 나갈 때 실행
        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            //팝업(툴팁) 제거
            ClearPopup();
        }

        private void ClearPopup()
        {
            //팝업 제거
            OnPointerExitSlot?.Invoke();
        }

        public void ClearSubscribedEvents()
        {
            //이벤트 자체를 null로 만들어서 람다식 포함 모든 이벤트 연결 해제
            OnPointerEnterSlot = null;
            OnPointerExitSlot = null;
            
            //추후 재사용 등을 위한 바인딩 연결 해제
            if (_viewModel != null) _viewModel.OnStateChanged -= Render;
            if (_rootElement != null) _rootElement.dataSource = null;
            _viewModel = null;
        }
    }
}