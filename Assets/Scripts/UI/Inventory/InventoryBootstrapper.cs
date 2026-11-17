using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

namespace MyInventory
{
    public class InventoryBootstrapper : MonoBehaviour
    {
        //인벤토리의 모델과 뷰를 이용해서 뷰모델 생성 및 연결하기 위해 연결받음
        [SerializeField, Range(8, 64)] int _initialCapacity = 32;
        [SerializeField, Range(8, 64)] int _maxCapacity = 64;
        [SerializeField] InventoryView _inventoryView;
        
        [Header("아이템 목록")]
        [SerializeField]  ItemData[] _itemDatabase;
        
        //인벤토리 모델
        private Inventory _inventoryModel;

        private void Start()
        {
            if (_inventoryView == null) return;
            
            //인벤토리 모델 생성
            _inventoryModel = new Inventory(_initialCapacity, _maxCapacity, _inventoryView.ColumnCount);
            
            //인벤토리 모델 데이터를 기준으로 뷰모델 생성 이후 뷰에서 바인딩 진행
            InventoryViewModel viewModel = new InventoryViewModel(_inventoryModel);
            _inventoryView.Bind(viewModel);

            //바인딩 이후 초기화 진행
            _inventoryModel.Initialize();
            
            //게임 시작 시 모든 아이템 종류를 인벤토리에 넣어줌
            if (_itemDatabase != null && _itemDatabase.Length > 0)
            {
                foreach (var itemData in _itemDatabase)
                {
                    if (itemData == null) continue;

                    _inventoryModel.Add(itemData);
                }
            }
        }
        
        //인벤토리 열기 및 닫기
        public void OnInventory(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            _inventoryView.ToggleWindow();
        }
        
        //R키 입력 시 석판 회전
        public void OnRotateTablet(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            _inventoryView?.RotateCurrentTablet();
        }

        //K키 테스트 아이템 획득
        public void OnTestItem(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (_itemDatabase != null && _itemDatabase.Length > 0)
            {
                int randomIndex = Random.Range(0, _itemDatabase.Length);
                ItemData randomItem = _itemDatabase[randomIndex];

                _inventoryModel.Add(randomItem);

                Debug.Log($"[Test Drop] {randomItem.Name} 획득!");
            }
        }
    }
}