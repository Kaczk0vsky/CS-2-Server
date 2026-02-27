using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Cvars;

namespace WelcomePlugin;

public class WelcomePlugin : BasePlugin
{
    private CounterStrikeSharp.API.Modules.Timers.Timer? _timer;
    private readonly string _modeName = "[COD MOD]";

    public override string ModuleName => "Base Plugin";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Kaczk0vsky";
    public override string ModuleDescription => "Cycles between messages on chat";

    public override void Load(bool hotReload)
    {
        Console.WriteLine("Welcome Plugin loaded successfully!");
        // TODO: Change timer to longer value
        _timer = AddTimer(30.0f, BroadcastMessage, TimerFlags.REPEAT);

        // Event: player connect
        RegisterEventHandler<EventPlayerConnect>((@event, info) =>
        {
            var player = @event.Userid;
            Console.WriteLine($"Player {player?.PlayerName} connected.");
            return HookResult.Continue;
        });

        // Command /rules
        AddCommand("rules", "Show server rules", (player, info) =>
        {
            player?.PrintToChat($"{ChatColors.Green}Server rules:{ChatColors.Default} Be nice and respect others.");
        });
    }

    private void BroadcastMessage()
    {
        switch (Random.Shared.Next(0, 8))
        {
            case 0:
                Server.PrintToChatAll(
                    $" {ChatColors.Green}{_modeName}{ChatColors.Default} By playing here you accept {ChatColors.Blue}/rules");
                break;

            case 1:
                Server.PrintToChatAll(
                    $" {ChatColors.Green}{_modeName}{ChatColors.Default} Visit our website: {ChatColors.Blue}https://www.localhost.com");
                break;

            case 2:
                var ip = ConVar.Find("ip")?.StringValue ?? "unknown";
                var port = ConVar.Find("hostport")?.GetPrimitiveValue<int>() ?? 0;

                Server.PrintToChatAll(
                    $" {ChatColors.Green}{_modeName}{ChatColors.Default} Add: {ChatColors.Blue}{ip}:{port}{ChatColors.Default} to favourites!");
                break;

            case 3:
                Server.PrintToChatAll($" {ChatColors.Green}{_modeName}{ChatColors.Default} Wpisz {ChatColors.Yellow}!klasa{ChatColors.Default} aby wybrać klasę!");
                break;

            case 4:
                Server.PrintToChatAll($" {ChatColors.Green}{_modeName}{ChatColors.Default} Wpisz {ChatColors.Yellow}!ranks{ChatColors.Default} aby zobaczyć rangi!");
                break;

            case 5:
                Server.PrintToChatAll($" {ChatColors.Green}{_modeName}{ChatColors.Default} Wpisz {ChatColors.Yellow}!myrank{ChatColors.Default} aby zobaczyć swoją rangę!");
                break;

            case 6:
                Server.PrintToChatAll($" {ChatColors.Green}{_modeName}{ChatColors.Default} Wpisz {ChatColors.Yellow}!top{ChatColors.Default} aby zobaczyć top graczy!");
                break;

            case 7:
                Server.PrintToChatAll($" {ChatColors.Green}{_modeName}{ChatColors.Default} Wpisz {ChatColors.Yellow}!codstats{ChatColors.Default} aby zobaczyć statystyki COD!");
                break;
        }
    }
}