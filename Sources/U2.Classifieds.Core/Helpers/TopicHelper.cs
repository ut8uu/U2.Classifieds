using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace U2.Classifieds.Core;

public static class TopicHelper
{
    public static void ExtractTopicTitle(HtmlDocument doc, TopicDto topic)
    {
        var titleXPath = "//h1[@data-cy='ad_title']";
        var titleNode = doc.DocumentNode.SelectSingleNode(titleXPath);

        if (titleNode == null)
        {
            Console.WriteLine($"Cannot find title in {topic.Url}");
            topic.ParserStatusCode = ParserStatusCode.Fail;
            return;
        }

        topic.Title = titleNode.InnerText.Trim();
    }

    public static void ExtractTopicDescription(HtmlDocument doc, TopicDto topic)
    {
        var descriptionXPath = "/html/body/div/div/div[3]/div[3]/div/div[2]";
        var descriptionNode = doc.DocumentNode.SelectSingleNode(descriptionXPath);
        if (descriptionNode == null)
        {
            Console.WriteLine($"Cannot find description in {topic.Url}");
            topic.ParserStatusCode = ParserStatusCode.Fail;
            return;
        }

        topic.Description = descriptionNode.OuterHtml;
    }

    public static void ExtractTopicPrice(HtmlDocument doc, TopicDto topic)
    {
        var xPath = "//div[@data-testid='ad-price-container']/h3";
        var node = doc.DocumentNode.SelectSingleNode(xPath);
        if (node == null)
        {
            Console.WriteLine($"Cannot find price in {topic.Url}");
            topic.ParserStatusCode = ParserStatusCode.Fail;
            return;
        }

        var value = node.InnerText.Trim();
        var priceRegExpr = "([\\d\\s]+)\\s+([^\\d\\.]+)";
        if (!RegularExpressionHelper.Match(priceRegExpr, value, RegexOptions.IgnoreCase, out var matches))
        {
            Console.WriteLine($"Cannot recognize price in {topic.Url}");
            topic.ParserStatusCode = ParserStatusCode.Fail;
            return;
        }

        if (!int.TryParse(matches[1], out var intValue))
        {
            Console.WriteLine($"Cannot convert price to int in {topic.Url}");
            topic.ParserStatusCode = ParserStatusCode.Fail;
            return;
        }

        topic.Price = intValue;
        topic.Currency = matches[2].Trim();
    }

    public static void ExtractTopicImages(HtmlDocument doc, TopicDto topic)
    {
        var xPath = "//div[@class='swiper-container']//img";
        var images = doc.DocumentNode.SelectNodes(xPath);
        if (images == null)
        {
            Console.WriteLine($"Cannot find images in {topic.Url}");
            topic.ParserStatusCode = ParserStatusCode.Fail;
            return;
        }

        foreach (var image in images)
        {
            var src = image.Attributes["src"];
            if (src != null)
            {
                topic.Images.Add(src.Value.Trim());
                continue;
            }

            src = image.Attributes["data-src"];
            if (src != null)
            {
                topic.Images.Add(src.Value.Trim());
                continue;
            }
        }
    }
}
