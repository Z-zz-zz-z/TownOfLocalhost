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
            if(isSuccess) {
                status.resetStarts();
            }
            _logger.LogInformation("ゲームID\"" + e.Game.Code + "\"のCustomStatusを初期化しました。");
            foreach(var p in e.Game.Players) {
                _logger.LogInformation("Starting:" + p.Character.PlayerInfo.RoleType.ToString() + "(" + p.Character.PlayerInfo.PlayerName + ")");
            }
        }
    }
}