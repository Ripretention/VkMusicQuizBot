using VkNet;
using System;
using VkNetLongpoll;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace VkMusicQuizBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cfg = new BotConfigurationBuilder().Build().Get<BotConfiguration>();
            var db = new FileDatabase(cfg.Database.PathFolder);

            var downloader = new AudioTrackDownloader(cfg.FFMpeg);

            var vkApi = new VkApi();
            await vkApi.AuthorizeAsync(new VkNet.Model.ApiAuthParams { AccessToken = cfg.Vk.AccessToken });
            var longpoll = new Longpoll(vkApi, (long)cfg.Vk.GroupId);

            var admCommands = new AdministrationCommands(longpoll.Handler, db, cfg.Developers);
            var commonCommands = new CommonCommands(longpoll.Handler, db, downloader);
            admCommands.Release();
            commonCommands.Release();

            Console.WriteLine("GO");
            await longpoll.Start();
        }
    }
}
