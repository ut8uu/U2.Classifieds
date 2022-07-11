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

using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace U2.Classifieds.Core;

public enum CacheType
{
    Branch,
    Topic,
}

public static class FileCache
{
    public static string UrlToPath(string folder, string url, string extension)
    {
        var cachePath = CoreSettings.Default.CacheDirectory;
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(url));

        var hashString = BitConverter.ToString(hashBytes).Replace("-", "");
        var p1 = hashString.Substring(0, 2);
        var p2 = hashString.Substring(2, 2);
        var p3 = hashString.Substring(4, 2);
        var p4 = hashString.Substring(6, 2);

        return Path.Combine(cachePath, folder, p1, p2, p3, p4, $"{hashString}.{extension}");
    }

    private static string IdToPath(string folder, int id, int start = 0)
    {
        var cachePath = CoreSettings.Default.CacheDirectory;
        var id1 = id / 1000000;
        var id2 = (id % 1000000) / 1000;
        var id3 = (id % 1000);
        var id4 = $"{Path.PathSeparator}{start}";
        if (start == 0)
        {
            id4 = string.Empty;
        }
        var path = Path.Combine(cachePath, "Cache", folder,
            id1.ToString().PadLeft(3, '0'),
            id2.ToString().PadLeft(3, '0'),
            $"{id3.ToString().PadLeft(3, '0')}{id4}.html");
        return path;
    }

    private static string IdToPath(string folder, string id, int start = 0)
    {
        var cachePath = CoreSettings.Default.CacheDirectory;
        var id1 = id.Substring(0, 1);
        var id2 = id.Substring(1, 1);
        var id3 = id.Substring(2, 1);
        var id4 = id.Substring(3, 1);
        var id5 = id.Substring(4);
        if (start == 0)
        {
            id4 = string.Empty;
        }
        var path = Path.Combine(cachePath, "Cache", folder,
            id1, id2, id3, id4, $"{id5}.html");
        return path;
    }

    public static string TryGetTopicCache(string url)
    {
        var id = UrlHelper.GetOriginalIdFromUrl(url);
        var path = UrlToPath("topics", id, "html");
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        return null;
    }

    public static string TryGetBranchCache(string url)
    {
        return null;
        var path = UrlToPath("branches", url, "html");
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        return null;
    }

    public static void PutCache(int id, int start, string folder, string content)
    {
        var path = IdToPath(folder, id, start);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content);
    }

    public static void PutTopicCache(string url, string content)
    {
        var id = UrlHelper.GetOriginalIdFromUrl(url);
        var path = UrlToPath("topics", id, "html");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content);
    }

    public static void PutBranchCache(string url, string content)
    {
        var path = UrlToPath("branches", url, "html");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content);
    }
}
