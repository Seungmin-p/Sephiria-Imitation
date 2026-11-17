using UnityEngine;

namespace MyInventory
{
    //수량 아이템 - 포션 아이템 (Model 계층)
    public class PotionItem : Item
    {
        //포션 데이터
        public PotionItemData PotionData => (PotionItemData)Data;

        //현재 아이템 개수
        public int Count { get; private set; }

        //이 아이템이 하나의 슬롯에 들어갈 수 있는 최대 개수
        public int MaxCount => PotionData.MaxCount;

        //아이템이 최대치까지 차있는지에 대한 여부
        public bool IsMax => Count >= MaxCount;

        //아이템의 개수가 없는지 체크
        public bool IsEmpty => Count <= 0;

        public PotionItem(PotionItemData data, int count = 1) : base(data)
        {
            SetCount(count);
        }

        //아이템이 슬롯별 최대 개수를 넘은 경우, 최대 개수로 보정
        public void SetCount(int count)
        {
            Count = Mathf.Clamp(count, 0, MaxCount);
        }

        public bool Use()
        {
            if (IsEmpty) return false;

            //SetCount를 이용해서 수치 -1
            SetCount(Count - 1);

            //실제 게임이라면 여기에 포션 사용 효과 추가
            return true;
        }
    }
}