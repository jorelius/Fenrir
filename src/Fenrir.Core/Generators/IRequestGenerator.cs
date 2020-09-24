using Fenrir.Core.Comparers;
using Fenrir.Core.Models.RequestTree;
using System;
using System.Collections.Generic;

namespace Fenrir.Core.Generators
{
    public interface IRequestGenerator
    {
        /// <summary>
         /// Unique id that differentiates
         /// each request generator
         /// </summary>
         /// <returns></returns>
        Guid Id { get; }

        /// <summary>
        /// Human readable name
        /// </summary>
        /// <returns></returns>
        string Name { get; }

        /// <summary>
        /// Generator description
        /// </summary>
        /// <returns></returns>
        string Description { get; }

        List<Option> Options { get; set; }

        /// <summary>
        /// Generator override for default 
        /// comparer factory
        /// </summary>
        ResultComparerFactory ComparerFactoryOverride { get; set; } 

        IEnumerable<Request> Run();
    }

}