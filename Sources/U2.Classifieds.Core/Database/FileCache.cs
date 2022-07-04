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
    private static string UrlToPath(string folder, string url)
    {
        var cachePath = CoreSettings.Default.CacheDirectory;
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(url));
        var hashString = Encoding.UTF8.GetString(hashBytes);
        var p1 = hashString.Substring(0, 2);
        var p2 = hashString.Substring(2, 2);
        var p3 = hashString.Substring(3, 2);
        var p4 = hashString.Substring(4, 2);

        return Path.Combine(cachePath, p1, p2, p3, p4, $"{hashString}.html");
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
        var path = Path.Combine(cachePath,"Cache", folder, 
            id1.ToString().PadLeft(3, '0'), 
            id2.ToString().PadLeft(3, '0'), 
            $"{id3.ToString().PadLeft(3, '0')}{id4}.html");
        return path;
    }

    public static string TryGetTopicCache(string url)
    {
        var path = UrlToPath("topics", url);
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        return null;
    }

    public static string TryGetBranchCache(string url)
    {
        var path = UrlToPath("branches", url);
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
        var path = UrlToPath("topics", url);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content);
    }

    public static void PutBranchCache(string url, string content)
    {
        var path = UrlToPath("branches", url);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, content);
    }
}
