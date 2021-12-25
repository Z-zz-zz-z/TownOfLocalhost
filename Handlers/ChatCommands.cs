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
    public class ChatCommands : IEventListener
    {
        static System.Random rand = new System.Random();
        private readonly ILogger<EmptyBottlePlugin> _logger;
        public ChatCommands(ILogger<EmptyBottlePlugin> logger)
        {
            _logger = logger;
        }
        [EventListener]
        public void onPlayerSentChat(IPlayerChatEvent e)
        {
            //prefix
            if(e.Message.StartsWith("/") || e.Message.StartsWith("!")){
                string cmd1;
                string cmd2;
                var FirstSpace = e.Message.IndexOf(" ");
                if (FirstSpace == -1){
                    cmd1 = e.Message.Substring(1,e.Message.Length - 1);
                    cmd2 = null;
                } else {
                    cmd1 = e.Message.Substring(1, FirstSpace - 1);
                    cmd2 = e.Message.Substring(FirstSpace + 1);
                }
                var PIDFail = "エラー:数値の引数を正常に変換できませんでした。";
                var PlayerCTRL = e.PlayerControl;
                CustomStatusHolder.SettingsHolder.TryGetValue(e.Game.Code, out var settings);
                //役職設定
                if(cmd1 == "jester") {
                    int cmd2int;
                    if(int.TryParse(cmd2, out cmd2int)) {
                        settings.JesterCount = cmd2int;
                        PlayerCTRL.SendChatToPlayerAsync("Jesterを" + cmd2 + "人に設定しました\r\n" + 
                        "クルーを一人Jesterに置き換えます");
                    } else {
                        PlayerCTRL.SendChatToPlayerAsync(PIDFail);
                    }
                }
                if(cmd1 == "madmate") {
                    int cmd2int;
                    if(int.TryParse(cmd2, out cmd2int)) {
                        settings.MadmateCount = cmd2int;
                        PlayerCTRL.SendChatToPlayerAsync("Madmateを" + cmd2 + "人に設定しました\r\n" + 
                        "エンジニアを一人Madmateに置き換えます");
                    } else {
                        PlayerCTRL.SendChatToPlayerAsync(PIDFail);
                    }
                }
                if(cmd1 == "sheriff") {
                    int cmd2int;
                    if(int.TryParse(cmd2, out cmd2int)) {
                        settings.SheriffCount = cmd2int;
                        PlayerCTRL.SendChatToPlayerAsync("Sheriffを" + cmd2 + "人に設定しました\r\n" + 
                        "無からSheriffを一人割り当てます");
                    } else {
                        PlayerCTRL.SendChatToPlayerAsync(PIDFail);
                    }
                }
                //部屋の設定
                if(cmd1 == "killcool") {
                    float cmd2float;
                    if(float.TryParse(cmd2, out cmd2float)) {
                        e.Game.Options.KillCooldown = cmd2float;
                        PlayerCTRL.SendChatToPlayerAsync("キルクールダウンを" + cmd2 + "に変更しました。");
                        e.Game.SyncSettingsAsync();
                    } else {
                        PlayerCTRL.SendChatToPlayerAsync(PIDFail);
                    }
                }
                if(cmd1 == "commontask") {
                    int cmd2int;
                    if(int.TryParse(cmd2, out cmd2int)) {
                        e.Game.Options.NumCommonTasks = cmd2int;
                        PlayerCTRL.SendChatToPlayerAsync("コモンタスクの量を" + cmd2 + "に変更しました。");
                        e.Game.SyncSettingsAsync();
                    } else {
                        PlayerCTRL.SendChatToPlayerAsync(PIDFail);
                    }
                }
                if(cmd1 == "longtask") {
                    int cmd2int;
                    if(int.TryParse(cmd2, out cmd2int)) {
                        e.Game.Options.NumLongTasks = cmd2int;
                        PlayerCTRL.SendChatToPlayerAsync("ロングタスクの量を" + cmd2 + "に変更しました。");
                        e.Game.SyncSettingsAsync();
                    } else {
                        PlayerCTRL.SendChatToPlayerAsync(PIDFail);
                    }
                }
                if(cmd1 == "shorttask") {
                    int cmd2int;
                    if(int.TryParse(cmd2, out cmd2int)) {
                        e.Game.Options.NumShortTasks = cmd2int;
                        PlayerCTRL.SendChatToPlayerAsync("ショートタスクの量を" + cmd2 + "に変更しました。");
                        e.Game.SyncSettingsAsync();
                    } else {
                        PlayerCTRL.SendChatToPlayerAsync(PIDFail);
                    }
                }
                if(cmd1 == "map") {
                    if(cmd2 == "skeld") {
                        e.Game.Options.Map = MapTypes.Skeld;
                        PlayerCTRL.SendChatToPlayerAsync("mapをSkeldに変更しました。");
                        e.Game.SyncSettingsAsync();
                    } else if(cmd2 == "mirahq") {
                        e.Game.Options.Map = MapTypes.MiraHQ;
                        PlayerCTRL.SendChatToPlayerAsync("mapをMiraHQに変更しました。");
                        e.Game.SyncSettingsAsync();
                    } else if(cmd2 == "polus") {
                        e.Game.Options.Map = MapTypes.Polus;
                        PlayerCTRL.SendChatToPlayerAsync("mapをPolusに変更しました。");
                        e.Game.SyncSettingsAsync();
                    } else if(cmd2 == "airship") {
                        e.Game.Options.Map = MapTypes.Airship;
                        PlayerCTRL.SendChatToPlayerAsync("mapをAirshipに変更しました。");
                        e.Game.SyncSettingsAsync();
                    } else {
                        PlayerCTRL.SendChatToPlayerAsync("エラー:mapの名前が無効です。以下の名前が使えます。\r\nskeld, mirahq, polus, airship");
                    }
                }
                //試合中のコマンド
                if(cmd1 == "endgame") {
                    if(e.ClientPlayer.IsHost) {
                        foreach(var player in e.Game.Players) {
                            if(player.Character.PlayerInfo.IsImpostor) {
                                player.Character.SetNameAsync("ゲームは強制的に終了されました");
                                foreach(var player2 in e.Game.Players) {
                                    if(!player2.Character.PlayerInfo.IsImpostor) {
                                        player.Character.MurderPlayerAsync(player2.Character);
                                    }
                                }
                                player.Character.MurderPlayerAsync(player.Character);
                            }
                        }
                    } else {
                        PlayerCTRL.SendChatToPlayerAsync("エラー:あなたはホストではないため、このコマンドを実行できません。");
                    }
                }
                //プレイヤーのオプション
                if(cmd1 == "rename") {
                    if(cmd2 == null) {
                        e.ClientPlayer.Character.SendChatToPlayerAsync("エラー:名前が指定されていません。");
                    } else {
                        e.ClientPlayer.Character.SetNameAsync(cmd2);
                    }
                }
                if(cmd1 == "verify") {
                    if(cmd2 == null) {
                        e.ClientPlayer.Character.SendChatToPlayerAsync("エラー:名前が指定されていません。");
                    } else {
                        var writer = e.Game.StartRpc(e.PlayerControl.NetId, (RpcCalls)62, e.ClientPlayer.Client.Id);
                        writer.Write(cmd2);
                        e.Game.FinishRpcAsync(writer);
                    }
                }
                if(cmd1 == "tp") {
                    if(int.TryParse(cmd2, out var TargetID)) {
                        foreach(var target in e.Game.Players) {
                            if(target.Character.PlayerId == TargetID) {
                                e.ClientPlayer.Character.NetworkTransform.SnapToAsync(target.Character.NetworkTransform.Position);
                            }
                        }
                    } else {
                        e.ClientPlayer.Character.SendChatToPlayerAsync(PIDFail);
                    }
                }
                if(cmd1 == "idlist") {
                    foreach(var player in e.Game.Players) {
                        e.PlayerControl.SendChatToPlayerAsync(player.Character.PlayerInfo.PlayerName + ":" + player.Character.PlayerId);
                    }
                }
                if(cmd1 == "imp") {
                    if(int.TryParse(cmd2, out var TargetID)) {
                        foreach(var target in e.Game.Players) {
                            if(target.Character.PlayerId == TargetID) {
                                foreach(var p in e.Game.Players) {
                                    if(p.Character.PlayerId == TargetID) {
                                        var writer = e.Game.StartRpc(p.Character.NetId, RpcCalls.SetRole, target.Client.Id);
                                        writer.Write((byte)RoleTypes.Impostor);
                                        e.Game.FinishRpcAsync(writer);
                                    } else {
                                        var writer = e.Game.StartRpc(p.Character.NetId, RpcCalls.SetRole, target.Client.Id);
                                        writer.Write((byte)RoleTypes.Crewmate);
                                        e.Game.FinishRpcAsync(writer);
                                    }
                                }
                            }
                        }
                    } else {
                        e.ClientPlayer.Character.SendChatToPlayerAsync(PIDFail);
                    }
                }
                if(cmd1 == "lobbyoutside") {
                    if(e.Game.GameState == 0) {
                        e.PlayerControl.NetworkTransform.SnapToAsync(new System.Numerics.Vector2(0,5));
                    } else {
                        PlayerCTRL.SendChatToPlayerAsync("エラー:既にゲームは開始されています。\r\nゲームが開始していない状態で変更してください。");
                    }
                }
                if(cmd1 == "lobbyinside") {
                    if(e.Game.GameState == 0) {
                        e.PlayerControl.NetworkTransform.SnapToAsync(new System.Numerics.Vector2(0,0));
                    } else {
                        PlayerCTRL.SendChatToPlayerAsync("エラー:既にゲームは開始されています。\r\nゲームが開始していない状態で変更してください。");
                    }
                }
                //help
                if(cmd1 == "help") {
                    if(cmd2 == null) {
                        e.ClientPlayer.Character.SendChatToPlayerAsync("以下のオプションが使用可能です。\r\nrole, option, user");
                    }
                    if(cmd2 == "role") {
                        e.ClientPlayer.Character.SendChatToPlayerAsync(
                            "/<役職名> <人数>\\r\n" + 
                            "役職名の一覧は以下の通りです\r\n"+
                            "jester, madmate, sheriff"
                            );
                    }
                    if(cmd2 == "option") {
                        e.ClientPlayer.Character.SendChatToPlayerAsync(
                            "/stoppertime, /killcool /madmateknowsimpostor\r\n" + 
                            "/map, /noscantask /hideandseek\r\n" + 
                            "/commontask, /longtask, /shorttask\r\n" + 
                            "/targetmode"
                            );
                    }
                    if(cmd2 == "user") {
                        e.ClientPlayer.Character.SendChatToPlayerAsync(
                            "/tp, /rename, /idlist\r\n" + 
                            "/lobbyoutside, /lobbyinside"
                            );
                    }
                }
            //ログ
            _logger.LogInformation("// Command executed.\r\n" + cmd1 + "\r\n" + cmd2);
            e.IsCancelled = true;
            }
        }
    }
}