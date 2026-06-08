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
        [Tooltip("Flowchart에서 선언된 Dictionary(Int, Command) 변수 참조")]
        [SerializeField] protected DictionaryIntCommandData menuDictionary;

        public override void OnEnter()
        {
            Flowchart flowchart = GetFlowchart();
            if (flowchart != null)
            {
                var dict = menuDictionary.Value;
                if (dict != null)
                {
                    int[] availableQuestIds = FungusEventBridge.getAvailableQuestId;
                    int[] finishableQuestIds = FungusEventBridge.getFinishableQuestId;

                    // 중복 조회를 방지하고 처리 대상을 합치기 위해 HashSet 사용
                    HashSet<int> questIdsToEnable = new HashSet<int>();

                    if (availableQuestIds != null)
                    {
                        foreach (int id in availableQuestIds)
                        {
                            questIdsToEnable.Add(id);
                        }
                    }

                    if (finishableQuestIds != null)
                    {
                        foreach (int id in finishableQuestIds)
                        {
                            questIdsToEnable.Add(id);
                        }
                    }

                    foreach (int id in questIdsToEnable)
                    {
                        if (dict.ContainsKey(id))
                        {
                            CommandReference cmdRef = dict[id];
                            Command cmd = cmdRef.command;
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
                    }
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            int dictCount = (menuDictionary.Value != null) ? menuDictionary.Value.Count : 0;

            int[] availableQuestIds = FungusEventBridge.getAvailableQuestId;
            int[] finishableQuestIds = FungusEventBridge.getFinishableQuestId;
            int availableCount = availableQuestIds != null ? availableQuestIds.Length : 0;
            int finishableCount = finishableQuestIds != null ? finishableQuestIds.Length : 0;

            return $"Enable options from available ({availableCount}) & finishable ({finishableCount}) in '{menuDictionary.GetDescription()}' ({dictCount} items)";
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255); // Menu 커맨드와 동일한 파란색 계열
        }
    }
}
