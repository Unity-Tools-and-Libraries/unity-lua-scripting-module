using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace io.github.thisisnozaku.scripting
{
    public class WrappedDictionary : IUserDataType
    {
        private IDictionary underlying;
        private Dictionary<int, object> resolutionCache = new Dictionary<int, object>();
        public WrappedDictionary(IDictionary underlying)
        {
            this.underlying = underlying;
            foreach(var key in underlying.Keys)
            {
                var hash = DynValue.FromObject(null, key.GetHashCode()).ToObject().GetHashCode();
                resolutionCache[hash] = underlying[key];
            }
        }

        public DynValue Index(Script script, DynValue index, bool isDirectIndexing)
        {
            var indexHash = index.ToObject().GetHashCode();
            if (resolutionCache.ContainsKey(indexHash))
            {
                return DynValue.FromObject(script, resolutionCache[index.ToObject().GetHashCode()]);
            }
            return DynValue.Nil;

        }

        public DynValue MetaIndex(Script script, string metaname)
        {
            throw new NotImplementedException();
        }

        public bool SetIndex(Script script, DynValue index, DynValue value, bool isDirectIndexing)
        {
            underlying[index.ToObject()] = value.ToObject();
            resolutionCache[index.ToObject().GetHashCode()] = value.ToObject();
            return true;
        }
    }
}