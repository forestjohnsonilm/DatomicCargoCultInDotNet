using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DatomicNet.Core
{
    public interface ISchemaConfiguration
    {
        IReadOnlyList<BaseTypeRegistration> RegisterTypes { get; }
    }
}