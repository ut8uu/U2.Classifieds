/* 
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

using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using U2.Classifieds.Core.Database;

namespace U2.Classifieds.Core;

public sealed class ClassifiedsService
{
    private ClassifiedsStorage _storage;

    public ClassifiedsService(ClassifiedsStorage storage)
    {
        _storage = storage;
    }

    #region Branches

    public async Task<BranchDto> GetWaitingBranchAsync(CancellationToken cancellationToken)
    {
        var filterBuilder = Builders<BranchDto>.Filter;
        var filter = filterBuilder.Lte(x => x.NextLoadTime, DateTime.UtcNow);

        return await _storage.TryGetBranchAsync(filter, cancellationToken);
    }

    public async Task AddBranchIfNotExistsAsync(BranchDto branch, CancellationToken cancellationToken)
    {
        if (!(await _storage.HasBranchAsync(branch.Url, cancellationToken)))
        {
            await _storage.AddBranchAsync(branch, cancellationToken);
        }
    }

    public async Task AddOrUpdateBranchAsync(BranchDto branch, CancellationToken cancellationToken)
    {
        if (await _storage.HasBranchAsync(branch.Url, cancellationToken))
        {
            await _storage.UpdateBranchAsync(branch, cancellationToken);
        }
        else
        {
            await _storage.AddBranchAsync(branch, cancellationToken);
        }
    }

    public Task<BranchDto> GetBranchAsync(string url, CancellationToken token)
    {
        var filterBuilder = Builders<BranchDto>.Filter;
        var filter = filterBuilder.Eq(x => x.Url, url);
        return _storage.TryGetBranchAsync(filter, token);
    }

    public Task<BranchDto> GetBranchByTitle(string title, CancellationToken token)
    {
        var filterBuilder = Builders<BranchDto>.Filter;
        var filter = filterBuilder.Eq(x => x.Title, title);
        return _storage.TryGetBranchAsync(filter, token);
    }

    #endregion

    #region Topics

    private static void FixTopic(TopicDto topic)
    {
        if (string.IsNullOrEmpty(topic.OriginalId))
        {
            topic.OriginalId = UrlHelper.GetOriginalIdFromUrl(topic.Url);
        }
    }

    public async Task<TopicDto> GetWaitingTopicAsync(CancellationToken cancellationToken)
    {
        var filterBuilder = Builders<TopicDto>.Filter;
        var filter = filterBuilder.Eq(x => x.LoadState, UrlLoadState.Unknown);

        return await _storage.TryGetTopicAsync(filter, cancellationToken);
    }

    public async Task<bool> AddTopicIfNotExistsAsync(TopicDto topic, CancellationToken cancellationToken)
    {
        FixTopic(topic);
        if (!(await _storage.HasTopicWithOriginalIdAsync(topic.OriginalId, cancellationToken)))
        {
            await _storage.AddTopicAsync(topic, cancellationToken);
            return true;
        }

        return false;
    }

    public async Task AddOrUpdateTopicAsync(TopicDto topic, CancellationToken cancellationToken)
    {
        FixTopic(topic);
        if (await _storage.HasTopicWithOriginalIdAsync(topic.OriginalId, cancellationToken))
        {
            await _storage.UpdateTopicByOriginalIdAsync(topic, cancellationToken);
        }
        else
        {
            await _storage.AddTopicAsync(topic, cancellationToken);
        }
    }

    public async Task UpdateTopicAsync(TopicDto topic, CancellationToken cancellationToken)
    {
        FixTopic(topic);
        await _storage.UpdateTopicByIdAsync(topic, cancellationToken);
    }

    #endregion

    #region Users

    public async Task<UserDto> AddOrUpdateUserAsync(UserDto user, CancellationToken cancellationToken)
    {
        var existingUser = await _storage.TryGetUserAsync(user.OriginalId, cancellationToken);
        if (existingUser != null)
        {
            user.Id = existingUser.Id;
            await _storage.UpdateUserAsync(user, cancellationToken);
        }
        else
        {
            await _storage.AddUserAsync(user, cancellationToken);
        }

        return user;
    }

    #endregion

    #region Users

    public async Task<ImageDto> GetWaitingImageAsync(CancellationToken cancellationToken)
    {
        var filterBuilder = Builders<ImageDto>.Filter;
        var filter = filterBuilder.Eq(x => x.LoadState, UrlLoadState.Unknown);

        return await _storage.TryGetImageAsync(filter, cancellationToken);
    }

    public async Task AddOrUpdateImageAsync(ImageDto image, CancellationToken cancellationToken)
    {
        if (await _storage.HasImageAsync(image.Url, cancellationToken))
        {
            await _storage.UpdateImageAsync(image, cancellationToken);
        }
        else
        {
            await _storage.AddImageAsync(image, cancellationToken);
        }
    }

    #endregion

}
