using CsvHelper.Configuration;
using Fenrir.Core.Models.RequestTree;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Cli
{
    public class RequestMap: ClassMap<Request>
    {
        public RequestMap()
        {
            Map(m => m.Url).Index(0).Name("url");
            Map(m => m.Method).Index(1).Name("method");
            Map(m => m.Payload).Index(2).Name("payload").Ignore();
            Map(m => m.ExpectedResult).Index(3).Name("expected").Ignore();
            Map(m => m.Pre).Ignore();
            Map(m => m.Metadata).Ignore();
        }
    }
}
