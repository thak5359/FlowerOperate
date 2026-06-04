using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Fungus
{
    [CommandInfo("Narrative", 
                 "DisableQuestMenuOptions", 
                 "수주받은 퀘스트 ID 목록을 확인하여 해당 메뉴 커맨드의 Interactable을 false로 설정합니다.")]
    [AddComponentMenu("")]
    public class DisableQuestMenuOptions : Command
    {
        [Tooltip("Flowchart에서 선언된 Dictionary(Int, Command) 변수 참조")]
        [SerializeField] protected DictionaryIntCommandData menuDictionary;

        [Tooltip("수주받은 퀘스트의 ID 리스트 (int 배열)")]
        [SerializeField] protected int[] questIds = FungusEventBridge.getAvailableQuestId;

        public override void OnEnter()
        {
            var dict = menuDictionary.Value;
            if (dict != null && questIds != null)
            {
                foreach (int id in questIds)
                {
                    if (dict.ContainsKey(id))
                    {
                        CommandReference cmdRef = dict[id];
                        Command cmd = cmdRef.command;
                        if (cmd != null)
                        {
                            // 1. Menu 타입의 커맨드인 경우 직접 Interactable 속성 변경
                            if (cmd is Menu menuCommand)
                            {
                                menuCommand.HideThisOption = false;
                            }
                            // 2. 다른 타입의 커맨드인 경우 리플렉션을 통해 Interactable 속성 설정 시도
                            else
                            {
                                var interactableProp = cmd.GetType().GetProperty("Interactable", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                                if (interactableProp != null && interactableProp.CanWrite)
                                {
                                    interactableProp.SetValue(cmd, false);
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
            int questCount = (questIds != null) ? questIds.Count() : 0;
            return $"Check {questCount} quests in Dictionary ({dictCount} items)";
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255); // Menu 커맨드와 동일한 파란색 계열
        }
    }
}
