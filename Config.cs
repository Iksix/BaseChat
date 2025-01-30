using IksAdminApi;

namespace BaseChat;

public class Config : PluginCFG<Config>
{
    public static Config Instance {get; set;} = new Config();

    public bool CanPlayerSendMessageToAdmins {get; set;} = true; // Позволяет игроку отправлять сообщения админам с доступом к админ чату через @
    public bool CanPlayerReplyToMessage {get; set;} = true; // Позволяет игроку ответить на приватное сообщение через команду css_r
}