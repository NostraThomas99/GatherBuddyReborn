using ClickLib.Bases;
using ClickLib.Enums;
using ClickLib.Structures;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Automation.UIInput;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GatherBuddy.Classes;
using GatherBuddy.Interfaces;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GatherBuddy.AutoGather
{
    public partial class AutoGather
    {
        private unsafe void InteractWithNode()
        {
            if (!CanAct)
                return;

            var targetSystem = TargetSystem.Instance();
            if (targetSystem == null)
                return;

            TaskManager.DelayNext(1000);
            TaskManager.Enqueue(() =>
            {
                targetSystem->OpenObjectInteraction((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)NearestNode.Address);
            });
        }

        private unsafe void DoGatherWindowTasks(IGatherable item)
        {
            if (GatheringAddon == null)
                return;

            List<uint> ids = new List<uint>(GatheringAddon->ItemIds.ToArray());
            var itemIndex = GetIndexOfItemToClick(ids, item);
            if (itemIndex < 0)
                itemIndex = GatherBuddy.GameData.Gatherables.Where(item => ids.Contains(item.Key)).Select(item => ids.IndexOf(item.Key))
                    .FirstOrDefault();

            var target = AtkStage.Instance();
            var eventData = ECommons.Automation.UIInput.EventData.ForNormalTarget(target, &GatheringAddon->AtkUnitBase);
            var inputData = ECommons.Automation.UIInput.InputData.Empty();

            TaskManager.Enqueue(() => !Svc.Condition[ConditionFlag.Gathering42]);
            TaskManager.Enqueue(() => ClickHelper.InvokeReceiveEvent(&GatheringAddon->AtkUnitBase.AtkEventListener, ECommons.Automation.UIInput.EventType.CHANGE, (uint)itemIndex, eventData, inputData));
        }

        private int GetIndexOfItemToClick(List<uint> ids, IGatherable item)
        {
            var gatherable = item as Gatherable;
            if (gatherable == null)
            {
                return GatherBuddy.GameData.Gatherables.Where(item => ids.Contains(item.Key)).Select(item => ids.IndexOf(item.Key))
                    .FirstOrDefault();

                ;
            }

            if (!gatherable.GatheringData.IsHidden
             || (gatherable.GatheringData.IsHidden && (HiddenRevealed || !ShouldUseLuck(ids, gatherable))))
            {
                return ids.FindIndex(i => i == gatherable.ItemId);
            }

            // If no matching item is found, return the index of the first non-hidden item
            for (int i = 0; i < ids.Count; i++)
            {
                var id = ids[i];
                gatherable = GatherBuddy.GameData.Gatherables.FirstOrDefault(it => it.Key == id).Value;
                if (gatherable == null)
                {
                    continue;
                }

                if (!gatherable.GatheringData.IsHidden || (gatherable.GatheringData.IsHidden && HiddenRevealed))
                {
                    return i;
                }
            }

            // If all items are hidden or none found, return -1
            return GatherBuddy.GameData.Gatherables.Where(item => ids.Contains(item.Key)).Select(item => ids.IndexOf(item.Key))
                .FirstOrDefault();

            ;
        }
    }
}
