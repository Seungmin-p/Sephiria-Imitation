using UnityEngine;
using UnityEngine.UIElements;

namespace MyInventory
{
    //아이템 정보 팝업
    public class InventoryTooltipView : BasePopupView
    {
        private readonly InventoryTooltipViewModel _popupViewModel; //뷰모델

        //아이템 종류별 툴팁
        private readonly VisualElement _artifactTooltip;
        private readonly VisualElement _potionTooltip;
        private readonly VisualElement _tabletTooltip;
        
        //아티팩트 별 표시
        private readonly VisualElement _artifactStarArea;
        private const string StarClass = "artifact-star";
        private const string ActiveStarClass = "artifact-star--active";
        
        //석판 툴팁에 사용될 영역, 템플릿
        private readonly VisualElement _tabletPreviewGrid;
        private readonly VisualTreeAsset _tabletCellTemplate;

        //석판 툴팁에 사용될 크기
        private const float CellSize = 38f;
        private const float CellGap = 4f;
        private const float CellSpacing = CellSize + CellGap;
        
        //석판 효과 안내 영역
        private readonly VisualElement _tabletIncreaseRow;
        private readonly VisualElement _tabletDecreaseRow;
        private readonly VisualElement _tabletRotationDescription;

        public InventoryTooltipView(
            VisualElement tooltipRootElement, 
            VisualTreeAsset tabletCellTemplate, 
            ArtifactSetDisplayData[] artifactSetDisplays, 
            EffectDisplayData[] effectDisplays,
            PotionEffectDisplayData[] potionEffectDisplays) : base(tooltipRootElement)
        {
            //아이템 종류별 툴팁
            _artifactTooltip = RootElement.Q<VisualElement>("ArtifactTooltip");
            _potionTooltip = RootElement.Q<VisualElement>("PotionTooltip");
            _tabletTooltip = RootElement.Q<VisualElement>("TabletTooltip");
            
            //석판 미리보기
            _tabletPreviewGrid = RootElement.Q<VisualElement>("TabletPreviewGrid");
            _tabletCellTemplate = tabletCellTemplate;

            //dataSource 지정
            _popupViewModel = new InventoryTooltipViewModel(artifactSetDisplays, effectDisplays, potionEffectDisplays);
            RootElement.dataSource = _popupViewModel;
            
            //아티팩트 별 영역
            _artifactStarArea = RootElement.Q<VisualElement>("ArtifactStarArea");
            
            //석판 효과 안내 영역
            _tabletIncreaseRow = RootElement.Q<VisualElement>("TabletIncreaseRow");
            _tabletDecreaseRow = RootElement.Q<VisualElement>("TabletDecreaseRow");
            _tabletRotationDescription = RootElement.Q<VisualElement>("TabletRotationDescription");
            
            //툴팁 크기 변경 시 중앙 위치 보정
            //Y축 Translate -50%의 오동작으로 인한 직접 조정
            RootElement.RegisterCallback<GeometryChangedEvent>(OnTooltipGeometryChanged);

            //유니티 버그로 피킹모드 설정이 풀리는 현상을 대비
            RootElement.pickingMode = PickingMode.Ignore;
            foreach (VisualElement child in RootElement.Children())
            {
                child.pickingMode = PickingMode.Ignore;
            }
        }

        //팝업 내용 구성 후 출력
        public void Show(Item item)
        {
            if (item == null) return;

            //아이템 종류별 툴팁 표시
            _artifactTooltip.style.display = item is ArtifactItem ? DisplayStyle.Flex : DisplayStyle.None;
            _potionTooltip.style.display = item is PotionItem ? DisplayStyle.Flex : DisplayStyle.None;
            _tabletTooltip.style.display = item is TabletItem ? DisplayStyle.Flex : DisplayStyle.None;
            
            //데이터를 뷰모델에 전달
            _popupViewModel.Setup(item);
            
            //아티팩트 별 생성
            if (item is ArtifactItem artifact)
                RenderArtifactStars(artifact);

            //석판 미리보기 생성
            if (item is TabletItem tablet)
            {
                RenderTabletPreview(tablet);
                UpdateTabletDescriptionRows(tablet.TabletData);
            }
            else
            {
                _tabletPreviewGrid?.Clear();
            }

            //출력
            base.Show();
        }

        public override void Hide()
        {
            _popupViewModel?.Clear();
            base.Hide();
        }

