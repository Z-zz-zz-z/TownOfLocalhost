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
    public class PlayerTaskManager : IEventListener
    {
        static System.Random rand = new System.Random();
        private readonly ILogger<EmptyBottlePlugin> _logger;
        public PlayerTaskManager(ILogger<EmptyBottlePlugin> logger)
        {
            _logger = logger;
        }
        [EventListener]
        public void onPLayerCompleteTask(IPlayerCompletedTaskEvent e) {
            //残りのタスク数を取得
            var isSuccess = CustomStatusHolder.StatusHolder.TryGetValue(e.Game.Code, out var status);
            if(!isSuccess) {
                _logger.LogError("タスク完了時の処理に失敗しました:CustomStatusを取得できません");
                return;
            }
            int remainingTasks = 0;
            foreach(var p in e.Game.Players) {
                var role = status.getRole(p.Character.PlayerId);
                if(!p.Character.PlayerInfo.IsImpostor &&
                role != customRoles.Jester &&
                role != customRoles.Madmate &&
                role != customRoles.Sheriff) {
                    foreach(var task in p.Character.PlayerInfo.Tasks) {
                        if(!task.Complete) remainingTasks++;
                    }
                }
            }
            //_logger.LogInformation("残りタスク:" + remainingTasks);
            if(remainingTasks <= 0) {
                foreach(var p in e.Game.Players) {foreach(var t in p.Character.PlayerInfo.Tasks) {
                    t.CompleteAsync();
                }}
            }
        }
    }
}