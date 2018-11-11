using Fenrir.Core.Models;
using Fenrir.Core.Models.RequestTree;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Core.Comparers
{
    public interface IResultComparer
    {
        Grade Compare(Result expected, Result actual);
    }
}
