using Fenrir.Core.Generators;
using Fenrir.Core.Models.RequestTree;
using System;
using System.Collections.Generic;

namespace Fenrir.Core.Tests.Plugin
{
    public class TestPlugin : IRequestGenerator
    {
        public Guid Id => Guid.Parse("67a8976c-7155-40bc-b23a-b265e127c8e1");

        public string Name => "Test Plugin";

        public string Description => "Provides a Test plugin for unit testing";
        
        public List<Option> Options => new List<Option>
        {
            new Option(new OptionDescription("Number", "1", "number of requests"))
        };

        public IEnumerable<Request> Run()
        {
            throw new NotImplementedException();
        }
    }
}
