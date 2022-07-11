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

using CommandLine;
using MongoDB.Driver;
using U2.Classifieds.Loader;

CommandLine.Parser.Default.ParseArguments<LoaderOptions>(args)
    .WithParsed(RunOptions)
    .WithNotParsed(HandleParseError);

static void RunOptions(LoaderOptions opts)
{
    if (!opts.Images && !opts.Topics && !opts.Branches)
    {
        Console.WriteLine("No flages is specified. Processing topics and images is used by default.");
        opts.Topics = true;
        opts.Images = true;
    }

    var runner = new Runner(opts);
    var ctx = new CancellationTokenSource();

    var task = Task.Run(() => runner.RunAsync(ctx.Token));
    task.Wait();

    ctx.Cancel();
}

static void HandleParseError(IEnumerable<Error> errs)
{
    Console.WriteLine(string.Join("\r\n", errs.Select(x => x.Tag.ToString())));
}
