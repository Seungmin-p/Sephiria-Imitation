using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyInventory
{
    //인벤토리 Model
    public class Inventory
    {
        //인벤토리 슬롯 배열
        private Item[] _items;
        
        //슬롯별 석판 레벨 보정값
        private readonly int[] _slotLevelChanges;

        //인벤토리 한 줄의 슬롯 개수
        public int ColumnCount { get; }

        //외부 호출용 인벤토리 크기 프로퍼티
        public int Capacity { get; private set; }

        //MVVM 구조를 위한 이벤트
        public event Action<int> OnSlotUpdated; //슬롯 업데이트
        public event Action<int> OnCapacityChanged; //인벤토리 크기 업데이트
        
        //생성자를 이용해서 초기 세팅 진행
        public Inventory(int initialCapacity, int maxCapacity, int columnCount)
        {
            //인벤토리 칸 수가 최대치를 넘을 수 없게 함
            if (initialCapacity > maxCapacity) initialCapacity = maxCapacity;
            
            //최대치에 맞춰서 인벤토리 공간 확보
            _items = new Item[maxCapacity];
            _slotLevelChanges = new int[maxCapacity];
            Capacity = initialCapacity;
            ColumnCount = Mathf.Max(1, columnCount);
        }

        //바인딩 처리 후 호출되는 인벤토리 크기 할당용 초기화 메소드
        public void Initialize()
        {
            //인벤토리 크기 할당을 위한 이벤트 호출
            OnCapacityChanged?.Invoke(Capacity);
        }

        //인벤토리에 아이템을 추가하는 로직
        public void Add(ItemData data)
        {
            if (data == null) return;

            //만약 아이템이 수량이 있는 아이템(포션)이라면
            if (data is PotionItemData)
            {
                //인벤토리 전체 검사
                for (int i = 0; i < Capacity; i++)
                {
                    //각 슬롯별로 포션인지 확인하고, 유형이 같은지, 또한 이미 최대수치인지 확인
                    if (_items[i] is PotionItem potion && potion.Data == data && !potion.IsMax)
                    {
                        //아이템 수량 1개 추가
                        potion.SetCount(potion.Count + 1);
                        UpdateSlot(i);
                        return;
                    }
                }
            }

            //포션을 합칠 슬롯이 없거나 포션이 아닌 경우 빈 슬롯에 추가
            for (int i = 0; i < Capacity; i++)
            {
                //슬롯이 비어있다면
                if (_items[i] == null)
                {
                    //현재 추가하려는 아이템 데이터에 맞춰서 아이템 생성 후 슬롯에 넣어줌
                    _items[i] = data.CreateItem();

                    //새로운 아이템에 따른 석판 효과 재계산
                    RecalculateLevelsAndNotifySlots(i);
                    return;
                }
            }

            //만약 인벤토리 내 빈칸을 못찾은 경우
            Debug.LogWarning("[Inventory] 인벤토리가 가득 찼습니다!");
        }

        //두 아이템의 위치를 바꿔주는 메소드
        public void Swap(int indexA, int indexB)
        {
            //두 인덱스가 문제 없는지 확인
            if (!IsValidIndex(indexA) || !IsValidIndex(indexB) || indexA == indexB ) return;

            Item itemA = _items[indexA];
            Item itemB = _items[indexB];

            //두 아이템이 다 비어있지 않고, 아이템이 서로 똑같고, 포션인지 먼저 체크
            if (itemA != null && itemB != null &&
                itemA.Data == itemB.Data &&
                itemA is PotionItem ciA && itemB is PotionItem ciB)
            {
                int maxCount = ciB.MaxCount;
                int sum = ciA.Count + ciB.Count;
                
                //두 아이템의 수량을 합쳐서 최대수치를 넘는지 체크
                if (sum <= maxCount)
                {
                    //최대 수량보다 적다면 옮기기 시작한 슬롯 내 아이템 개수를 0, 옮긴 슬롯의 아이템 개수를 합한 수량으로 변경
                    ciA.SetCount(0);
                    ciB.SetCount(sum);
                }
                else
                {
                    //최대치를 넘는다면 최대치와, 최대치를 빼고 남은 개수를 각각 지정
                    ciA.SetCount(sum - maxCount);
                    ciB.SetCount(maxCount);
                }
            }
            else
            {
                //새 칸이 비어있거나, 두 아이템이 같지 않거나, 수량아이템이 아니라면 단순하게 두 아이템 교체
                _items[indexA] = itemB;
                _items[indexB] = itemA;
            }
            
            //두 슬롯 업데이트
            RecalculateLevelsAndNotifySlots(indexA, indexB);
        }

        //아이템 사용 메소드 (현재는 포션만 사용 가능)
        public void Use(int index)
        {
            if (!IsValidIndex(index)) return;

            //포션인 경우
            if (_items[index] is PotionItem uItem)
            {
                //아이템 사용, 슬롯 업데이트
                if (uItem.Use()) UpdateSlot(index);
            }
        }

        //인덱스 범위에 문제가 없는지 확인
        private bool IsValidIndex(int index) => index >= 0 && index < Capacity;

        //슬롯 업데이트 이벤트를 호출하는 메소드
        private void UpdateSlot(int index)
        {
            //인덱스에 문제가 없는지 체크
            if (!IsValidIndex(index)) return;
         
            //포션의 수량이 0이 된 경우 null 처리
            if (_items[index] is PotionItem ci && ci.IsEmpty)
            {
                _items[index] = null;
            }

            //슬롯 정보 업데이트 이벤트 호출
            OnSlotUpdated?.Invoke(index);
        }
        
        //인벤토리 내 모든 석판의 레벨 보정을 계산
        private void RecalculateTabletLevels()
        {
            //기존 슬롯 보정값 초기화
            Array.Clear(_slotLevelChanges, 0, _slotLevelChanges.Length);

            //인벤토리 내 모든 석판 확인
            for (int i = 0; i < Capacity; i++)
            {
                if (_items[i] is not TabletItem tablet) continue;

                //석판이 위치한 슬롯의 좌표
                int x = i % ColumnCount;
                int y = i / ColumnCount;

                //석판에 등록된 모든 보정 효과 확인
                foreach (TabletLevelModifier modifier in tablet.TabletData.Modifiers)
                {
                    //현재 석판 회전 상태를 반영한 좌표
                    Vector2Int offset = tablet.GetRotatedOffset(modifier.Offset);

                    //효과가 적용될 슬롯 좌표
                    int targetX = x + offset.x;
                    int targetY = y - offset.y;

                    //좌표가 인벤토리의 범위를 벗어나면 제외
                    //남은 Y축은 IsValidIndex에서 검토됨
                    if (targetX < 0 || targetX >= ColumnCount || targetY < 0)
                        continue;

                    //실제 슬롯 인덱스 계산
                    int targetIndex = targetY * ColumnCount + targetX;

                    //인덱스가 인벤토리 범위를 벗어나면 제외
                    if (!IsValidIndex(targetIndex))
                        continue;

                    //해당 슬롯에 레벨 보정값 합산
                    _slotLevelChanges[targetIndex] += modifier.LevelChange;
                }
            }

            //계산된 슬롯 보정값을 아티팩트에 적용
            for (int i = 0; i < Capacity; i++)
            {
                if (_items[i] is not ArtifactItem artifact) continue;

                //기존 석판 보정 제거
                artifact.ResetLevel();

                //현재 슬롯의 최종 보정값 적용
                artifact.AddLevel(_slotLevelChanges[i]);
            }
        }
        
        //지정한 슬롯의 석판 레벨 보정값 반환
        public int GetSlotLevelChange(int index)
        {
            return IsValidIndex(index) ? _slotLevelChanges[index] : 0;
        }
        
        //석판 레벨 재계산 및 전체 슬롯 변경 이벤트 호출
        private void RecalculateLevelsAndNotifySlots(params int[] changedSlots)
        {
            //재계산 이전 슬롯 보정값 보관
            int[] previousChanges = (int[])_slotLevelChanges.Clone();
            
            //모든 석판의 레벨 보정값 재계산
            RecalculateTabletLevels();

            for (int i = 0; i < Capacity; i++)
            {
                //아이템이 변경된 슬롯인지 확인
                bool itemChanged = Array.IndexOf(changedSlots, i) >= 0;

                //석판 레벨 보정값이 변경된 슬롯인지 확인
                bool levelChanged = previousChanges[i] != _slotLevelChanges[i];

                //아무것도 변경되지 않았다면 갱신하지 않음
                if (!itemChanged && !levelChanged) continue;

                UpdateSlot(i);
            }
        }
        
        //지정한 슬롯의 석판 회전
        public void RotateTablet(int index)
        {
            if (!IsValidIndex(index) || _items[index] is not TabletItem tablet)
                return;

            if (tablet.TabletData.CannotRotate) return;

            //석판 회전
            tablet.RotateClockwise();

            //회전된 효과 범위에 따른 레벨 재계산
            RecalculateLevelsAndNotifySlots(index);
        }
        
        //뷰 모델에서 호출할 데이터 조회용 메소드들
        //각각 해당 인덱스에 아이템이 존재하는지, 아이템이 무엇인지, 수량이 얼마나 되는지 확인
        public bool HasItem(int index) => IsValidIndex(index) && _items[index] != null;
        public Item GetItem(int index) => IsValidIndex(index) ? _items[index] : null;
        
        public int GetCurrentCount(int index)
        {
            if (!IsValidIndex(index)) return -1;
            if (_items[index] == null) return 0;
            if (!(_items[index] is PotionItem ci)) return 1;
            return ci.Count;
        }
    }
}