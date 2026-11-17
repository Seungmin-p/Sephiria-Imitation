using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace MyInventory
{
    public class InventoryViewModel
    {
        //인벤토리 모델, 슬롯 뷰 모델 리스트
        private readonly Inventory _model;
        private readonly List<ItemSlotViewModel> _slots;

        //인벤토리 칸 수를 모델에서 가져오는 프로퍼티
        public int Capacity => _model?.Capacity ?? 0;
        
        //뷰 영역에서 슬롯을 안전하게 다룰 수 있도록 List 대신 ReadOnlyCollection 사용
        public ReadOnlyCollection<ItemSlotViewModel> Slots { get; }

        public InventoryViewModel(Inventory model)
        {
            //모델 및 슬롯 리스트 등록
            _model = model;
            _slots = new List<ItemSlotViewModel>();

            //에디터 환경에서의 오류를 방지하기 위한 예외처리
            if (_model == null)
            {
                Slots = _slots.AsReadOnly();
                return;
            }

            //슬롯을 채워줌
            for (int i = 0; i < Capacity; i++)
            {
                _slots.Add(new ItemSlotViewModel());
            }
            
            //ReadOnlyCollection 타입으로 저장될 수 있도록 AsReadOnly 이용
            Slots = _slots.AsReadOnly();
            
            //변경된 슬롯 업데이트
            _model.OnSlotUpdated += RefreshSingleSlot;

            //전체 슬롯 새로고침
            RefreshAllSlots();
        }

        //전달된 인덱스에 맞는 슬롯을 업데이트 해주는 메소드
        private void RefreshSingleSlot(int index)
        {
            if (index < 0 || index >= _slots.Count || _model == null) return;

            //해당 인덱스의 아이템을 가져옴
            Item item = _model.GetItem(index);
            
            //현재 슬롯에 적용된 석판 레벨 보정값 전달
            _slots[index].SetSlotLevelChange(_model.GetSlotLevelChange(index));

            if (item != null)
            {
                //슬롯 내 아이템 정보 업데이트
                _slots[index].UpdateSlot(item);
            }
            else
            {
                //아이템이 없다면 슬롯 초기화
                _slots[index].ClearSlot();
            }
        }

        //모든 슬롯 새로고침
        private void RefreshAllSlots()
        {
            if (_model == null) return;
            
            //슬롯 전체를 돌면서
            for (int i = 0; i < _slots.Count; i++)
            {
                //인벤토리 범위 안이라면 새로고침, 밖이라면 클리어
                if (i < _model.Capacity) RefreshSingleSlot(i);
                //추후 가방 크기 변경 등을 위해 범위 밖은 클리어 처리
                else _slots[i].ClearSlot();
            }
        }

        //인벤토리 크기 변경 이벤트 호출 시 사용,
        // private void HandleCapacityChanged(int newCapacity) => RefreshAllSlots();
        
        //뷰에서 아이템 스왑할 때 각각 모델의 관련 동작 실행
        public void RequestSwapSlots(int fromIdx, int toIdx) { if (_model != null) _model.Swap(fromIdx, toIdx); }
        
        //뷰에서 석판 회전을 요청
        public void RequestRotateTablet(int index)
        {
            if (_model != null) _model.RotateTablet(index);
        }
        
        //이벤트 해제
        public void UnbindFromModel()
        {
            if (_model == null) return;
            _model.OnSlotUpdated -= RefreshSingleSlot;
            // _model.OnCapacityChanged -= HandleCapacityChanged;
        }
        
        //모든 슬롯 뷰모델에 배경 이미지 전달
        public void SetSlotBackgrounds(Texture2D[] backgrounds, Texture2D tabletBackground)
        {
            foreach (var slot in _slots)
                slot.SetSlotBackgrounds(backgrounds, tabletBackground);
        }
    }
}