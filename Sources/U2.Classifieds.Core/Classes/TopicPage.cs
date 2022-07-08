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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2.Classifieds.Core;

public enum ItemCondition
{
    Unknown,
    New,
    Used,
}

public enum SellerType
{
    Unknown,
    Business,
    Private,
}

public sealed class TopicPage
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string OriginalId { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public Guid UserId { get; set; }
    public List<string> Images { get; set; }
    public SellerType SellerType { get; set; }
    public ItemCondition ItemCondition { get; set; }
    public List<string> Phones { get; set; }
    public List<string> DeliveryInfo { get; set; }
}
