using U2.Classifieds.Core;

namespace U2.classifieds.Core.Tests
{
    public class TopicParserTests
    {
        [Fact]
        public void CanParseTopic1()
        {
            var topicContent= TestResources.topic1;
            var topic = new TopicDto();
            OlxProcessor.ParseTopicPage(topicContent, topic);

            Assert.Equal("Брошь, сиамское серебро, клеймо.", topic.Title);
            Assert.Equal(650, topic.Price);
            Assert.Equal("грн", topic.Currency);
            Assert.NotNull(topic.Images);
            Assert.Equal(4, topic.Images.Count);
            Assert.Equal(ItemCondition.Used, topic.ItemCondition);
            Assert.Contains("OLX Доставка", topic.DeliveryInfo);
        }

        [Fact]
        public void CanExtractUser()
        {
            var content = TestResources.topic1;
            var user = new UserDto();
            TopicHelper.ExtractUserInfo(content, user);
            Assert.Equal("6speq", user.OriginalId);
            Assert.Equal("https://www.olx.ua/d/list/user/6speq/", user.Url);
            Assert.Equal("Олена", user.Name);
        }
    }
}