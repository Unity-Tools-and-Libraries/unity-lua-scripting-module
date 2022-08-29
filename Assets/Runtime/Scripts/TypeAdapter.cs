using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace io.github.thisisnozaku.scripting.types
{
    /*
     * Packages some definitions to support interop between an application domain type and the scripting engine.
     */
    public class TypeAdapter
    {
        public Type[] TypesToRegister;
        public Dictionary<Type, Func<Script, object, DynValue>> ClrToScriptConversions { get; }

        public TypeAdapter(Type[] typesToRegister, Dictionary<Type, Func<Script, object, DynValue>> ClrConverter)
        {
            this.TypesToRegister = typesToRegister;
            ClrToScriptConversions = ClrConverter;
        }


        public class AdapterBuilder
        {
            private Dictionary<Type, Func<Script, object, DynValue>> ClrToScriptConversions =  new Dictionary<Type, Func<Script, object, DynValue>>();
            private Type[] TypesToRegister;

            public AdapterBuilder WithClrConversion<T>(Func<Script, object, DynValue> ClrToScriptConverter)
            {
                ClrToScriptConversions[typeof(T)] = ClrToScriptConverter;
                return this;
            }

            public AdapterBuilder WithAdditionalRegisteredTypes(params Type[] type)
            {
                TypesToRegister = type;
                return this;
            }

            public TypeAdapter Build()
            {
                return new TypeAdapter(TypesToRegister, ClrToScriptConversions);
            }
        }
    }
}