/* 
 * This file is part of the U2.SharpTracker distribution
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

public enum ParserStatusCode
{
    Unknown,
    Success,
    Fail,
}

public enum UrlLoadStatusCode
{
    Unknown,
    Success,
    Failure,
    NotFound,
}

public enum UrlLoadState
{
    Unknown,
    Loading,
    Loaded,
    Completed, // regardless of the LoadStatus
}

public sealed class UrlInfo
{
    public UrlInfo() { }

    public UrlInfo(string url)
    {
        Url = url;
    }

    public string Url { get; set; }
    public int AttemptsLeft { get; set; } = 5;
    public UrlLoadState UrlLoadState { get; set; }
    public UrlLoadStatusCode UrlLoadStatusCode { get; set; }
}