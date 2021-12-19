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
    public class assignRoles : IEventListener
    {
        static System.Random rand = new System.Random();
        private readonly ILogger<EmptyBottlePlugin> _logger;
        public assignRoles(ILogger<EmptyBottlePlugin> logger)
        {
            _logger = logger;
        }
        [EventListener]
        public void onGameStarted(IGameStartedEvent e) {
            Task task = Task.Run(() => {
                List<Api.Net.IClientPlayer> Crewmates = new List<Api.Net.IClientPlayer>();
                List<Api.Net.IClientPlayer> Scientists = new List<Api.Net.IClientPlayer>();
                List<Api.Net.IClientPlayer> Engineers = new List<Api.Net.IClientPlayer>();
                List<Api.Net.IClientPlayer> Impostors = new List<Api.Net.IClientPlayer>();
                List<Api.Net.IClientPlayer> Shapeshifters = new List<Api.Net.IClientPlayer>();
                Thread.Sleep(5000);
                foreach(var p in e.Game.Players) {
                    _logger.LogInformation("Started:" + p.Character.PlayerInfo.RoleType.ToString()
                     + "(" + /*p.Character.PlayerInfo.PlayerName*/" - " + ")");
                    switch(p.Character.PlayerInfo.RoleType) {
                        case RoleTypes.Crewmate:
                            Crewmates.Add(p);
                            break;
                        case RoleTypes.Impostor:
                            Impostors.Add(p);
                            break;
                        case RoleTypes.Scientist:
                            Scientists.Add(p);
                            break;
                        case RoleTypes.Engineer:
                            Engineers.Add(p);
                            break;
                        case RoleTypes.Shapeshifter:
                            Shapeshifters.Add(p);
                            break;
                        default:
                            break;
                    }
                }
                var succesToGetSettings = CustomStatusHolder.SettingsHolder.TryGetValue(e.Game.Code, out var settings);
                if(!succesToGetSettings) _logger.LogError("ゲーム開始処理に失敗しました:CustomOptionsの取得に失敗しました");
                for(var i = 0; i < settings.JesterCount; i++) setRoleInList(e.Game.Code, Crewmates, customRoles.Jester);
                for(var i = 0; i < settings.MadmateCount; i++) setRoleInList(e.Game.Code, Engineers, customRoles.Madmate);
                //役職通知
                var successToGetStatus = CustomStatusHolder.StatusHolder.TryGetValue(e.Game.Code, out var status);
                if(!succesToGetSettings) _logger.LogError("ゲーム開始処理に失敗しました:CustomStatusの取得に失敗しました");
                foreach(var p in e.Game.Players) {
                    var role = status.getRole(p.Character.PlayerId);
                    noticeRoleByName(p,role,e.Game);
                }
            });
        }
        public void setRoleInList(string code, List<Api.Net.IClientPlayer> list, customRoles role) {
            var isSuccess = CustomStatusHolder.StatusHolder.TryGetValue(code, out var status);
            if(!isSuccess) {
                _logger.LogError("役職の割り当てに失敗しました:CustomStatusの取得に失敗しました");
                return;
            }
            List<Api.Net.IClientPlayer> valid = new List<Api.Net.IClientPlayer>();
            foreach(var p in list) {
                if(status.getRole(p.Character.PlayerId) == customRoles.Default) valid.Add(p);
            }
            if(valid.Count == 0) {
                _logger.LogError("役職の割り当てに失敗しました:割り当て可能なプレイヤーが存在しません");
                return;
            }
            var assignID = rand.Next(valid.Count);
            status.PlayerRoles.Add(valid[assignID].Character.PlayerId, role);
            _logger.LogInformation(role.ToString() + " => " + valid[assignID].Character.PlayerInfo.PlayerName);
        }
        public void noticeRoleByName(Api.Net.IClientPlayer player, customRoles role, Api.Games.IGame Game) {
            string beforeName = player.Character.PlayerInfo.PlayerName;
            string noticeName;
            var doNoticeByChat = true;
            switch(role) {
                case customRoles.Jester:
                    noticeName = "You Are Jester\r\nあなたはてるてるです";
                    break;
                case customRoles.Madmate:
                    noticeName = "You Are Madmate\r\nあなたは狂人です";
                    break;
                case customRoles.Sheriff:
                    noticeName = "You Are Sheriff\r\nあなたはシェリフです";
                    break;
                default:
                    noticeName = "Playing on localhost";
                    doNoticeByChat = false;
                    break;
            }
            if(doNoticeByChat) player.Character.SendChatToPlayerAsync(noticeName);
            player.Character.SetNameAsync("Playing on localhost");
            var writer = Game.StartRpc(player.Character.NetId, RpcCalls.SetName, player.Client.Id);
            writer.Write(noticeName);
            Game.FinishRpcAsync(writer);
            Task task = Task.Run(() => {
                Thread.Sleep(20000);
                player.Character.SetNameAsync(beforeName);
            });
        }
    }
}