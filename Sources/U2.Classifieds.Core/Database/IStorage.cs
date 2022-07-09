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

using System.Text.Json;
using MongoDB.Bson;

namespace U2.Classifieds.Core;

public interface IStorage
{
    Task AddBranchAsync(BranchDto branch, CancellationToken cancellationToken);
    Task<BranchDto> TryGetBranchAsync(int originalBranchId, CancellationToken cancellationToken);
    Task<BranchDto> TryGetBranchAsync(Guid branchId, CancellationToken cancellationToken);
    Task UpdateBranchAsync(BranchDto branch, CancellationToken cancellationToken);
    Task DeleteBranchAsync(Guid id, CancellationToken cancellationToken);
    IAsyncEnumerable<BranchDto> GetBranchesAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<BranchDto> GetBranchesAsync(Guid parentId, CancellationToken cancellationToken);

    Task AddTopicAsync(TopicDto topic, CancellationToken cancellationToken);
    Task<TopicDto> TryGetTopicAsync(Guid id, CancellationToken cancellationToken);
    Task<TopicDto> TryGetTopicAsync(string topic, CancellationToken cancellationToken);
    Task UpdateTopicByOriginalIdAsync(TopicDto topic, CancellationToken cancellationToken);
    Task DeleteTopicAsync(Guid id, CancellationToken cancellationToken);

    IAsyncEnumerable<TopicDto> GetTopicsAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<TopicDto> GetTopicsAsync(Guid branchId, CancellationToken cancellationToken);

    Task<TopicDto> GetTopicAsync(BsonDocument filter, CancellationToken token);

    Task<bool> HasTopicWithUrlAsync(string url, CancellationToken cancellationToken);
    Task<bool> HasTopicWithOriginalIdAsync(string originalId, CancellationToken cancellationToken);
    Task<bool> HasBranchAsync(string branch, CancellationToken cancellationToken);
    Task<bool> HasBranchAsync(int originalId, CancellationToken cancellationToken);

}
