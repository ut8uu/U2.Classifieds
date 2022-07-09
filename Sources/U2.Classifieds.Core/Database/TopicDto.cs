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

namespace U2.Classifieds.Core;

public sealed class TopicDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BranchId { get; set; }
    public string OriginalId { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public string Currency { get; set; }
    public Guid UserId { get; set; }
    public List<string> Images { get; set; } = new();
    public SellerType SellerType { get; set; }
    public ItemCondition ItemCondition { get; set; }
    public List<string> Phones { get; set; }
    public List<string> DeliveryInfo { get; set; }
    public UrlLoadState LoadState { get; set; }
    public UrlLoadStatusCode LoadStatusCode { get; set; }
    public ParserStatusCode ParserStatusCode { get; set; }
}

