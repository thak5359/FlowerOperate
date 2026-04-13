using UnityEngine;
using System.Collections.Generic;
using Fungus;

namespace Fungus
{
    // [기존 MultiCharacterData 구조 유지]
    [System.Serializable]
    public class MultiCharacterData
    {
        public Character character;
        public bool useLayeredFace;
        public string facePortraitName;
    }

    [CommandInfo("Narrative",
                 "Say Multi",
                 "최대 3명의 캐릭터 표정을 동시에 변경하며 대사를 출력합니다.")]
    public class SayMulti : Say
    {
        [SerializeField] protected List<MultiCharacterData> multiCharacters = new List<MultiCharacterData>(3);
        [SerializeField] protected string customCharacterName;

        // [추가] 이름 텍스트 색상 설정
        [SerializeField] protected Color nameColor = Color.white;

        public override void OnEnter()
        {
            // 1. 모든 리스트 순회하며 표정 변경 적용
            foreach (var data in multiCharacters)
            {
                if (data.character != null && data.useLayeredFace)
                {
                    LayeredCharacter layeredChar = data.character as LayeredCharacter;
                    if (layeredChar != null)
                    {
                        layeredChar.ApplyFace(data.facePortraitName);
                    }
                }
            }

            // 2. 기본 Say 로직 실행 (이 로직이 대화창을 활성화합니다)
            base.OnEnter();

            // 3. [수정] 대화창 이름 및 색상 강제 설정
            var sayDialog = SayDialog.GetSayDialog();
            if (sayDialog != null)
            {
                string displayName = string.IsNullOrEmpty(customCharacterName) ? GetCombinedNames() : customCharacterName;
                // 이름과 함께 우리가 설정한 nameColor를 전달합니다.
                sayDialog.SetCharacterName(displayName, nameColor);
            }
        }

        // 캐릭터 리스트에서 이름을 합쳐오는 도우미 함수
        protected string GetCombinedNames()
        {
            List<string> nameList = new List<string>();
            foreach (var d in multiCharacters)
            {
                if (d.character != null) nameList.Add(d.character.NameText);
            }
            return string.Join(" & ", nameList);
        }

        public override string GetSummary()
        {
            string nameLabel = string.IsNullOrEmpty(customCharacterName) ? GetCombinedNames() : customCharacterName;
            return $"{nameLabel}: {storyText}";
        }
    }
}