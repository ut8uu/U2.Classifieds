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

public static class UrlHelper
{
    private const string OriginalIdInUrlRegExpr = "-([^.-]+).html";

    public static string GetOriginalIdFromUrl(string url)
    {
        return RegularExpressionHelper.MatchAndGetFirst(OriginalIdInUrlRegExpr, url);
    }

    public static string TrimUrl(string url)
    {
        var x = url.Trim();
        var idx = x.IndexOf('?');
        if (idx > -1)
        {
            x = x[..idx];
        }

        return x;
    }

    public static string FixUrl(string url)
    {
        var x = TrimUrl(url);
        if (x.StartsWith('/'))
        {
            x = $"https://www.olx.ua{x}";
        }

        x = x.Replace("/d/uk/obyavlenie", "/d/obyavlenie");

        var pos = x.IndexOfAny(new[] { '?', '#' });
        if (pos > -1)
        {
            x = x[..pos];
        }

        return x;
    }
}