        //석판 좌표에 따른 미리보기 생성
        private void RenderTabletPreview(TabletItem tablet)
        {
            if (_tabletPreviewGrid == null) return;
            
            TabletItemData data = tablet.TabletData;

            //기존 미리보기 제거
            _tabletPreviewGrid.Clear();

            //메인 석판 (0, 0)을 포함한 전체 좌표 범위
            int minX = 0;
            int maxX = 0;
            int minY = 0;
            int maxY = 0;

            //석판 효과의 최소 ~ 최대 좌표를 구함
            foreach (TabletLevelModifier modifier in data.Modifiers)
            {
                Vector2Int offset = tablet.GetRotatedOffset(modifier.Offset);

                minX = Mathf.Min(minX, offset.x);
                maxX = Mathf.Max(maxX, offset.x);
                minY = Mathf.Min(minY, offset.y);
                maxY = Mathf.Max(maxY, offset.y);
            }

            //전체 미리보기 영역 크기 계산
            int columnCount = maxX - minX + 1;
            int rowCount = maxY - minY + 1;

            _tabletPreviewGrid.style.width = columnCount * CellSize + (columnCount - 1) * CellGap;
            _tabletPreviewGrid.style.height = rowCount * CellSize + (rowCount - 1) * CellGap;

            //메인 석판 셀 생성
            VisualElement mainCell = CreateTabletCell(Vector2Int.zero, minX, maxY);

            //미리보기 아이콘을 석판 본체의 아이콘으로 변경
            if (data.IconSprite != null)
            {
                VisualElement icon = mainCell.Q<VisualElement>("CellIcon");
                icon.style.backgroundImage = new StyleBackground(data.IconSprite);
                icon.style.rotate = new Rotate(Angle.Degrees(tablet.Rotation * 90f));
            }

            //효과 셀 생성
            foreach (TabletLevelModifier modifier in data.Modifiers)
            {
                VisualElement cell = CreateTabletCell(tablet.GetRotatedOffset(modifier.Offset), minX, maxY);

                //혹시 모를 현재 칸의 메인 석판 아이콘 숨기기
                cell.Q<VisualElement>("CellIcon").style.display = DisplayStyle.None;

                //효과 수치 설정
                Label label = cell.Q<Label>("LevelLabel");

                //수치에 따른 표기, 색상 설정
                label.text = (modifier.LevelChange > 0 ? "+" : "") + $"{modifier.LevelChange}";
                label.style.color = modifier.LevelChange > 0 ? Color.green : Color.red;

                label.style.display = DisplayStyle.Flex;
            }
        }
        
        //지정한 좌표에 석판 셀 생성
        private VisualElement CreateTabletCell(Vector2Int offset, int minX, int maxY)
        {
            //셀 템플릿을 복제하여 실제 셀 가져오기
            TemplateContainer container = _tabletCellTemplate.Instantiate();
            VisualElement cell = container.Q<VisualElement>("TabletCell");

            //셀 위치 및 크기 설정
            cell.style.position = Position.Absolute;
            cell.style.left = (offset.x - minX) * CellSpacing;
            cell.style.top = (maxY - offset.y) * CellSpacing;
            cell.style.width = CellSize;
            cell.style.height = CellSize;

            _tabletPreviewGrid.Add(cell);

            return cell;
        }
        
        //석판 효과에 따른 안내 영역 표시
        private void UpdateTabletDescriptionRows(TabletItemData data)
        {
            bool hasIncrease = false;
            bool hasDecrease = false;

            foreach (TabletLevelModifier modifier in data.Modifiers)
            {
                if (modifier.LevelChange > 0) hasIncrease = true;
                if (modifier.LevelChange < 0) hasDecrease = true;
            }

            _tabletIncreaseRow.style.display = hasIncrease ? DisplayStyle.Flex : DisplayStyle.None;
            _tabletDecreaseRow.style.display = hasDecrease ? DisplayStyle.Flex : DisplayStyle.None;
            _tabletRotationDescription.style.display = data.CannotRotate ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        //툴팁 크기 변경 시 중앙 위치 보정
        private void OnTooltipGeometryChanged(GeometryChangedEvent evt)
        {
            float height = evt.newRect.height;
            if (height <= 0f) return;

            RootElement.style.translate = new Translate(
                Length.Percent(-50f),
                new Length(-height * 0.5f, LengthUnit.Pixel)
            );
        }
        
        //아티팩트 레벨에 따른 별 생성
        private void RenderArtifactStars(ArtifactItem artifact)
        {
            if (_artifactStarArea == null) return;

            //기존 별 제거
            _artifactStarArea.Clear();

            //최대 레벨만큼 별 생성
            for (int i = 0; i < artifact.ArtifactData.MaxLevel; i++)
            {
                VisualElement star = new VisualElement { pickingMode = PickingMode.Ignore };

                //기본적으로 StarClass 부여, 레벨 조건에 맞춰서 ActiveStarClass 추가 부여
                star.AddToClassList(StarClass);
                star.EnableInClassList(ActiveStarClass, i < artifact.Level);

                _artifactStarArea.Add(star);
            }
        }
    }
}