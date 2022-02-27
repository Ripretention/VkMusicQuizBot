using System;
using VkNet.Model;
using VkNetLongpoll;
using System.Text.Json;
using System.Threading.Tasks;
using VkNet.Enums.SafetyEnums;
using Microsoft.Extensions.Logging;

namespace VkMusicQuizBot
{
    public class NewMessageHandler
    {
        private LongpollEventHandler lpHandler;
        private ILogger<NewMessageHandler> logger;
        public NewMessageHandler(LongpollEventHandler lpHandler, ILogger<NewMessageHandler> logger = null)
        {
            this.lpHandler = lpHandler ?? throw new ArgumentNullException(nameof(lpHandler));
            this.logger = logger;
        }

        public void Release()
        {
            lpHandler.On<MessageContext>(GroupUpdateType.MessageNew, (ctx, next) => 
            {
                logger?.LogInformation($"Message received: {((ctx.Body?.Text ?? "") == "" ? "empty" : ctx.Body.Text)}");
                payloadHandle(ctx.Body);

                next();
                return Task.CompletedTask;
            });
            logger?.LogDebug("Handler has initialized");
        }

        public void payloadHandle(Message msg)
        {
            if (msg.Payload == null || msg.Payload == String.Empty) return;

            try
            {
                var payload = JsonSerializer.Deserialize<Utils.CommandPayload>(msg.Payload);
                msg.Text = payload.Command;
                logger?.LogDebug("Message payload has parsed");
            }
            catch (Exception)
            { 
            }
        }
    }
}
