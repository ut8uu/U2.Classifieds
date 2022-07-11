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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using U2.Classifieds.Core;

namespace U2.Classifieds.Loader;

internal sealed class Runner
{
    private readonly IProcessor _processor;

    public Runner(LoaderOptions loaderOptions)
    {
        var options = new Options
        {
            Images = loaderOptions.Images,
            Topics = loaderOptions.Topics,
            Branches = loaderOptions.Branches,
            InitBranches = loaderOptions.InitBranches,
        };
        _processor = new OlxProcessor(options);
    }

    public async Task RunAsync(CancellationToken token)
    {
        await _processor.RunAsync(token);
    }

}