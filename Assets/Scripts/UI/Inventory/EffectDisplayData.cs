using System;
using UnityEngine;

namespace MyInventory
{
    //효과 수치 표시 방식
    public enum EffectValueType
    {
        //고정 수치, 퍼센트 비율(0 ~ 1)
        Flat,
        Percentage
    }

    //효과 종류별 UI 표시 정보
    [Serializable]
    public class EffectDisplayData
    {
        [SerializeField] private ArtifactModifierType _type; //아티팩트의 각종 효과 타입
        [SerializeField] private string _displayName; //효과 명칭
        [SerializeField] private string _spriteName; //효과 아이콘 -> TextCore Sprite Asset 사용
        [SerializeField] private Color _textColor = Color.white; //효과 색상 -> 포션에서만 사용될 예정
        [SerializeField] private EffectValueType _valueType; //수치 타입

        public ArtifactModifierType Type => _type;
        public string DisplayName => _displayName;
        public string SpriteName => _spriteName;
        public Color TextColor => _textColor;
        public EffectValueType ValueType => _valueType;
    }
}