using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MyInventory
{
    //아이템 설명의 토큰을 실제 표시 문자열로 변환
    public sealed class EffectDescriptionFormatter
    {
        private static readonly Regex TokenRegex = new(@"\{([^{}]+)\}");
        private readonly Dictionary<ArtifactModifierType, EffectDisplayData> _effectDisplays = new();
        private readonly Dictionary<PotionEffectType, PotionEffectDisplayData> _potionEffectDisplays = new();
        
        public EffectDescriptionFormatter(EffectDisplayData[] effectDisplays, PotionEffectDisplayData[] potionEffectDisplays)
        {
            //아티팩트 효과 표시 정보 등록
            if (effectDisplays != null)
            {
                foreach (EffectDisplayData display in effectDisplays)
                {
                    if (display == null) continue;

                    if (_effectDisplays.ContainsKey(display.Type))
                    {
                        Debug.LogError($"[EffectDescriptionFormatter] 중복 등록된 효과 타입: {display.Type}");
                        continue;
                    }

                    _effectDisplays.Add(display.Type, display);
                }
            }

            //포션 효과 표시 정보 등록
            if (potionEffectDisplays != null)
            {
                foreach (PotionEffectDisplayData display in potionEffectDisplays)
                {
                    if (display == null) continue;

                    if (_potionEffectDisplays.ContainsKey(display.Type))
                    {
                        Debug.LogError($"[EffectDescriptionFormatter] 중복 등록된 포션 효과 타입: {display.Type}");
                        continue;
                    }

                    _potionEffectDisplays.Add(display.Type, display);
                }
            }
        }

        //아이템 설명을 실제 표시 문자열로 변환
        public string Format(Item item)
        {
            if (item == null) return string.Empty;

            string description = item.Data.EffectDescription;

            if (string.IsNullOrEmpty(description)) return string.Empty;

            //아티팩트 토큰 변환
            if (item is ArtifactItem artifact)
                return TokenRegex.Replace(description, match => ResolveToken(artifact, match));

            //포션 토큰 변환
            if (item is PotionItem potion)
                return TokenRegex.Replace(description, match => ResolvePotionToken(potion, match));

            return description;
        }

        //토큰 종류에 따라 변환 처리
        private string ResolveToken(ArtifactItem artifact, Match match)
        {
            //{} 내부 텍스트만을 따로 가져옴
            string token = match.Groups[1].Value;
            
            //현재 상태에 맞는 표시용 효과 데이터 가져오기
            IReadOnlyList<ArtifactModifierValue> displayModifiers = GetDisplayModifiers(artifact);

            //효과가 하나일 때는 값의 타입을 생략할 수 있음
            if (token == "Value")
            {
                //만약 아티팩트에 효과가 여러개면 오류 출력
                if (displayModifiers.Count != 1)
                    return LogTokenError(artifact, match.Value, "Value에 대응하는 효과를 특정할 수 없습니다.");

                //효과가 하나 뿐이라면 해당하는 효과의 값 전달
                return FormatValue(artifact, displayModifiers[0], match.Value);
            }

            //Value: 를 이용한 명시적 값 타입 반환
            if (token.StartsWith("Value:", StringComparison.Ordinal))
            {
                //Value:의 뒷 부분을 가져옴
                string typeName = token.Substring("Value:".Length);

                //뒷 부분의 텍스트가 게임 내 존재하는 효과의 타입인지 확인
                if (!TryGetModifierType(typeName, out ArtifactModifierType type))
                    return LogTokenError(artifact, match.Value, "존재하지 않는 효과 타입입니다.");

                //뒷 부분의 텍스트가 아티팩트에도 등록된 효과인지 확인
                if (!TryGetModifier(artifact, type, out ArtifactModifierValue modifier, out string error))
                    return LogTokenError(artifact, match.Value, error);

                //문제 없다면 해당하는 효과의 값 전달
                return FormatValue(artifact, modifier, match.Value);
            }

            //==== 일반 효과 토큰 처리 ====
            
            //게임 내 존재하는 효과의 타입인지 확인
            if (!TryGetModifierType(token, out ArtifactModifierType modifierType))
                return LogTokenError(artifact, match.Value, "등록되지 않은 토큰입니다.");

            //아티팩트에 등록된 효과인지 확인
            if (!TryGetModifier(artifact, modifierType, out _, out string displayError))
                return LogTokenError(artifact, match.Value, displayError);

            //해당 타입을 표기할 이름과 아이콘이 게임에 등록되어 있는지 확인
            if (!_effectDisplays.TryGetValue(modifierType, out EffectDisplayData displayData))
                return LogTokenError(artifact, match.Value, "효과 표시 정보가 없습니다.");

            //문제 없다면 변환처리 진행
            return FormatDisplay(displayData);
        }

        //문자열을 있는 그대로 ArtifactModifierType으로 변환
        private static bool TryGetModifierType(string name, out ArtifactModifierType type)
        {
            return Enum.TryParse(name, out type) && Enum.IsDefined(typeof(ArtifactModifierType), type);
        }

        //현재 레벨에서 해당 효과의 실제 수치 검색
        private static bool TryGetModifier(ArtifactItem artifact, ArtifactModifierType type, out ArtifactModifierValue modifier, out string error)
        {
            modifier = default;
            error = string.Empty;
            bool found = false;

            //특정 효과와 그 효과의 값을 묶어놓은 구조체를 이용해서 아티팩트에 등록되어 있는 상태인지 확인
            foreach (ArtifactModifierValue value in GetDisplayModifiers(artifact))
            {
                //전달받은 type과 동일한 타입이 존재하는지 확인
                if (value.Type != type) continue;

                //아티팩트의 현재 레벨에 같은 효과가 여러개 존재하는 경우
                if (found)
                {
                    error = $"Lv.{artifact.Level}에 {type} 효과가 중복 등록되어 있습니다.";
                    return false;
                }

                //타입을 찾았으면 modifier를 통해 전달 진행
                modifier = value;
                found = true;
            }

            if (!found)
                error = $"Lv.{artifact.Level}에 {type} 효과가 존재하지 않습니다.";
            
            return found;
        }
        
        //포션 토큰 종류에 따라 변환 처리
        private string ResolvePotionToken(PotionItem potion, Match match)
        {
            //{} 내부 텍스트만 가져옴
            string token = match.Groups[1].Value;

            //포션은 효과가 하나이므로 Value만 사용
            if (token == "Value")
                return FormatPotionValue(potion, match.Value);

            //게임 내 존재하는 포션 효과 타입인지 확인
            if (!TryGetPotionEffectType(token, out PotionEffectType type))
                return LogTokenError(potion, match.Value, "등록되지 않은 포션 토큰입니다.");

            //실제 포션에 등록된 효과인지 확인
            if (potion.PotionData.EffectType != type)
                return LogTokenError(potion, match.Value, "포션에 해당 효과가 존재하지 않습니다.");

            //효과 표시 정보가 등록되어 있는지 확인
            if (!_potionEffectDisplays.TryGetValue(type, out PotionEffectDisplayData displayData))
                return LogTokenError(potion, match.Value, "포션 효과 표시 정보가 없습니다.");

            //문제 없다면 변환처리 진행
            return FormatDisplay(displayData);
        }
        
        //포션의 실제 효과 수치를 표시 문자열로 변환
        private string FormatPotionValue(PotionItem potion, string originalToken)
        {
            //현재 포션의 실제 효과 타입 가져오기
            PotionEffectType type = potion.PotionData.EffectType;

            //효과 표시 정보가 등록되어 있는지 확인
            if (!_potionEffectDisplays.TryGetValue(type, out PotionEffectDisplayData display))
                return LogTokenError(potion, originalToken, "포션 효과 표시 정보가 없습니다.");

            //실제 포션 효과 수치 가져오기
            float value = potion.PotionData.Value;

            //값의 타입이 퍼센트면 * 100 적용
            if (display.ValueType == EffectValueType.Percentage)
                value *= 100f;

            //포션에는 + 기호와 색상을 적용하지 않음
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
        
        //문자열을 있는 그대로 PotionEffectType으로 변환
        private static bool TryGetPotionEffectType(string name, out PotionEffectType type)
        {
            return Enum.TryParse(name, out type) && Enum.IsDefined(typeof(PotionEffectType), type);
        }
        
        //아티팩트 표시 정보 변환
        private static string FormatDisplay(EffectDisplayData display)
        {
            return FormatDisplay(display.DisplayName, display.SpriteName, display.TextColor);
        }

        //포션 표시 정보 변환
        private static string FormatDisplay(PotionEffectDisplayData display)
        {
            return FormatDisplay(display.DisplayName, display.SpriteName, display.TextColor);
        }

        //효과 이름과 스프라이트를 Rich Text로 변환
        private static string FormatDisplay(string displayName, string spriteName, Color textColor)
        {
            //가장 먼저 이름이 존재하는지 확인
            string result = displayName ?? string.Empty;

            //아이콘 이름이 존재하는지 확인해서 효과 이름뒤에 sprite 태그 형식으로 붙여줌
            if (!string.IsNullOrEmpty(spriteName))
                result += $"<sprite name=\"{spriteName}\">";

            //기본 색상이 아니라면 색상 적용(설정 상 포션에서만 사용될 예정)
            if (textColor != Color.white)
                result = $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{result}</color>";

            return result;
        }

        //효과의 수치를 Rich Text로 변환
        private string FormatValue(ArtifactItem artifact, ArtifactModifierValue modifier, string originalToken)
        {
            //변환할 보정값의 타입이, 게임 내 효과 표시 정보에 존재하는지 확인
            if (!_effectDisplays.TryGetValue(modifier.Type, out EffectDisplayData display))
                return LogTokenError(artifact, originalToken, "효과 표시 정보가 없습니다.");

            //값의 타입이 퍼센트면 * 100, 아니라면 있는 그대로 저장
            float value = display.ValueType == EffectValueType.Percentage
                ? modifier.Value * 100f
                : modifier.Value;

            //양수 및 0은 +N, 음수는 -N 형태로 텍스트 확보
            string text = value.ToString("+0.##;-0.##;+0", CultureInfo.InvariantCulture);
            
            //값에 따른 색상 적용
            string color = value < 0f ? "FF0000" : "00FF00";

            //색상을 적용한 최종 텍스트 반환
            return $"<color=#{color}>{text}</color>";
        }

        //잘못된 토큰에 대한 로그 출력
        private static string LogTokenError(Item item, string token, string reason)
        {
            //아이템 이름 : 효과명 - 오류 원인
            Debug.LogError($"[EffectDescriptionFormatter] {item.Data.Name}: {token} - {reason}");
            return token;
        }
        
        //툴팁에 표시할 아티팩트 효과 데이터 반환
        private static IReadOnlyList<ArtifactModifierValue> GetDisplayModifiers(ArtifactItem artifact)
        {
            return artifact.IsActive
                ? artifact.Modifiers
                : artifact.ArtifactData.GetModifiers(0);
        }
    }
}