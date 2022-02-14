using Moq;
using NUnit.Framework;
using VkNet.Abstractions;
using System.Threading.Tasks;

namespace VkMusicQuizBot.Tests.Bot
{
    [TestFixture]
    public class MemberIdResolverTest
    {
        private Mock<IVkApi> vkApiMock;
        private Utils.MemberIdResolver revolver;

        [SetUp]
        public void Setup()
        {
            var utilsCategoryVkApiMock = new Mock<IUtilsCategory>();
            utilsCategoryVkApiMock
                .Setup(ld => ld.ResolveScreenNameAsync("somepublicid"))
                .Returns(Task.FromResult(new VkNet.Model.VkObject
                {
                    Id = 12,
                    Type = VkNet.Enums.VkObjectType.Group
                }));
            utilsCategoryVkApiMock
                .Setup(ld => ld.ResolveScreenNameAsync("someuserid"))
                .Returns(Task.FromResult(new VkNet.Model.VkObject
                {
                    Id = 1,
                    Type = VkNet.Enums.VkObjectType.User
                }));
            utilsCategoryVkApiMock
                .Setup(ld => ld.ResolveScreenNameAsync("someunsupportedid"))
                .Returns(Task.FromResult(new VkNet.Model.VkObject
                {
                    Id = 1432,
                    Type = VkNet.Enums.VkObjectType.Application
                }));
            utilsCategoryVkApiMock
                .Setup(ld => ld.ResolveScreenNameAsync("undefid"))
                .Returns(Task.FromResult(new VkNet.Model.VkObject { Id = null }));
            vkApiMock = new Mock<IVkApi>();
            vkApiMock
                .SetupGet(ld => ld.Utils)
                .Returns(utilsCategoryVkApiMock.Object);

            revolver = new Utils.MemberIdResolver(vkApiMock.Object);
        }
        [Test]
        public async Task ResolveFromOnlyNumbersTest([Values("123", "1", "32", "21")] string str)
        {
            var resolvedId = await revolver.Resolve(str);

            Assert.AreEqual(long.Parse(str), resolvedId);
        }
        [Test]
        [TestCase("id123", 123)]
            
        [TestCase("club321", -321)]

        [TestCase("https:://vk.com/public32", -32)]

        public async Task ResolveFromStandartLink(string str, long? expectedResolveResult)
        {
            var resolvedId = await revolver.Resolve(str);

            Assert.AreEqual(expectedResolveResult, resolvedId);
        }

        [Test]
        [TestCase("somepublicid", -12)]

        [TestCase("someuserid", 1)]

        [TestCase("someunsupportedid", null)]
        [TestCase("undefid", null)]
        public async Task ResolveFromLink(string str, long? expectedResolveResult)
        {
            var resolvedId = await revolver.Resolve(str);

            Assert.AreEqual(expectedResolveResult, resolvedId);
        }
    }
}
