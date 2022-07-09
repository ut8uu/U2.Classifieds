using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2.Classifieds.Core.Database
{
    public sealed class ImageDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TopicId { get; set; }
        public string Url { get; set; }
        public UrlLoadState LoadState { get; set; }
        public UrlLoadStatusCode StatusCode { get; set; }
        public string LocalPath { get; set; }
    }
}
