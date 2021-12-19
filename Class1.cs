using System;
using System.Threading.Tasks;
using Impostor.Api.Plugins;
using Impostor.Api.Events;
using Impostor.Api.Events.Managers;
using Impostor.Plugins.EBPlugin.Handlers;
using Microsoft.Extensions.Logging;

namespace Impostor.Plugins.EBPlugin
{
    [ImpostorPlugin(
        id: "local.EmptyBottle.au",
        name: "EBPlugin",
        author: "tukasa_01",
        version: "0.0.1")]
    public class EmptyBottlePlugin : PluginBase
    {
        private readonly ILogger<EmptyBottlePlugin> _logger;
        private readonly IEventManager _eventManager;
        private IDisposable _uregChatCommands;
        private IDisposable _uregCustomStatusManager;
        private IDisposable _uregAssignRoles;
        private IDisposable _uregOnPlayerDie;
        public EmptyBottlePlugin(ILogger<EmptyBottlePlugin> logger, IEventManager eventManager)
        {
            _logger = logger;
            _eventManager = eventManager;
        }
        public override ValueTask EnableAsync()
        {
            _logger.LogInformation("EmptyBottlePlugin is being enabled.");
            _uregChatCommands = _eventManager.RegisterListener(new ChatCommands(_logger));
            _uregCustomStatusManager = _eventManager.RegisterListener(new CustomStatusManager(_logger));
            _uregAssignRoles = _eventManager.RegisterListener(new assignRoles(_logger));
            _uregOnPlayerDie = _eventManager.RegisterListener(new onPlayerDie(_logger));
            return default;
        }
        public override ValueTask DisableAsync()
        {
            _logger.LogInformation("EmptyBottlePlugin is being disabled.");
            _uregChatCommands.Dispose();
            _uregCustomStatusManager.Dispose();
            _uregAssignRoles.Dispose();
            _uregOnPlayerDie.Dispose();
            return default;
        }
    }
    public enum customRPC {
        jesterWin = 0
    }
}