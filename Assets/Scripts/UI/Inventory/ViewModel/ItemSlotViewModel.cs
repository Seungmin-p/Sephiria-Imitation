using System;
using Unity.Properties;
using UnityEngine.UIElements;
using UnityEngine;

namespace MyInventory
{
    public class ItemSlotViewModel
    {
        //현재 슬롯의 아이템
        private Item _item;

        //등급별 슬롯 배경 이미지
        private Texture2D[] _slotBackgrounds;
        
        //석판 전용 슬롯 배경 이미지
        private Texture2D _tabletSlotBackground;
        
        //현재 슬롯에 적용된 석판 레벨 보정값
        private int _slotLevelChange;

        //==== 런타임 데이터 바인딩 ====

        //아이템 이미지
        [CreateProperty]
        public StyleBackground IconSprite => _item?.Data.IconSprite != null
            ? new StyleBackground(_item.Data.IconSprite)
            : new StyleBackground(StyleKeyword.None);
        
        //음수 레벨 아티팩트의 아이콘을 어둡게 처리
        [CreateProperty]
        public StyleColor IconTint
        {
            get
            {
                if (_item is ArtifactItem artifact && artifact.Level < 0)
                    return new StyleColor(new Color(0.4f, 0.4f, 0.4f, 1f));

                return new StyleColor(Color.white);
            }
        }

        //아이템 등급별 슬롯 배경
        [CreateProperty]
        public StyleBackground BackgroundSprite
        {
            get
            {
                //석판은 별도의 전용 배경 사용
                if (_item is TabletItem && _tabletSlotBackground != null)
                    return new StyleBackground(_tabletSlotBackground);
                
                //빈 슬롯은 0번, 일반 등급부터 1~4번 이미지 사용, 포션은 0번
                int index = _item == null || _item is PotionItem ? 0 : (int)_item.Data.Rarity + 1;

                if (_slotBackgrounds == null || index >= _slotBackgrounds.Length || _slotBackgrounds[index] == null)
                    return new StyleBackground(StyleKeyword.None);

                return new StyleBackground(_slotBackgrounds[index]);
            }
        }

        //아티팩트 레벨
        [CreateProperty]
        public string LevelText => _item is ArtifactItem artifact
            ? $"{artifact.Level}/{artifact.ArtifactData.MaxLevel}"
            : "";

        //아티팩트 레벨 표시 여부
        [CreateProperty]
        public StyleEnum<DisplayStyle> LevelVisible => new(
            _item is ArtifactItem ? DisplayStyle.Flex : DisplayStyle.None
        );

        //아티팩트 레벨 색상
        [CreateProperty]
        public StyleColor LevelColor
        {
            get
            {
                if (_item is not ArtifactItem artifact)
                    return new StyleColor(Color.white);

                if (artifact.Level < 0)
                    return new StyleColor(Color.red);

                if (artifact.Level > artifact.ArtifactData.MaxLevel)
                    return new StyleColor(Color.yellow);

                if (artifact.Level == artifact.ArtifactData.MaxLevel)
                    return new StyleColor(Color.green);

                return new StyleColor(Color.white);
            }
        }
        
        //슬롯에 적용된 석판 레벨 보정값
        [CreateProperty]
        public string SlotLevelChangeText => _slotLevelChange > 0
            ? $"+{_slotLevelChange}"
            : _slotLevelChange.ToString();

        //석판 보정값 표시 여부
        [CreateProperty]
        public StyleEnum<DisplayStyle> SlotLevelChangeVisible => new(
            (_item == null || _item is PotionItem) && _slotLevelChange != 0
                ? DisplayStyle.Flex
                : DisplayStyle.None
        );

        //빈 슬롯의 석판 보정값 색상
        [CreateProperty]
        public StyleColor SlotLevelChangeColor => new(
            _slotLevelChange < 0 ? Color.red : Color.white
        );
        
        //석판 회전 상태에 따른 아이콘 회전
        [CreateProperty]
        public StyleRotate IconRotation => new(new Rotate(Angle.Degrees(
            _item is TabletItem tablet ? tablet.Rotation * 90f : 0f
        )));

        //포션 수량
        [CreateProperty]
        public string CountText => _item is PotionItem potion
            ? $"{potion.Count}/{potion.MaxCount}"
            : "";

        //포션 수량 표시 여부
        [CreateProperty]
        public StyleEnum<DisplayStyle> CountVisible => new(
            _item is PotionItem ? DisplayStyle.Flex : DisplayStyle.None
        );

        //슬롯 데이터가 변경될 때 마다 호출될 이벤트
        public event Action OnStateChanged;

        //뷰 영역에서 각종 판단을 위해 사용하는 프로퍼티
        public bool HasItem => _item != null;
        public ItemData ItemData => _item?.Data;
        public Item Item => _item;

        //등급별 슬롯 배경 이미지 연결
        public void SetSlotBackgrounds(Texture2D[] backgrounds, Texture2D tabletBackground)
        {
            _slotBackgrounds = backgrounds;
            _tabletSlotBackground = tabletBackground;
            OnStateChanged?.Invoke();
        }

        //인벤토리 뷰 모델에서 호출하는 슬롯 업데이트 메소드
        public void UpdateSlot(Item item)
        {
            _item = item;
            OnStateChanged?.Invoke();
        }

        //인벤토리 뷰 모델에서 호출하는 슬롯 클리어 메소드
        public void ClearSlot()
        {
            if (_item == null) return;

            _item = null;
            OnStateChanged?.Invoke();
        }
        
        //슬롯에 적용된 석판 레벨 보정값 변경
        public void SetSlotLevelChange(int levelChange)
        {
            if (_slotLevelChange == levelChange) return;

            _slotLevelChange = levelChange;
            OnStateChanged?.Invoke();
        }
    }
}