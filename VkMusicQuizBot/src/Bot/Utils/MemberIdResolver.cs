using System.Linq;
using VkNet.Enums;
using VkNet.Model;
using VkNet.Abstractions;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace VkMusicQuizBot.Utils
{
    public class MemberIdResolver
    {
        private IVkApi api;
        public MemberIdResolver(IVkApi api)
        {
            this.api = api;
        }

        public async Task<long?> Resolve(Message msg, string captureStr = null)
        {
            if (msg.ReplyMessage != null)
                return msg.ReplyMessage.FromId;
            if (msg.ForwardedMessages.Count > 0)
                return msg.ForwardedMessages.First().FromId;

            return await Resolve(captureStr ?? msg.Text);
        }
        public async Task<long?> Resolve(string str)
        {
            if (str == null || str == System.String.Empty)
                return null;

            var idMatch = new Regex(@"(id|club|public)(\d+)", RegexOptions.IgnoreCase).Match(str);
            if (idMatch.Success)
                return (idMatch.Groups[1].Value == "id" ? 1 : -1) * long.Parse(idMatch.Groups[2].Value);
            if (new Regex(@"^[\d]+$", RegexOptions.IgnoreCase).IsMatch(str))
            {
                long parsedMemberId;
                long.TryParse(str, out parsedMemberId);
                if (parsedMemberId != 0)
                    return parsedMemberId;
            }

            var response = await api.Utils.ResolveScreenNameAsync(str);
            return response != null && response.Id != null && (response.Type == VkObjectType.User || response.Type == VkObjectType.Group)
                    ? (response.Type == VkObjectType.Group ? -1 : 1) * response.Id
                    : null;
        }
    }
}
