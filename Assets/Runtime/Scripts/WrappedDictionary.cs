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
        public WrappedDictionary(IDictionary underlying)
        {
            this.underlying = underlying;
        }

        public DynValue Index(Script script, DynValue index, bool isDirectIndexing)
        {
            if (underlying.Contains(index.ToObject()))
            {
                return DynValue.FromObject(script, underlying[index.ToObject()]);
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
            return true;
        }
    }
}