using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyInventory
{
    public class ArtifactItem : Item
    {
        //아티팩트 데이터
        public ArtifactItemData ArtifactData => (ArtifactItemData)Data;
        
        //석판 보정까지 포함된 현재 레벨
        public int Level { get; private set; }
        
        //실제 효과를 조회할 레벨
        public int EffectiveLevel => Level < 0 ? -1 : Mathf.Min(Level, ArtifactData.MaxLevel);

        //음수 레벨이면 비활성화
        public bool IsActive => Level >= 0;
        
        //레벨에 따른 보정값
        public IReadOnlyList<ArtifactModifierValue> Modifiers => IsActive
            ? ArtifactData.GetModifiers(EffectiveLevel)
            : Array.Empty<ArtifactModifierValue>();

        //레벨 정하기(기본 0레벨)
        public ArtifactItem(ArtifactItemData data) : base(data) { }

        //석판 보정을 다시 계산하기 전에 레벨 초기화
        public void ResetLevel()
        {
            Level = 0;
        }

        //석판에 따른 레벨 보정
        public void AddLevel(int amount)
        {
            Level += amount;
        }
    }
}
