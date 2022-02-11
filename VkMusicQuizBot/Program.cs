using VkNet;
using System;
using System.Linq;
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

            Console.WriteLine("start..");
            var vkApi = new VkApi();
            await vkApi.AuthorizeAsync(new VkNet.Model.ApiAuthParams { AccessToken = cfg.Vk.AccessToken });

            var longpoll = new Longpoll(vkApi, (long)cfg.Vk.GroupId);
            var admCommands = new AdministrationCommands(longpoll.Handler);
            admCommands.Release();

            await longpoll.Start();
            Console.WriteLine("end");
        }
    }
}
