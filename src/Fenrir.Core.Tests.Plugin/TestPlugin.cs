﻿using Fenrir.Core.Comparers;
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
        
        public List<Option> Options { get; set; } = new List<Option>
        {
            new Option(new OptionDescription("Number", "1", "number of requests"))
        };
        public ResultComparerFactory ComparerFactoryOverride { get; set; } = null; 

        public IEnumerable<Request> Run()
        {
            var count = int.Parse(Options[0].Value);
            for(int i = 0; i < count; i++)
            {
                yield return new Request();
            }
        }
    }
}
