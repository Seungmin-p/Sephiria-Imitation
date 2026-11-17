using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyInventory
{
    //석판 중심으로부터의 위치와 아티팩트 레벨 보정값
    [Serializable]
    public struct TabletLevelModifier
    {
        [SerializeField] private Vector2Int _offset;
        [SerializeField] private int _levelChange;

        public Vector2Int Offset => _offset;
        public int LevelChange => _levelChange;
    }

    [CreateAssetMenu(fileName = "Item_Tablet_", menuName = "Inventory System/Item Data/Tablet", order = 2)]
    public class TabletItemData : ItemData
    {
        [Header("석판 데이터")]
        [SerializeField] private TabletLevelModifier[] _modifiers = Array.Empty<TabletLevelModifier>();
        [SerializeField] private bool _cannotRotate;

        public IReadOnlyList<TabletLevelModifier> Modifiers => _modifiers ?? Array.Empty<TabletLevelModifier>();
        public bool CannotRotate => _cannotRotate;

        //타입에 맞는 새로운 아이템 생성
        public override Item CreateItem()
        {
            return new TabletItem(this);
        }
    }
}
