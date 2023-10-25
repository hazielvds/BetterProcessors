using BrokeProtocol.API;
using BrokeProtocol.Entities;
using System;

namespace BetterProcessors
{
    public class Core : Plugin
    {
        public static Core Instance { get; private set; }
        public FileHandler FileHandler { get; private set; } = new FileHandler();
        public EntityHandler EntityHandler { get; private set; } = new EntityHandler();

        public Core()
        {
            Instance = this;

            Info = new PluginInfo("Better Processors", "bmake", "Create simple processors.");
            FileHandler.LoadProcessors();

            EntityHandler.LoadEntities();
        }
    }
    public class Commands : IScript
    {
        public Commands()
        {
            CommandHandler.RegisterCommand("preload", new Action<ShPlayer>(Reload));
        }

        public void Reload(ShPlayer player)
        {
            InterfaceHandler.SendGameMessageToAll("&6Reloading all trader files...");

            Core.Instance.FileHandler.LoadProcessors();
        }
    }
}
