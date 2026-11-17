using UnityEngine;
using Unity.Properties;
using UnityEngine.UIElements;

namespace MyInventory
{
    public class InventoryTooltipViewModel
    {
        private Item _itemModel; //주입받은 아이템 모델
        private readonly ArtifactSetDisplayData[] _artifactSetDisplays; //아티팩트 세트 효과 표기 정보
        private readonly EffectDescriptionFormatter _effectFormatter; //아이템 개별 효과 표기 정보
        private string _effectDescriptionText = string.Empty; //아이템 개별 효과 설명용 텍스트
        
        //아티팩트 세트 표시, 효과 정보 설정
        public InventoryTooltipViewModel(ArtifactSetDisplayData[] artifactSetDisplays, EffectDisplayData[] effectDisplays, PotionEffectDisplayData[] potionEffectDisplays)
        {
            _artifactSetDisplays = artifactSetDisplays;
            _effectFormatter = new EffectDescriptionFormatter(effectDisplays, potionEffectDisplays);
        }

        //팝업 상단 아이템 이름, 아이콘 바인딩
        [CreateProperty] public string NameText => _itemModel?.Data.Name ?? string.Empty;
        [CreateProperty] public StyleBackground IconSprite => _itemModel?.Data.IconSprite != null
            ? new StyleBackground(_itemModel.Data.IconSprite)
            : new StyleBackground(StyleKeyword.None);
        
        //석판 회전 상태에 따른 툴팁 아이콘 회전
        [CreateProperty]
        public StyleRotate IconRotation => new(new Rotate(Angle.Degrees(
            _itemModel is TabletItem tablet ? tablet.Rotation * 90f : 0f
        )));
        
        //아티팩트 세트 이름
        [CreateProperty] public string ArtifactSetNameText => GetArtifactSetDisplay()?.Name ?? string.Empty;
        
        //아티팩트 세트 아이콘
        [CreateProperty]
        public StyleBackground ArtifactSetIconSprite
        {
            get
            {
                Sprite icon = GetArtifactSetDisplay()?.Icon;

                return icon != null
                    ? new StyleBackground(icon)
                    : new StyleBackground(StyleKeyword.None);
            }
        }
        
        //아이템 효과 설명
        [CreateProperty] public string EffectDescriptionText => _effectDescriptionText;
        
        //아이템 부가 설명
        [CreateProperty] public string SubDescriptionText => _itemModel?.Data.SubDescription ?? string.Empty;
        
        //아티팩트 등급
        [CreateProperty]
        public string ArtifactRarityText
        {
            get
            {
                if (_itemModel is not ArtifactItem artifact) return string.Empty;

                return artifact.Data.Rarity switch
                {
                    ItemRarity.Normal => "일반 아티팩트",
                    ItemRarity.Uncommon => "고급 아티팩트",
                    ItemRarity.Rare => "희귀 아티팩트",
                    ItemRarity.Legendary => "전설 아티팩트",
                    _ => string.Empty
                };
            }
        }
        
        //음수 레벨 아티팩트의 제약 표시 여부
        [CreateProperty]
        public StyleEnum<DisplayStyle> RestrictionVisible => new(
            _itemModel is ArtifactItem { IsActive: false }
                ? DisplayStyle.Flex
                : DisplayStyle.None
        );
        
        //음수 레벨 아티팩트의 효과 설명 투명도
        [CreateProperty]
        public StyleFloat ArtifactEffectOpacity => new(
            _itemModel is ArtifactItem { IsActive: false } ? 0.15f : 1f
        );
        
        //현재 아티팩트에 해당하는 세트 표시 정보
        private ArtifactSetDisplayData GetArtifactSetDisplay()
        {
            if (_itemModel is not ArtifactItem artifact || _artifactSetDisplays == null)
                return null;

            ArtifactSetType setType = artifact.ArtifactData.SetType;

            if (setType == ArtifactSetType.None) return null;

            foreach (ArtifactSetDisplayData setData in _artifactSetDisplays)
            {
                if (setData != null && setData.SetType == setType)
                    return setData;
            }

            return null;
        }

        //팝업이 열릴 때 데이터를 세팅하는 메소드
        public void Setup(Item item)
        {
            _itemModel = item;
            
            //아이템 표기 정보 데이터에 아이템을 전달하면서, 깔끔하게 변환된 아이템의 표기 정보를 가져옴
            _effectDescriptionText = _effectFormatter.Format(item);
        }

        public void Clear()
        {
            _itemModel = null;
            _effectDescriptionText = string.Empty;
        }
    }
}