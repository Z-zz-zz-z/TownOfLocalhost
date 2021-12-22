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
    public class onPlayerDie : IEventListener
    {
        static System.Random rand = new System.Random();
        private readonly ILogger<EmptyBottlePlugin> _logger;
        public onPlayerDie(ILogger<EmptyBottlePlugin> logger)
        {
            _logger = logger;
        }
        [EventListener]
        public void onPlayerExiled(IPlayerExileEvent e) {
            var isSuccess = CustomStatusHolder.StatusHolder.TryGetValue(e.Game.Code, out var status);
            if(!isSuccess) _logger.LogError("プレイヤー追放時の処理に失敗しました:CustomStatusの取得に失敗しました");
            if(status.getRole(e.PlayerControl.PlayerId) == customRoles.Jester && !status.isJesterDead) {
                Task task = Task.Run(() => {
                    Thread.Sleep(12000);
                    statusController.forceSoloWin(e.ClientPlayer, CustomRPC.SoloWin, e.Game, "Jester wins", soloWinReason.Jester);
                });
            }
        }
    }
}