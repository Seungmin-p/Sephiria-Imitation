using System;
using UnityEngine;

namespace MyInventory
{
    //포션 효과별 UI 표시 정보
    [Serializable]
    public class PotionEffectDisplayData
    {
        [SerializeField] private PotionEffectType _type; //포션 효과 타입(체력 or 마나 등)
        [SerializeField] private string _displayName; //효과 이름
        [SerializeField] private string _spriteName; //효과 아이콘
        [SerializeField] private Color _textColor = Color.white; //효과 색상
        [SerializeField] private EffectValueType _valueType; //수치 타입

        public PotionEffectType Type => _type;
        public string DisplayName => _displayName;
        public string SpriteName => _spriteName;
        public Color TextColor => _textColor;
        public EffectValueType ValueType => _valueType;
    }
}