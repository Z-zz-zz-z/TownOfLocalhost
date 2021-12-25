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
    public class CustomStatusManager : IEventListener
    {
        static System.Random rand = new System.Random();
        private readonly ILogger<EmptyBottlePlugin> _logger;
        public CustomStatusManager(ILogger<EmptyBottlePlugin> logger)
        {
            _logger = logger;
        }
        [EventListener]
        public void onRoomCreated(IGameCreatedEvent e) {
            CustomStatusHolder.SettingsHolder.Add(e.Game.Code, new CustomGameSettings());
            var status = new CustomGameStatus();
            status.resetStarts();
            CustomStatusHolder.StatusHolder.Add(e.Game.Code.Code, status);
            _logger.LogInformation("ゲームID\"" + e.Game.Code + "\"のCustomSettingsとCustomStatusを作成・登録しました。");
        }
        [EventListener]
        public void resetGameStatus(IGameStartingEvent e) {
            var isSuccess = CustomStatusHolder.StatusHolder.TryGetValue(e.Game.Code, out var status);
            var hasClientModBak = status.hasClientMod;
            if(isSuccess) {
                status.resetStarts();
            }
            status.hasClientMod = hasClientModBak;
            _logger.LogInformation("ゲームID\"" + e.Game.Code + "\"のCustomStatusを初期化しました。");
            foreach(var p in e.Game.Players) {
                _logger.LogInformation("Starting:" + p.Character.PlayerInfo.RoleType.ToString() + "(" + p.Character.PlayerInfo.PlayerName + ")");
            }
            assignRoles.AssignFakeImpostors(e,_logger);
        }

        [EventListener]
        public void BreakSettingsAndStatus(IGameDestroyedEvent e) {
            CustomStatusHolder.SettingsHolder.Remove(e.Game.Code);
            CustomStatusHolder.StatusHolder.Remove(e.Game.Code);
        }
        [EventListener]
        public void CheckClientMods(IPlayerSetStartCounterEvent e) {
            if(e.SecondsLeft == 3) {
                _logger.LogInformation("クライアント用modの確認処理を開始します");
                string[] playerNames = new string[e.Game.PlayerCount];
                bool[] hasClientMod = new bool[e.Game.PlayerCount];
                var verifyText = "検証中...";
                foreach(var p in e.Game.Players) {
                    playerNames[p.Character.PlayerId] = p.Character.PlayerInfo.PlayerName;
                    var writer = e.Game.StartRpc(p.Character.NetId, (RpcCalls)CustomRPC.VerifyMod, p.Client.Id);
                    writer.Write(verifyText);
                    e.Game.FinishRpcAsync(writer);
                }
                Task task = Task.Run(() => {
                    Thread.Sleep(2000);
                    foreach(var p in e.Game.Players) {
                        if(p.Character.PlayerInfo.PlayerName == verifyText)
                            hasClientMod[p.Character.PlayerId] = true;
                        else hasClientMod[p.Character.PlayerId] = false;
                        p.Character.SetNameAsync(playerNames[p.Character.PlayerId]);
                    }
                    var success = CustomStatusHolder.StatusHolder.TryGetValue(e.Game.Code, out var status);
                    if(!success) _logger.LogError("クライアントmodの認証処理に失敗しました:CustomStatusを取得できませんでした");
                    status.hasClientMod = hasClientMod;
                    _logger.LogInformation(string.Join(", ", hasClientMod));
                });
            }
        }
    }
}