using Fenrir.Core.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fenrir.Core.Extensions
{
    public static class OptionListExtensions
    {
        public static Dictionary<string, Option> ToOptionsDictionary(this List<Option> options)
        {
            if (options == null)
            {
                throw new ArgumentException("options must not be null", "options"); 
            }

            return options.ToDictionary(k => k.Description.Key, v => v); 
        }
    }
}
