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
    public class CustomGameSettings {
        public int JesterCount = 0;
        public int MadmateCount = 0;
        public int SheriffCount = 0;
        public bool HideAndSeekEnabled = false;
    }
    public class CustomGameStatus {
        public bool isJesterDead = false;
        public Dictionary<byte, customRoles> PlayerRoles = new Dictionary<byte, customRoles>();
        public bool[] hasClientMod = new bool[15];
        public List<byte> SheriffPlayerIDs = new List<byte>();
        public void resetStarts() {
            isJesterDead = false;
            PlayerRoles = new Dictionary<byte, customRoles>();
            hasClientMod = new bool[15];
            SheriffPlayerIDs = new List<byte>();
        }
        public customRoles getRole(byte playerID) {
            var isSuccess = PlayerRoles.TryGetValue(playerID, out var role);
            if(isSuccess) return role;
            else return customRoles.Default;
        }
    }
    public class CustomStatusHolder {
        public static Dictionary<string, CustomGameSettings> SettingsHolder = new Dictionary<string, CustomGameSettings>();
        public static Dictionary<string, CustomGameStatus> StatusHolder = new Dictionary<string, CustomGameStatus>();
    }
    public enum customRoles : byte {
        Default = 0,
        Impostor,
        Jester,
        Madmate,
        Sheriff
    }
    public enum soloWinReason : byte {
        empty = 255,
        Jester = 0,
        Terrorist
    }
    public enum CustomRPC {
        SetCustomRoles = 60,//receve only
        SoloWin,
        VerifyMod,
        SetNotImpostors
    }
}