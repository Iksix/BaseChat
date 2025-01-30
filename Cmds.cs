
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using IksAdminApi;
using Microsoft.Extensions.Localization;

namespace BaseChat;

public static class Cmds
{
    private static IIksAdminApi _api = Main.Api;
    private static IStringLocalizer _localizer = Main.sLocalizer;
    public static Dictionary<CCSPlayerController, CCSPlayerController?> LastPrivateMessage = new Dictionary<CCSPlayerController, CCSPlayerController?>(); // Target -> Sender

    public static void Psay(CCSPlayerController? caller, List<string> args, CommandInfo info)
    {
        var name = caller?.PlayerName ?? "CONSOLE";
        var msg = string.Join(" ", args.Skip(1));

        _api.DoActionWithIdentity(caller, args[0], (target, _) => {
            if (target == null || target.IsBot) return;
            LastPrivateMessage[target] = caller!;
            target.Print(_localizer["Templates.Psay"].AReplace(["name", "msg"], [name, msg]), "");
            caller.Print(_localizer["Templates.PsayToSender"].AReplace(["name", "msg", "target"], [name, msg, target.PlayerName]), "");
        }, blockedArgs: ["@bots"]);
    }
    public static void Reply(CCSPlayerController caller, List<string> args, CommandInfo info)
    {
        if (!LastPrivateMessage.TryGetValue(caller, out var target) || target == null)
        {
            caller.Print(_localizer["Message.NoLastMessage"], "");
            return;
        }
        var name = caller.PlayerName;
        var msg = string.Join(" ", args);
        target.Print(_localizer["Templates.Reply"].AReplace(["name", "msg"], [name, msg]), "");
        caller.Print(_localizer["Templates.PsayToSender"].AReplace(["name", "msg", "target"], [name, msg, target.PlayerName]), "");
    }

    public static void ISay(CCSPlayerController caller, List<string> args, CommandInfo info)
    {
        if (!float.TryParse(args[0], out var time))
        {
            throw new ArgumentException("Time must be a number");
        }
        var img = string.Join("", args.Skip(1));
        var players = PlayersUtils.GetOnlinePlayers();
        foreach (var p in players)
        {
            p.HtmlMessage($"<img src='{img}'>", time);
        }
    }
    public static void HSay(CCSPlayerController caller, List<string> args, CommandInfo info)
    {
        if (!float.TryParse(args[0], out var time))
        {
            throw new ArgumentException("Time must be a number");
        }
        var msg = string.Join(" ", args.Skip(1));
        var players = PlayersUtils.GetOnlinePlayers();
        foreach (var p in players)
        {
            p.HtmlMessage(msg, time);
        }
    }
    public static void ASay(CCSPlayerController? caller, List<string> args, CommandInfo info)
    {
        var name = caller?.PlayerName ?? "CONSOLE";
        var msg = string.Join(" ", args);
        var targets = _api.ServerAdmins.Where(x => x.Controller != null && !x.IsDisabled);
        foreach (var t in targets)
        {
            t.Controller.Print(_localizer["Templates.FromAdminToAdmins"].AReplace(["name", "msg"], [name, msg]), "");
        }
    }
}