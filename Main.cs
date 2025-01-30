using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using IksAdminApi;
using Microsoft.Extensions.Localization;

namespace BaseChat;

public class Main : AdminModule
{
    public override string ModuleName => "BaseChat";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "iks__";
    public static Config Cfg = null!;
    public static IStringLocalizer sLocalizer = null!;

    [GameEventHandler]
    public HookResult OnDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null) return HookResult.Continue;
        Cmds.LastPrivateMessage.Remove(player);
        return HookResult.Continue;
    }
    

    private HookResult OnSay(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null) return HookResult.Continue;
        bool toTeam = commandInfo.GetArg(0) == "say_team";
        var msg = commandInfo.GetCommandString;
        if (toTeam)
        {
            msg = msg.Remove(0, 9);
        } else {
            msg = msg.Remove(0, 4);
        }
        if (msg.StartsWith("\""))
        {
            msg = msg.Remove(0, 1);
            msg = msg.Remove(msg.Length - 1, 1);
        }

        if (msg.StartsWith("@"))
        {
            var msgToSend = msg.Remove(0, 1);
            if (player.HasPermissions("basechat.adminchat"))
            {
                if (toTeam) { // Сообщение админам имеющим доступ к админ чату 
                    var targets = PlayersUtils.GetOnlinePlayers().Where(x => x.HasPermissions("basechat.adminchat"));
                    foreach (var t in targets)
                    {
                        t.Print(Localizer["Templates.FromAdminToAdmins"].AReplace(["name", "msg"], [player.PlayerName, msgToSend]), "");
                    }
                } else { // Сообщение всем от лица админа
                    var targets = PlayersUtils.GetOnlinePlayers();
                    foreach (var t in targets)
                    {
                        t.Print(Localizer["Templates.FromAdminToPlayers"].AReplace(["name", "msg"], [player.PlayerName, msgToSend]), "");
                    }
                }
            } else {
                if (Cfg.CanPlayerSendMessageToAdmins) 
                {
                    var targets = PlayersUtils.GetOnlinePlayers().Where(x => x.HasPermissions("basechat.adminchat"));
                    foreach (var t in targets)
                    {
                        t.Print(Localizer["Templates.FromPlayerToAdmins"].AReplace(["name", "msg"], [player.PlayerName, msgToSend]), "");
                    }
                } else {
                    player.Print(Localizer["Message.CantSendMessageToAdmins"], "");
                }
            }
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    public override void Ready()
    {
        sLocalizer = Localizer;
        Config.Instance = Config.Instance.ReadOrCreate(ModuleDirectory + "/../../configs/plugins/IksAdmin_Modules/BaseChat.json", Config.Instance);
        Cfg = Config.Instance;
        AddTimer(1, () => {
            AddCommandListener("say", OnSay);
            AddCommandListener("say_team", OnSay);
        });
        Api.RegisterPermission("basechat.adminchat", "b"); // Разрешение для доступа к админ-чату @
        Api.RegisterPermission("basechat.psay", "b"); // css_psay <#uid/#steamId/name/@...> <message> - личное сообщение
        Api.RegisterPermission("basechat.reply", "*"); // css_psay <#uid/#steamId/name/@...> <message> - личное сообщение
        Api.RegisterPermission("basechat.isay", "b"); // css_isay <time> "image url" - вывести изображение в худ
        Api.RegisterPermission("basechat.hsay", "b"); // css_hsay <time> <message> - сообщение для игроков в hud
        Api.RegisterPermission("basechat.asay", "b"); // css_asay <message> - сообщение для админов (Не зависит от флагов админов)
    }
    public override void InitializeCommands()
    {
        Api.AddNewCommand(
            "psay",
            "Отправить личное сообщение",
            "basechat.psay",
            "css_psay <#uid/#steamId/name/@...> <message>",
            Cmds.Psay,
            minArgs: 2
        );
        Api.AddNewCommand(
            "r",
            "Ответить на личное сообщение",
            "basechat.reply",
            "css_r <message>",
            Cmds.Reply,
            minArgs: 1
        );
        Api.AddNewCommand(
            "isay",
            "Вывести изображение в HUD",
            "basechat.isay",
            "css_isay <time> <img url>",
            Cmds.ISay,
            minArgs: 2
        );
        Api.AddNewCommand(
            "hsay",
            "Написать в худ",
            "basechat.hsay",
            "css_hsay <time> <message>",
            Cmds.HSay,
            minArgs: 2
        );
        Api.AddNewCommand(
            "asay",
            "Отправить сообщение админам",
            "basechat.asay",
            "css_asay <message>",
            Cmds.ASay,
            minArgs: 1
        );
    }

    public override void Unload(bool hotReload)
    {
        base.Unload(hotReload);
        RemoveCommandListener("say", OnSay, HookMode.Pre);
        RemoveCommandListener("say_team", OnSay, HookMode.Pre);
    }
}
