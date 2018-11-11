using System.Collections.Generic;

namespace Fenrir.Core.Comparers
{
    public class ResultComparerFactory
    {
        private Dictionary<string, IResultComparer> _comparers; 

        public ResultComparerFactory()
        {
            var jsonComparer = new Json();
            var imageComparer = new Image();
            var defaultComparer = new Exact(); 
            
            _comparers = new Dictionary<string, IResultComparer>
            {
                { "application/json", jsonComparer },
                { "image/gif", imageComparer },
                { "image/jpeg", imageComparer },
                { "image/tiff", imageComparer },
                { "default", defaultComparer }
            };
        }

        public ResultComparerFactory(Dictionary<string, IResultComparer> comparers) : this()
        {
            foreach(var comparer in comparers)
            {
                _comparers[comparer.Key] = comparer.Value;
            }
        }

        public IResultComparer GetByContentType(string contentType)
        {
            if (!_comparers.ContainsKey(contentType))
            {
                return _comparers["default"];
            }

            return _comparers[contentType];
        }
    }
}