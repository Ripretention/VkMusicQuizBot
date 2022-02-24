using System;
using VkNet.Enums.SafetyEnums;
using VkNetLongpoll;
using System.Threading.Tasks;
using VkNet.Model;
using VkNet.Model.GroupUpdate;
using System.Text.Json;

namespace VkMusicQuizBot
{
    public class NewMessageHandler
    {
        private LongpollEventHandler lpHandler;
        public NewMessageHandler(LongpollEventHandler lpHandler)
        {
            this.lpHandler = lpHandler ?? throw new ArgumentNullException(nameof(lpHandler));
        }

        public void Release()
        {
            lpHandler.On<Message>(GroupUpdateType.MessageEvent, (ctx, next) => 
            {
                payloadHandle(ctx);

                next();
                return Task.CompletedTask;
            });
        }

        public void payloadHandle(Message msg)
        {
            if (msg.Payload == null || msg.Payload == String.Empty) return;

            try
            {
                var payload = JsonSerializer.Deserialize<Utils.CommandPayload>(msg.Payload);
                msg.Text = payload.Command;
            }
            catch (Exception)
            { 
            }
        }
    }
}
