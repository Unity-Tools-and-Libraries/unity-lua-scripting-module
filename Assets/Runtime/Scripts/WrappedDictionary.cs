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
            foreach (var key in underlying.Keys)
            {
                var hash = key.GetHashCode();
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

        private Func<DynValue> NextFunction(Script script, IDictionaryEnumerator enumerator, bool numericOnly = false)
        {
            Func<DynValue> next = null;
            next = () =>
            {
                while (enumerator.MoveNext())
                {
                    int parsed = 0;
                    if (!numericOnly || (enumerator.Entry.Key is string && int.TryParse(enumerator.Entry.Key as string, out parsed)))
                    {
                        return DynValue.NewTuple(//DynValue.FromObject(script, next),
                            DynValue.FromObject(script, enumerator.Entry.Key),
                            DynValue.FromObject(script, enumerator.Entry.Value));
                    }
                }
                return DynValue.Nil;
            };
            return next;
        }

        public DynValue MetaIndex(Script script, string metaname)
        {
            switch (metaname)
            {
                case "__pairs":
                    {
                        IDictionaryEnumerator enumerator = underlying.GetEnumerator();
                        return DynValue.FromObject(script, (Func<Func<DynValue>>)(() => NextFunction(script, enumerator)));
                    }
                case "__ipairs":
                    {
                        IDictionaryEnumerator enumerator = underlying.GetEnumerator();
                        return DynValue.FromObject(script, (Func<Func<DynValue>>)(() => NextFunction(script, enumerator, true)));
                    }
                default:
                    throw new NotImplementedException(metaname);
            }

        }

        public bool SetIndex(Script script, DynValue index, DynValue value, bool isDirectIndexing)
        {
            underlying[index.ToObject()] = value.ToObject();
            resolutionCache[index.ToObject().GetHashCode()] = value.ToObject();
            return true;
        }
    }
}