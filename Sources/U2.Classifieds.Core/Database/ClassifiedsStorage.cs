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

using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace U2.Classifieds.Core;

public sealed class ClassifiedsStorage : IStorage
{
    private const string BranchesCollectionName = "Branches";
    private const string TopicsCollectionName = "Topics";

    private readonly IMongoDatabase _database;
    readonly IMongoCollection<BranchDto> _branchesCollection;
    readonly IMongoCollection<TopicDto> _topicsCollection;

    public ClassifiedsStorage(IMongoDatabase database)
    {
        _database = database;
        _branchesCollection = database.GetCollection<BranchDto>(BranchesCollectionName);
        _topicsCollection = database.GetCollection<TopicDto>(TopicsCollectionName);
    }

    public Task AddBranchAsync(BranchDto branch, CancellationToken cancellationToken)
    {
        return _branchesCollection.InsertOneAsync(branch, new InsertOneOptions(), cancellationToken);
    }

    public async Task<BranchDto> TryGetBranchAsync(FilterDefinition<BranchDto> filter, CancellationToken cancellationToken)
    {
        using var cursor = await _branchesCollection.FindAsync(filter, options: null, cancellationToken);
        return cursor.FirstOrDefault();
    }

    public Task<BranchDto> TryGetBranchAsync(int originalBranchId, CancellationToken cancellationToken)
    {
        return _branchesCollection.Find(b => b.OriginalId == originalBranchId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<BranchDto> TryGetBranchAsync(Guid branchId, CancellationToken cancellationToken)
    {
        return _branchesCollection.Find(b => b.Id == branchId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task UpdateBranchAsync(BranchDto branch, CancellationToken cancellationToken)
    {
        return _branchesCollection.ReplaceOneAsync(p => p.Url == branch.Url, branch, new ReplaceOptions { }, cancellationToken);
    }

    public Task DeleteBranchAsync(Guid id, CancellationToken cancellationToken)
    {
        return _branchesCollection.DeleteOneAsync(b => b.Id == id, cancellationToken);
    }

    public async IAsyncEnumerable<BranchDto> GetBranchesAsync(CancellationToken cancellationToken)
    {
        var cursor = await _branchesCollection.FindAsync(b => !string.IsNullOrEmpty(b.Url), options: null, cancellationToken);
        await foreach (var branch in cursor.ToAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return branch;
        }
    }

    public async IAsyncEnumerable<BranchDto> GetBranchesAsync(Guid parentId, CancellationToken cancellationToken)
    {
        var cursor = await _branchesCollection.FindAsync(b => b.ParentId == parentId, options: null, cancellationToken);
        await foreach (var branch in cursor.ToAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return branch;
        }
    }

    public Task AddTopicAsync(TopicDto Topic, CancellationToken cancellationToken)
    {
        return _topicsCollection.InsertOneAsync(Topic, new InsertOneOptions(), cancellationToken);
    }

    public async Task<TopicDto> TryGetTopicAsync(FilterDefinition<TopicDto> filter, CancellationToken cancellationToken)
    {
        using var cursor = await _topicsCollection.FindAsync(filter, options: null, cancellationToken);
        return cursor.FirstOrDefault();
    }
    
    public Task<TopicDto> TryGetTopicAsync(Guid id, CancellationToken cancellationToken)
    {
        return _topicsCollection.Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TopicDto> TryGetTopicAsync(string url, CancellationToken cancellationToken)
    {
        return _topicsCollection.Find(x => x.Url == url)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task UpdateTopicByOriginalIdAsync(TopicDto Topic, CancellationToken cancellationToken)
    {
        return _topicsCollection.ReplaceOneAsync(x => x.OriginalId == Topic.OriginalId, Topic, new ReplaceOptions { }, cancellationToken);
    }

    public Task UpdateTopicByIdAsync(TopicDto Topic, CancellationToken cancellationToken)
    {
        return _topicsCollection.ReplaceOneAsync(x => x.Id == Topic.Id, Topic, new ReplaceOptions { }, cancellationToken);
    }

    public Task DeleteTopicAsync(Guid id, CancellationToken cancellationToken)
    {
        return _topicsCollection.DeleteOneAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TopicDto> GetTopicAsync(BsonDocument filter, CancellationToken token)
    {
        using var cursor = await _topicsCollection.FindAsync(filter, options: null, token);
        return cursor.FirstOrDefault(token);
    }

    public async IAsyncEnumerable<TopicDto> GetTopicsAsync(CancellationToken cancellationToken)
    {
        var cursor = await _topicsCollection.FindAsync(x => !string.IsNullOrEmpty(x.Url), options: null, cancellationToken);
        await foreach (var topic in cursor.ToAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return topic;
        }
    }

    public async IAsyncEnumerable<TopicDto> GetTopicsAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var cursor = await _topicsCollection.FindAsync(b => b.BranchId == branchId, options: null, cancellationToken);
        await foreach (var Topic in cursor.ToAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return Topic;
        }
    }

    public Task<bool> HasTopicWithUrlAsync(string url, CancellationToken cancellationToken)
    {
        var foundTopic = _topicsCollection.Find(x => x.Url == url)
            .FirstOrDefault(cancellationToken);
        return Task.FromResult(foundTopic != null);
    }

    public Task<bool> HasTopicWithOriginalIdAsync(string originalId, CancellationToken cancellationToken)
    {
        var foundTopic = _topicsCollection.Find(x => x.OriginalId == originalId)
            .FirstOrDefault(cancellationToken);
        return Task.FromResult(foundTopic != null);
    }

    public Task<bool> HasBranchAsync(string url, CancellationToken cancellationToken)
    {
        var branchesCount = _branchesCollection.Find(x => x.Url == url)
            .CountDocumentsAsync(cancellationToken);
        return Task.FromResult(branchesCount.Result > 0);
    }

    public Task<bool> HasBranchAsync(int originalId, CancellationToken cancellationToken)
    {
        var hasBranch = _branchesCollection.Find(x => x.OriginalId == originalId)
            .Any(cancellationToken);
        return Task.FromResult(hasBranch);
    }
}
