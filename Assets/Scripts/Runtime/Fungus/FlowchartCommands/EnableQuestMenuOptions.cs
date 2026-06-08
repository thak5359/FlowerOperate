using UnityEngine;
using System.Collections.Generic;

namespace Fungus
{
    [CommandInfo("Narrative", 
                 "EnableQuestMenuOptions", 
                 "수주 가능/완료 가능 퀘스트 ID 목록을 확인하여 해당 메뉴 커맨드의 Hide This Option 설정을 false로 만들어 줍니다.")]
    [AddComponentMenu("")]
    public class EnableQuestMenuOptions : Command
    {
        [Tooltip("수주 가능 퀘스트 Dictionary(Int, Command) 변수 참조")]
        [SerializeField] protected DictionaryIntCommandData availableQuestDictionary;

        [Tooltip("완료 가능 퀘스트 Dictionary(Int, Command) 변수 참조")]
        [SerializeField] protected DictionaryIntCommandData finishableQuestDictionary;

        public override void OnEnter()
        {
            Flowchart flowchart = GetFlowchart();
            if (flowchart != null)
            {
                int[] availableQuestIds = FungusEventBridge.getAvailableQuestId;
                int[] finishableQuestIds = FungusEventBridge.getFinishableQuestId;

                // 1. 모든 관련 커맨드 수집 및 노출할 커맨드 분류를 위한 해시셋 준비
                HashSet<Command> allCommands = new HashSet<Command>();
                HashSet<Command> showCommands = new HashSet<Command>();

                // 수주 가능 퀘스트 딕셔너리 수집
                var availableDict = availableQuestDictionary.Value;
                if (availableDict != null)
                {
                    foreach (var kvp in availableDict)
                    {
                        if (kvp.Value.command != null)
                        {
                            allCommands.Add(kvp.Value.command);
                        }
                    }

                    if (availableQuestIds != null)
                    {
                        foreach (int id in availableQuestIds)
                        {
                            if (availableDict.ContainsKey(id) && availableDict[id].command != null)
                            {
                                showCommands.Add(availableDict[id].command);
                            }
                        }
                    }
                }

                // 완료 가능 퀘스트 딕셔너리 수집
                var finishableDict = finishableQuestDictionary.Value;
                if (finishableDict != null)
                {
                    foreach (var kvp in finishableDict)
                    {
                        if (kvp.Value.command != null)
                        {
                            allCommands.Add(kvp.Value.command);
                        }
                    }

                    if (finishableQuestIds != null)
                    {
                        foreach (int id in finishableQuestIds)
                        {
                            if (finishableDict.ContainsKey(id) && finishableDict[id].command != null)
                            {
                                showCommands.Add(finishableDict[id].command);
                            }
                        }
                    }
                }

                // 2. 최종 상태 적용 (노출 대상 목록에 포함되어 있다면 보이고, 그렇지 않다면 숨김)
                foreach (Command cmd in allCommands)
                {
                    bool isVisible = showCommands.Contains(cmd);
                    SetCommandOptionVisibility(cmd, isVisible);
                }
            }

            Continue();
        }

        protected virtual void SetCommandOptionVisibility(Command cmd, bool visible)
        {
            if (cmd != null)
            {
                bool hide = !visible;
                // 1. Menu 타입의 커맨드인 경우 직접 HideThisOption 속성 변경
                if (cmd is Menu menuCommand)
                {
                    menuCommand.HideThisOption = hide;
                }
                // 2. 다른 타입의 커맨드인 경우 리플렉션을 통해 HideThisOption 속성 설정 시도
                else
                {
                    var hideProp = cmd.GetType().GetProperty("HideThisOption", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (hideProp != null && hideProp.CanWrite)
                    {
                        hideProp.SetValue(cmd, hide);
                    }
                }
            }
        }

        public override string GetSummary()
        {
            int availableDictCount = (availableQuestDictionary.Value != null) ? availableQuestDictionary.Value.Count : 0;
            int finishableDictCount = (finishableQuestDictionary.Value != null) ? finishableQuestDictionary.Value.Count : 0;

            int[] availableQuestIds = FungusEventBridge.getAvailableQuestId;
            int[] finishableQuestIds = FungusEventBridge.getFinishableQuestId;
            int availableCount = availableQuestIds != null ? availableQuestIds.Length : 0;
            int finishableCount = finishableQuestIds != null ? finishableQuestIds.Length : 0;

            return $"Available: {availableCount}/{availableDictCount} ({availableQuestDictionary.GetDescription()}), Finishable: {finishableCount}/{finishableDictCount} ({finishableQuestDictionary.GetDescription()})";
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255); // Menu 커맨드와 동일한 파란색 계열
        }
    }
}
