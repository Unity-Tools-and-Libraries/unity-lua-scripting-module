using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattleScript.Interpreter;
using WattleScript.Interpreter.Interop;

namespace io.github.thisisnozaku.scripting
{
    public class WrappedDictionary : IUserDataType
    {
        private IDictionary underlying;

        public IDictionary Underlying => underlying;
        
        public WrappedDictionary(IDictionary underlying)
        {
            this.underlying = underlying;
        }

        public DynValue Index(Script script, DynValue index, bool isDirectIndexing)
        {
            var indexObject = index.ToObject();
            var keyType = underlying.GetType().GetGenericArguments()[0];
            if (indexObject != null && indexObject.GetType() != keyType)
            {
                throw new ScriptRuntimeException(string.Format("Tried to index a dictionary with a key type of {0} using an actual key of type {1}. To avoid unexpectedly, silently failing to find a value, indexing with the right type is strictly enforced.",
                    keyType.Name,
                    indexObject.GetType().Name));
            }
            return DynValue.FromObject(script, underlying[index.ToObject()]);
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
            return true;
        }
    }
}