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

                // 1. 수주 가능 퀘스트 처리
                var availableDict = availableQuestDictionary.Value;
                if (availableDict != null && availableQuestIds != null)
                {
                    foreach (int id in availableQuestIds)
                    {
                        if (availableDict.ContainsKey(id))
                        {
                            CommandReference cmdRef = availableDict[id];
                            EnableCommandOption(cmdRef.command);
                        }
                    }
                }

                // 2. 완료 가능 퀘스트 처리
                var finishableDict = finishableQuestDictionary.Value;
                Debug.Log(finishableQuestIds.Length);
                if (finishableDict != null && finishableQuestIds != null)
                {
                    foreach (int id in finishableQuestIds)
                    {
                        Debug.Log(id);
                        if (finishableDict.ContainsKey(id))
                        {
                            CommandReference cmdRef = finishableDict[id];
                            EnableCommandOption(cmdRef.command);
                        }
                    }
                }
            }

            Continue();
        }

        protected virtual void EnableCommandOption(Command cmd)
        {
            if (cmd != null)
            {
                // 1. Menu 타입의 커맨드인 경우 직접 HideThisOption 속성 변경
                if (cmd is Menu menuCommand)
                {
                    menuCommand.HideThisOption = false;
                }
                // 2. 다른 타입의 커맨드인 경우 리플렉션을 통해 HideThisOption 속성 설정 시도
                else
                {
                    var hideProp = cmd.GetType().GetProperty("HideThisOption", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (hideProp != null && hideProp.CanWrite)
                    {
                        hideProp.SetValue(cmd, false);
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
