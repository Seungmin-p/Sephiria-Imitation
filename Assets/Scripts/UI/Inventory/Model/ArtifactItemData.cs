using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyInventory
{
    //세트 효과
    public enum ArtifactSetType
    {
        None,
        Ember,
        Sturdy,
        Shadow,
        Lake
    }

    //스탯, 공격 범위 등 각종 보정 데이터
    public enum ArtifactModifierType
    {
        MaxHealth,
        MaxMana,
        ManaRegen,
        PhysicalAttackPower,
        FireAttackPower,
        IceAttackPower,
        ElectricAttackPower,
        Defense,
        Evasion,
        CriticalChance,
        CriticalDamage,
        AttackSpeedMultiplier,
        MoveSpeedMultiplier,
        AttackRange,
        DashDistance
    }

    //보정 속성과 그 값을 의미하는 구조체
    [Serializable]
    public struct ArtifactModifierValue
    {
        [SerializeField] private ArtifactModifierType _type;
        [SerializeField] private float _value;

        public ArtifactModifierType Type => _type;
        public float Value => _value;
    }

    //레벨별 보정 수치 데이터
    [Serializable]
    public class ArtifactLevelData
    {
        [SerializeField] private ArtifactModifierValue[] _modifiers = Array.Empty<ArtifactModifierValue>();

        public IReadOnlyList<ArtifactModifierValue> Modifiers => _modifiers ?? Array.Empty<ArtifactModifierValue>();
    }

    [CreateAssetMenu(fileName = "Item_Artifact_", menuName = "Inventory System/Item Data/Artifact", order = 1)]
    public class ArtifactItemData : ItemData
    {
        [Header("아티팩트 데이터")]
        [SerializeField] private ArtifactSetType _setType;
        [SerializeField] private ArtifactLevelData[] _levels = { new ArtifactLevelData() };

        [Header("아이템 설명")]
        //메인 설명과 부가 설명
        [SerializeField, TextArea(3, 5)] private string _effectDescription;
        [SerializeField, TextArea(3, 5)] private string _subDescription;

        public int MaxLevel => Mathf.Max(0, (_levels?.Length ?? 0) - 1);
        public ArtifactSetType SetType => _setType;
        public override string EffectDescription => _effectDescription;
        public override string SubDescription => _subDescription;

        //레벨별 보정값 가져오기 (Lv.0 = 배열 첫 번째 항목)
        public IReadOnlyList<ArtifactModifierValue> GetModifiers(int level)
        {
            if (_levels == null || level < 0 || level >= _levels.Length)
                return Array.Empty<ArtifactModifierValue>();

            return _levels[level]?.Modifiers ?? Array.Empty<ArtifactModifierValue>();
        }

        //최소 1개의 데이터가 존재할 수 있도록 유지
        private void OnValidate()
        {
            if (_levels == null || _levels.Length == 0)
                _levels = new[] { new ArtifactLevelData() };
        }

        //타입에 맞는 새로운 아이템 생성
        public override Item CreateItem()
        {
            return new ArtifactItem(this);
        }
    }
}
