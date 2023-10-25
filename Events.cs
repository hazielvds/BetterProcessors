using BrokeProtocol.API;
using BrokeProtocol.Collections;
using BrokeProtocol.Entities;
using BrokeProtocol.Required;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using BrokeProtocol.Utility;

namespace BetterProcessors
{
    public class PluginCustomEvents : IScript
    {
        [CustomTarget]
        public bool TradeAction(ShEntity entity, ShPlayer caller)
        {
            string[] args = entity.data.Split(' ');

            List<LabelID> LabelIDs = new List<LabelID>();

            if(args.Length < 2)
            {
                caller.svPlayer.SendGameMessage("This npc don't have trades");
                return true;
            }
            Trader trader = Core.Instance.FileHandler.TraderList.FirstOrDefault(x => x.Id == args[1]);
            foreach(Trade trade in trader.Trades) LabelIDs.Add(new LabelID(trade.Display, $"{trader.Id}.{trade.Id}"));
            caller.svPlayer.SendOptionMenu(trader.Name, caller.ID, "trader", LabelIDs.ToArray(), new LabelID[] { new LabelID("Make", "make"), new LabelID("Get Recipe", "recipe") });
            
            return true;
        }
        [CustomTarget]
        public void OnEnterTradeAction(Serialized trigger, ShPhysical physical)
        {
            if(physical is ShPlayer player)
            {
                ShEntity entity = (ShEntity)trigger;
                string[] args = entity.data.Split(' ');

                List<LabelID> LabelIDs = new List<LabelID>();

                if (args.Length < 2)
                {
                    player.svPlayer.SendGameMessage("This npc don't have trades");
                    return;
                }
                Trader trader = Core.Instance.FileHandler.TraderList.FirstOrDefault(x => x.Id == args[1]);
                foreach (Trade trade in trader.Trades) LabelIDs.Add(new LabelID(trade.Display, $"{trader.Id}.{trade.Id}"));
                player.svPlayer.SendOptionMenu(trader.Name, player.ID, "trader", LabelIDs.ToArray(), new LabelID[] { new LabelID("Make", "make"), new LabelID("Get Recipe", "recipe") });

            }
        }
    }
    public class PluginManagerEvents : ManagerEvents
    {
        [Execution(ExecutionMode.PostEvent)]
        public override bool Start()
        {
            Debug.LogFormat("Starting to register traders");

            List<Trader> RegTrader = Core.Instance.FileHandler.TraderList;
            if (RegTrader is null) { Debug.Log("There is no traders to register."); return true; };
            var Traders = EntityCollections.Entities.Where(x => !string.IsNullOrEmpty(x.data) ).Where(x => x.data.ToLower().StartsWith("trader"));

            if (Traders is null) { Debug.Log("Processors is null"); return true; };

            Debug.Log(RegTrader);

            foreach(ShEntity trader in Traders)
            {
                if (trader is null) continue;
                try
                {
                    Debug.LogFormat($"Registering trader on entity: {trader.name} at possition: {trader.GetPosition}");
                    trader.svEntity.SvAddDynamicAction("TradeAction", "Trade");
                    Debug.LogFormat("Dinamic sv action added to entity...");
                    Debug.LogFormat("Processor registered successfully.");
                } catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }

            return true;
        }
    }

    public class PluginPlayerEvents : PlayerEvents
    {
        [Execution(ExecutionMode.Event)]
        public override bool Ready(ShPlayer player)
        {
            player.svPlayer.CustomData["isCrafting"] = false;
            return true;
        }
        [Execution(ExecutionMode.Event)]
        public override bool OptionAction(ShPlayer player, int targetID, string id, string optionID, string actionID)
        {
            if(id == "trader")
            {
                string[] args = optionID.Split('.');
                Trader trader = Core.Instance.FileHandler.TraderList.FirstOrDefault(x => x.Id == args[0]);
                Trade trade = trader.Trades.FirstOrDefault(x => x.Id == args[1]);


                switch (actionID)
                {
                    case "make":
                        if(!string.IsNullOrEmpty(trader.Permission) && !player.svPlayer.HasPermission(trader.Permission))
                        {
                            player.svPlayer.SendGameMessage("&4No tienes permiso para utilizar este mercader");
                            break;
                        }
                        if(trader.Jobs.Length > 0 && !trader.Jobs.Contains(player.svPlayer.job.info.shared.jobName))
                        {
                            player.svPlayer.SendGameMessage("&4No tienes el trabajo necesario para hablar con este mercader.");
                            break;
                        }
                        player.svPlayer.CustomData.TryFetchCustomData("isCrafting", out bool isCrafting);
                        if (isCrafting) break;
                        if(hasItems(player, trade.Input))
                        {
                            player.svPlayer.StartCoroutine(TradeCoroutine(trade, player));
                        } else
                        {
                            player.svPlayer.SendGameMessage($"You don't have the required items to craft that recipe");
                        }
                        break;

                    case "recipe":
                        string recipeText = "";

                        foreach(var item in trade.Input)
                        {
                            recipeText += $"&ax{item.Value} &6{item.Key}";
                        }

                        player.svPlayer.SendGameMessage(recipeText);
                        break;
                }
            }
            return true;
        }
        private bool hasItems(ShPlayer player, Dictionary<string, int> items)
        {
            int hasCount = 0;
            foreach(var item in items)
            {
                ShItem itemObj = Core.Instance.EntityHandler.Items.FirstOrDefault(x => x.Key == item.Key).Value;
                if (player.HasItem(itemObj) && player.MyItemCount(itemObj) >= item.Value) hasCount++;
            }
            return hasCount == items.Count;
        }

        IEnumerator TradeCoroutine(Trade trade, ShPlayer player)
        {
            player.svPlayer.CustomData["isCrafting"] = true;
            player.svPlayer.SvProgressBar(0f, 1f / trade.Time, "processProgress");
            yield return new WaitForSeconds(trade.Time);
            if (player.IsDead) yield return false;
            foreach(var item in trade.Input)
            {
                ShItem i = Core.Instance.EntityHandler.Items.FirstOrDefault(x => x.Key == item.Key).Value;
                if(i != null)
                {
                    player.TransferItem(DeltaInv.RemoveFromMe, i.index, item.Value, true);
                }
            }
            foreach (var item in trade.Output)
            {
                ShItem i = Core.Instance.EntityHandler.Items.FirstOrDefault(x => x.Key == item.Key).Value;
                if(i != null)
                {
                    player.TransferItem(DeltaInv.AddToMe, i.index, item.Value, true);
                }
            }
            player.svPlayer.SvProgressStop("processProgress");
            player.svPlayer.CustomData["isCrafting"] = false;
            yield return true;
        }
        public override bool Death(ShDestroyable destroyable, ShPlayer attacker)
        {
            if(destroyable is ShPlayer player)
            {
                player.svPlayer.SvProgressStop("processProgress");
            }
            return true;
        }
    }
    
}
