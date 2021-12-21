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
                Thread.Sleep(100);
                var successToGetStatus = CustomStatusHolder.StatusHolder.TryGetValue(e.Game.Code, out var status);
                if(!successToGetStatus) _logger.LogError("ゲーム開始処理に失敗しました:CustomStatusの取得に失敗しました");
                foreach(var p in e.Game.Players) {
                    _logger.LogInformation("Started:" + p.Character.PlayerInfo.RoleType.ToString()
                     + "(" + /*p.Character.PlayerInfo.PlayerName*/" - " + ")");
                    switch(p.Character.PlayerInfo.RoleType) {
                        case RoleTypes.Crewmate:
                            Crewmates.Add(p);
                            status.PlayerRoles.Add(p.Character.PlayerId, customRoles.Default);
                            break;
                        case RoleTypes.Impostor:
                            Impostors.Add(p);
                            //status.PlayerRoles.Add(p.Character.PlayerId, customRoles.Impostor);DEBUG
                            status.PlayerRoles.Add(p.Character.PlayerId, customRoles.Default);
                            break;
                        case RoleTypes.Scientist:
                            Scientists.Add(p);
                            status.PlayerRoles.Add(p.Character.PlayerId, customRoles.Default);
                            break;
                        case RoleTypes.Engineer:
                            Engineers.Add(p);
                            status.PlayerRoles.Add(p.Character.PlayerId, customRoles.Default);
                            break;
                        case RoleTypes.Shapeshifter:
                            Shapeshifters.Add(p);
                            status.PlayerRoles.Add(p.Character.PlayerId, customRoles.Impostor);
                            break;
                        default:
                            _logger.LogWarning("警告:役職不明のプレイヤー(" + p.Character.PlayerInfo.PlayerName + ")");
                            status.PlayerRoles.Add(p.Character.PlayerId, customRoles.Default);
                            break;
                    }
                }
                _logger.LogInformation("役職の取得とリスト化に成功");
                var succesToGetSettings = CustomStatusHolder.SettingsHolder.TryGetValue(e.Game.Code, out var settings);
                if(!succesToGetSettings) _logger.LogError("ゲーム開始処理に失敗しました:CustomOptionsの取得に失敗しました");

                var HostHasClientMods = status.hasClientMod[0];

                for(var i = 0; i < settings.JesterCount; i++) setRoleInList(e.Game.Code, Crewmates, customRoles.Jester);
                for(var i = 0; i < settings.MadmateCount; i++) setRoleInList(e.Game.Code, Engineers, customRoles.Madmate);

                if(HostHasClientMods) {
                     for(var i = 0; i < settings.SheriffCount; i++) setRoleInList(e.Game.Code, Impostors, customRoles.Sheriff);
                } else _logger.LogWarning("警告:ホストのクライアントmodを確認できませんでした。一部の役職が利用不可になります");
                
                successToGetStatus = CustomStatusHolder.StatusHolder.TryGetValue(e.Game.Code, out status);
                if(!successToGetStatus) _logger.LogError("ゲーム開始処理に失敗しました:CustomStatusの取得に失敗しました");

                //GuardianAngelの設定
                foreach(var p in e.Game.Players) {
                    var role = status.getRole(p.Character.PlayerId);
                    if(role == customRoles.Impostor) foreach(var p2 in e.Game.Players) {
                        var role2 = status.getRole(p2.Character.PlayerId);
                        if(role2 != customRoles.Impostor) {
                            var writer = e.Game.StartRpc(p2.Character.NetId, RpcCalls.SetRole, p.Client.Id);
                            writer.Write((byte)RoleTypes.GuardianAngel);
                            e.Game.SendToAsync(writer, p.Client.Id);
                            e.Game.FinishRpcAsync(writer);
                        }
                    }
                    if(role == customRoles.Sheriff) foreach(var p3 in e.Game.Players) {
                        var role2 = status.getRole(p3.Character.PlayerId);
                        var writer = e.Game.StartRpc(p3.Character.NetId, RpcCalls.SetRole, p.Client.Id);
                        if(p.Character.PlayerId != p3.Character.PlayerId) {
                            writer.Write((byte)RoleTypes.GuardianAngel);
                            e.Game.SendToAsync(writer, p.Client.Id);
                            e.Game.FinishRpcAsync(writer);
                        }
                    }
                }
                _logger.LogInformation("SetGuardianAngelの処理が完了しました");

                //役職通知
                _logger.LogInformation("追加役職のデータをbyte配列に変換します");
                byte[] AllRoles = new byte[15];
                foreach(var p in e.Game.Players) {
                    AllRoles.SetValue((byte)status.getRole(p.Character.PlayerId), p.Character.PlayerId);
                    //AllRoles[p.Character.PlayerId] = (byte)status.getRole(p.Character.PlayerId);
                }
                _logger.LogInformation("追加役職のデータをbyte配列に変換しました");
                foreach(var p in e.Game.Players) {
                    //_logger.LogInformation("役職通知繰り返し処理");
                    var writer = e.Game.StartRpc(p.Character.NetId, (RpcCalls)CustomRPC.SetCustomRoles, p.Client.Id);
                    writer.WriteBytesAndSize(AllRoles);
                    e.Game.FinishRpcAsync(writer);
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
            _logger.LogInformation("CustomStatusを取得しました");
            List<Api.Net.IClientPlayer> valid = new List<Api.Net.IClientPlayer>();
            foreach(var p in list) {
                if(status.getRole(p.Character.PlayerId) == customRoles.Default ||
                status.getRole(p.Character.PlayerId) == customRoles.Impostor) valid.Add(p);
            }
            if(valid.Count == 0) {
                _logger.LogError("役職の割り当てに失敗しました:割り当て可能なプレイヤーが存在しません");
                return;
            }
            _logger.LogInformation("割り当て可能なプレイヤーのリストを作成しました");
            var assignID = rand.Next(valid.Count);
            _logger.LogInformation("乱数を取得しました");
            status.PlayerRoles[valid[assignID].Character.PlayerId] = role;
            _logger.LogInformation(role.ToString() + " => " + /*valid[assignID].Character.PlayerInfo.PlayerName*/" - ");
        }
        public void noticeRoleByName(Api.Net.IClientPlayer player, customRoles role, Api.Games.IGame Game) {
            Task task = Task.Run(() => {
                Thread.Sleep(3000);
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
                Thread.Sleep(20000);
                player.Character.SetNameAsync(beforeName);
            });
        }
    }
}