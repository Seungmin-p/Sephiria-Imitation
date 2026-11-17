using UnityEngine;

namespace MyInventory
{
    public enum ItemRarity
    {
        Normal,
        Uncommon,
        Rare,
        Legendary
    }

    public abstract class ItemData : ScriptableObject
    {
        [Header("기본 아이템 데이터")]
        [SerializeField] int _id;
        [SerializeField] string _name; //아이템 이름
        [SerializeField, TextArea(3, 5)] string _tooltip; //아이템 설명
        [SerializeField] Sprite _iconSprite; //아이템 아이콘
        [SerializeField] ItemRarity _rarity; //아이템 등급
        
        [Header("아이콘 크기 설정")]
        [SerializeField, Range(0.1f, 1f)] private float _iconScale = 1f;
        
        public int ID => _id;
        public string Name => _name;
        public string Tooltip => _tooltip;
        public Sprite IconSprite => _iconSprite;
        public ItemRarity Rarity => _rarity;
        public float IconScale => _iconScale;
        
        //아이템 종류별 설명
        public virtual string EffectDescription => string.Empty;
        public virtual string SubDescription => string.Empty;

        //타입에 맞는 새로운 아이템 생성
        public abstract Item CreateItem();
    }
}