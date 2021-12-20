using System;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Net.Messages;
using Impostor.Api.Net.Messages.C2S;
using Impostor.Api.Net.Messages.S2C;
using Impostor.Api.Net.Messages.Rpcs;
using Impostor.Api.Net.Inner;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Impostor.Api.Innersloth;
using Impostor.Api.Innersloth.Customization;
using System.IO;
using System.Text;
using System.Collections.Immutable;
using ExtraPlayerFunctions.Desync;

namespace Impostor.Plugins.EBPlugin.Handlers
{
    public class statusController
    {
        static System.Random rand = new System.Random();
        public static void forceSoloWin(Api.Net.IClientPlayer player, customRPC customRPC, Api.Games.IGame Game, string winMessage = null, soloWinReason reason = soloWinReason.empty) {
            //全員へのRPC
            foreach(var p in Game.Players) {
                var writer = Game.StartRpc(p.Character.NetId, (RpcCalls)CustomRPC.SoloWin, p.Client.Id);
                writer.Write(player.Character.PlayerId);
                writer.Write((byte)reason);
                Game.FinishRpcAsync(writer);
            }

            Api.Net.IClientPlayer imp = null;
            Api.Net.IClientPlayer deadImp = null;
            List<Api.Net.IClientPlayer> ImpToRemove = new List<Api.Net.IClientPlayer>();
            List<Api.Net.IClientPlayer> PlayerToKill = new List<Api.Net.IClientPlayer>();   
            foreach(var p in Game.Players) {
                if(p.Character.PlayerInfo.IsImpostor) {
                    if(p.Character.PlayerInfo.IsDead) {
                        if(deadImp == null) deadImp = p;
                        else ImpToRemove.Add(p);
                    } else {
                        if(imp == null) imp = p;
                        else ImpToRemove.Add(p);
                    }
                } else {
                    PlayerToKill.Add(p);
                }
            }
            if(imp == null) imp = deadImp;
            else ImpToRemove.Add(deadImp);
            foreach(var p in ImpToRemove) {
                SetGuardianAngelAsync(p, Game);
            }
            var info = player.Character.PlayerInfo.CurrentOutfit;
            imp.Character.SetColorAsync(info.Color);
            imp.Character.SetHatAsync(info.HatId);
            imp.Character.SetPetAsync(info.PetId);
            imp.Character.SetSkinAsync(info.SkinId);
            var name = info.PlayerName;
            if(winMessage != null) name += "\r\n" + winMessage;
            imp.Character.SetNameAsync(name);
            foreach(var p in PlayerToKill) {
                imp.Character.MurderPlayerAsync(p.Character);
            }
            foreach(var p in ImpToRemove) {
                imp.Character.MurderPlayerAsync(p.Character);
            }
        }
        public static void SetGuardianAngelAsync(Api.Net.IClientPlayer player, Api.Games.IGame Game) {
            if(player == null) return;
            foreach(var every in Game.Players) {
                var writer = Game.StartRpc(player.Character.NetId, RpcCalls.SetRole, every.Client.Id);
                writer.Write((UInt16)RoleTypes.GuardianAngel);
                Game.FinishRpcAsync(writer);
            }
        }
        public static void SetImpostorAsync(Api.Net.IClientPlayer player, Api.Games.IGame Game) {
            if(player == null) return;
            foreach(var every in Game.Players) {
                var writer = Game.StartRpc(player.Character.NetId, RpcCalls.SetRole, every.Client.Id);
                writer.Write((UInt16)RoleTypes.Shapeshifter);
                Game.FinishRpcAsync(writer);
            }
        }
    }
}