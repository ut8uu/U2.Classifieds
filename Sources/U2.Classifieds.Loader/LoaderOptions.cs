using CommandLine.Text;
using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2.Classifieds.Loader;

public sealed class LoaderOptions
{
    [Option('i', "images", Required = false, HelpText = "Whether to process images.")]
    public bool Images { get; set; }

    [Option('t', "topics", Required = false, HelpText = "Whether to process topics.")]
    public bool Topics { get; set; }

    [Option('b', "branches", Required = false, HelpText = "Whether to process branches.")]
    public bool Branches { get; set; }

    [Option("init-branches", Required = false, HelpText = "Whether to init branches.")]
    public bool InitBranches { get; set; }

}