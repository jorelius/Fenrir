using Fenrir.Core.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fenrir.Core.Extensions
{
    public static class OptionDescriptionListExtensions
    {
        public static Dictionary<string, Option> ToOptionsDictionary(this List<OptionDescription> descriptions)
        {
            if (descriptions == null)
            {
                throw new ArgumentException("descriptions must not be null", "descriptions"); 
            }

            return descriptions.ToDictionary(k => k.Key, v => new Option(v)); 
        }
    }
}
