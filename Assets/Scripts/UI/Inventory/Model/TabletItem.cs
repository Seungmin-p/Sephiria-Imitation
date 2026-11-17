using UnityEngine;

namespace MyInventory
{
    public class TabletItem : Item
    {
        //석판 데이터
        public TabletItemData TabletData => (TabletItemData)Data;

        //현재 회전 상태 (90도 단위, 0~3)
        public int Rotation { get; private set; }

        public TabletItem(TabletItemData data) : base(data) { }

        //시계 방향으로 90도 회전
        public void RotateClockwise()
        {
            if (TabletData.CannotRotate) return;

            Rotation = (Rotation + 1) % 4;
        }
        
        //현재 회전 상태에 따른 석판 효과 좌표 반환
        public Vector2Int GetRotatedOffset(Vector2Int offset)
        {
            return Rotation switch
            {
                1 => new Vector2Int(offset.y, -offset.x),
                2 => new Vector2Int(-offset.x, -offset.y),
                3 => new Vector2Int(-offset.y, offset.x),
                _ => offset
            };
        }
    }
}
