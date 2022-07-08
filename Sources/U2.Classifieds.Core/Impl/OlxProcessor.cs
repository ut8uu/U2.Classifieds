﻿/* 
 * This file is part of the U2.Classifieds distribution
 * (https://github.com/ut8uu/U2.Classifieds).
 * 
 * Copyright (c) 2022 Sergey Usmanov.
 * 
 * This program is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU General Public License as published by  
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MongoDB.Driver.GeoJsonObjectModel;
using U2.Classifieds.Core.Impl;

namespace U2.Classifieds.Core;

public class OlxProcessor : ProcessorBase, IProcessor
{
    public OlxProcessor()
        : base(new SvcSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "Olx_Classifieds",
        })
    {
        var initialBranch = new BranchDto
        {
            Url = "https://olx.ua",
            Title = "Main page",
        };
        //var task = Service.AddBranchIfNotExistsAsync(initialBranch, Token);
        //task.Wait(Token);

        InitBranches().Wait();
    }

    private async Task InitBranches()
    {
        var allBranches =
            OlxResources.InitialBranches.Split("\r\n".ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);
        foreach (var url in allBranches)
        {
            var branch = new BranchDto
            {
                Id = Guid.NewGuid(),
                Title = "",
                Url = url,
                OriginalId = 0,
                ParentId = Guid.Empty,
            };
            await Service.AddBranchIfNotExistsAsync(branch, Token);
        }
    }

    public async Task RunAsync(CancellationToken token)
    {
        Start();

        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(100, token);
            }
        }
        catch (TaskCanceledException)
        {
            // do nothing here, task was just canceled
        }

        Stop();
    }

    private async Task<List<BranchDto>> ParseBreadCrumbsAsync(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return Enumerable.Empty<BranchDto>().ToList();
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(content);
        var ul = doc.DocumentNode.SelectSingleNode("//ul[@class='breadcrumb offerslist clr marginbott5 small xxxx']");
        var li = doc.DocumentNode.SelectNodes("//ul[@class='breadcrumb offerslist clr marginbott5 small xxxx']/li");

        if (li == null)
        {
            return Enumerable.Empty<BranchDto>().ToList();
        }
        
        var rootBranch = await Service.GetBranchAsync("https://olx.ua/", Token);
        var list = new List<BranchDto>
        {
            rootBranch,
        };

        var mainTitles = new[] {"головна", "все объявления"};

        foreach (var breadcrumb in li)
        {
            var x1 = new HtmlDocument();
            x1.LoadHtml(breadcrumb.InnerHtml);
            var a = x1.DocumentNode.SelectSingleNode("//a");
            if (a != null)
            {
                var text = a.InnerText.Trim();

                // all elements except the last
                if (mainTitles.Any(x 
                        => text.Equals(x, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                var url = a.Attributes["href"].Value;
                if (url.StartsWith('/'))
                {
                    url = $"https://www.olx.ua{url}";
                }
                var branch = await Service.GetBranchAsync(url, Token);
                if (branch == null)
                {
                    branch = new BranchDto
                    {
                        Url = url,
                        ParentId = list.Last().Id,
                    };
                    await Service.AddBranchIfNotExistsAsync(branch, Token);
                }

                if (branch.ParentId == Guid.Empty)
                {
                    branch.ParentId = list.Last().Id;
                    await Service.AddOrUpdateBranchAsync(branch, Token);
                }
                list.Add(branch);
            }
            else
            {
                // the last element
                var title = breadcrumb.InnerText.Trim();
                var branch = new BranchDto
                {
                    Title = title,
                    ParentId = list.Last().Id,
                };
                list.Add(branch);
            }
        }

        return list;
    }

    protected override async Task LoadBranchPageAsync()
    {
        return;
        var branch = await Service.GetWaitingBranchAsync(Token);
        if (branch == null)
        {
            return;
        }

        try
        {
            Console.WriteLine($"Processing {branch.Url}");

            var urlInfo = new UrlInfo(branch.Url);
            var content = await DownloadAsync(urlInfo, UrlKind.Branch, Token);
            if (!string.IsNullOrEmpty(content))
            {
                /*
                var breadCrumbs = await ParseBreadCrumbsAsync(content);
                if (breadCrumbs.Any())
                {
                    var currentBranch = breadCrumbs.Last();
                    if (branch.ParentId == Guid.Empty)
                    {
                        branch.ParentId = currentBranch.ParentId;
                    }
    
                    if (string.IsNullOrEmpty(branch.Title))
                    {
                        branch.Title = currentBranch.Title;
                    }
    
                    breadCrumbs.Remove(breadCrumbs.Last());
    
                    foreach (var breadCrumb in breadCrumbs)
                    {
                        await Service.AddOrUpdateBranchAsync(branch, Token);
                    }
                }
                */

                var page = await ParsePageAsync(content);
                if (page.Topics.Any())
                {
                    foreach (var topic in page.Topics)
                    {
                        var topicDto = new TopicDto
                        {
                            Url = topic.Url,
                            Title = topic.Title,
                            BranchId = branch.Id,
                        };
                        Console.WriteLine($"Added topic {topic.Url}");
                        await Service.AddTopicIfNotExistsAsync(topicDto, Token);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var m = ex.Message;
        }

        branch.LoadState = UrlLoadState.Completed;
        branch.LoadStatusCode = UrlLoadStatusCode.Success;
        branch.NextLoadTime = DateTime.UtcNow.AddHours(1);
        await Service.AddOrUpdateBranchAsync(branch, Token);
    }

    private async Task<ClassifiedsPage> ParsePageAsync(string content)
    {
        var result = new ClassifiedsPage();

        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        var links = doc.DocumentNode.SelectNodes("//a");

        var allLinks = links
            .Where(x => x.GetAttributeValue("href", string.Empty).Length > 0)
            .Select(x => x.Attributes["href"].Value)
            .Distinct()
            .OrderBy(z => z).ToArray();

        var xx = allLinks
            .Where(x =>
                x.Contains("/d/uk/obyavlenie", StringComparison.InvariantCultureIgnoreCase)
                || x.Contains("/d/obyavlenie", StringComparison.InvariantCultureIgnoreCase)
                )
            .Select(x => x).ToList();

        foreach (var link in xx)
        {
            var topic = new TopicDto
            {
                Url = TrimUrl(link),
                BranchId = Guid.Empty,
                LoadState = UrlLoadState.Unknown,
                LoadStatusCode = UrlLoadStatusCode.Unknown,
            };
            result.Topics.Add(topic);
            await Service.AddOrUpdateTopicAsync(topic, Token);
        }

        return result;
    }

    protected override Task LoadTopicPageAsync()
    {
        return Task.FromResult(0);
    }
}

public sealed class ClassifiedsPage
{
    public List<BranchDto> Branches { get; set; } = new();
    public List<TopicDto> Topics { get; set; } = new();
}
