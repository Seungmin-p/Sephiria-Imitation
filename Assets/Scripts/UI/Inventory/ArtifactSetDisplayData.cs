using System;
using UnityEngine;

namespace MyInventory
{
    //아티팩트 세트별 UI 표시 정보
    [Serializable]
    public class ArtifactSetDisplayData
    {
        [SerializeField] private ArtifactSetType _setType;
        [SerializeField] private string _name;
        [SerializeField] private Sprite _icon;

        public ArtifactSetType SetType => _setType;
        public string Name => _name;
        public Sprite Icon => _icon;
    }
}