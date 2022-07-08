using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2.Classifieds.Core;

public class BranchPage
{
    public string Url { get; set; }
    public Dictionary<string, string> Branches { get; set; }
    public List<TopicPage> Pages { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
