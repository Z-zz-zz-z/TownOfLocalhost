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
using Impostor.Api.Innersloth.Maps;
using System.Numerics;

namespace Impostor.Plugins.EBPlugin.Handlers
{
    public class VentManager : IEventListener
    {
        static System.Random rand = new System.Random();
        private readonly ILogger<EmptyBottlePlugin> _logger;
        public VentManager(ILogger<EmptyBottlePlugin> logger)
        {
            _logger = logger;
        }
        [EventListener]
        public void onPlayerEnterVent(IPlayerEnterVentEvent e) {
            var isSuccess = CustomStatusHolder.StatusHolder.TryGetValue(e.Game.Code, out var status);
            if(!isSuccess) {
                _logger.LogInformation("エラー:CustomStatusの取得に失敗しました");
                return;
            }
            status.LastVentedPos[e.PlayerControl.PlayerId] = e.PlayerControl.NetworkTransform.Position;
            _logger.LogInformation(status.LastVentedPos[e.PlayerControl.PlayerId].X + ", " + status.LastVentedPos[e.PlayerControl.PlayerId].Y);
            if(status.getRole(e.PlayerControl.PlayerId) == customRoles.Sheriff) {
                e.IsCancelled = true;
            }
        }
        [EventListener]
        public void onPlayerExitVent(IPlayerExitVentEvent e) {
            var isSuccess = CustomStatusHolder.StatusHolder.TryGetValue(e.Game.Code, out var status);
            if(!isSuccess) {
                _logger.LogInformation("エラー:CustomStatusの取得に失敗しました");
                return;
            }
            status.LastVentedPos[e.PlayerControl.PlayerId] = e.PlayerControl.NetworkTransform.Position;
            _logger.LogInformation(status.LastVentedPos[e.PlayerControl.PlayerId].X + ", " + status.LastVentedPos[e.PlayerControl.PlayerId].Y);
            if(status.getRole(e.PlayerControl.PlayerId) == customRoles.Sheriff) {
                e.IsCancelled = true;
            }
        }
        [EventListener]
        public void CancelSheriffVentMove(IPlayerVentEvent e) {
            var isSuccess = CustomStatusHolder.StatusHolder.TryGetValue(e.Game.Code, out var status);
            if(!isSuccess) {
                _logger.LogInformation("エラー:CustomStatusの取得に失敗しました");
                return;
            }
            if(status.getRole(e.PlayerControl.PlayerId) == customRoles.Sheriff) {
                _logger.LogInformation("ベント移動をキャンセル\r\n" + status.LastVentedPos[e.PlayerControl.PlayerId].X + ", " + status.LastVentedPos[e.PlayerControl.PlayerId].Y);
                Task task = Task.Run(() => {
                    Thread.Sleep(0);
                    e.PlayerControl.NetworkTransform.SnapToAsync(status.LastVentedPos[e.PlayerControl.PlayerId]);
                });
                return;
            }
            status.LastVentedPos[e.PlayerControl.PlayerId] = e.NewVent.Position;
            _logger.LogInformation(status.LastVentedPos[e.PlayerControl.PlayerId].X + ", " + status.LastVentedPos[e.PlayerControl.PlayerId].Y);
        }
    }
}