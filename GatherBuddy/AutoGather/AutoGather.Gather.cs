using ClickLib.Bases;
using Dalamud.Game.ClientState.Objects.Types;
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
using ECommons.Automation.UIInput;
using ECommons.DalamudServices;
using EventData = ClickLib.Structures.EventData;

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

            var ids       = GatheringAddon->ItemIds.ToArray();
            var itemIndex = GetIndexOfItemToClick(ids, item);
            if (itemIndex < 0)
            {
                itemIndex = GatherBuddy.GameData.Gatherables
                    .Where(item => ids.Contains(item.Key))
                    .Select(item => Array.IndexOf(ids, item.Key)).FirstOrDefault();
            }
            
            var receiveEventAddress = new nint(GatheringAddon->AtkUnitBase.AtkEventListener.VirtualTable->ReceiveEvent);
            var eventDelegate       = Marshal.GetDelegateForFunctionPointer<ClickHelper.ReceiveEventDelegate>(receiveEventAddress);

            
            var target    = AtkStage.Instance();
            var eventData = EventData.ForNormalTarget(target, &GatheringAddon->AtkUnitBase);
            var inputData = InputData.Empty();

            eventDelegate.Invoke(&GatheringAddon->AtkUnitBase.AtkEventListener, EventType.CHANGE, (uint)itemIndex, eventData.Data,
                inputData.Data);
        }

        private int GetIndexOfItemToClick(Span<uint> ids, IGatherable item)
        {
            var gatherable = item as Gatherable;
            for (int i = 0; i < ids.Length; i++)
            {
                Svc.Log.Debug(ids[i].ToString());
            }
            
            if (gatherable == null)
            {
                int result = -1;
                foreach (var pair in GatherBuddy.GameData.Gatherables)
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        if (ids[i] == pair.Key)
                        {
                            result = i;
                            break;
                        }
                    }
                    if (result != -1)
                        break;
                }
                return result;
                
            }

            if (!gatherable.GatheringData.IsHidden
             || (gatherable.GatheringData.IsHidden && (HiddenRevealed || !ShouldUseLuck(ids, gatherable))))
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    if (ids[i] == gatherable.ItemId) 
                    {
                        return i;
                    }
                }
            }
            
            // If no matching item is found, return the index of the first non-hidden item
            for (int i = 0; i < ids.Length; i++)
            {   
                Svc.Log.Debug(item.ItemId.ToString());
                Svc.Log.Debug(ids[i].ToString());
                
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
            return -1;

            ;
        }
    }
}
