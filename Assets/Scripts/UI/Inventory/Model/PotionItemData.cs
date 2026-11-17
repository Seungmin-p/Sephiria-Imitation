using UnityEngine;

namespace MyInventory
{
    //포션 효과 종류
    public enum PotionEffectType
    {
        RestoreHealthPercent,
        RestoreManaPercent
    }

    [CreateAssetMenu(fileName = "Item_Potion_", 
        menuName = "Inventory System/Item Data/Potion", order = 3)]
    public class PotionItemData : ItemData
    {
        [Header("사용 효과 설정")]
        [SerializeField] private PotionEffectType _effectType;
        [SerializeField] private float _value; //효과량 (0.2 = 20%)

        [Header("아이템 설명")]
        //메인 설명과 부가 설명
        [SerializeField, TextArea(3, 5)] private string _effectDescription;
        [SerializeField, TextArea(3, 5)] private string _subDescription;

        public int MaxCount => 5; //포션의 슬롯별 최대 개수 (고정)
        public PotionEffectType EffectType => _effectType;
        public float Value => _value;
        public override string EffectDescription => _effectDescription;
        public override string SubDescription => _subDescription;

        public override Item CreateItem()
        {
            //런타임 상태를 담을 PotionItem 인스턴스를 생성하여 반환
            return new PotionItem(this);
        }
    }
}