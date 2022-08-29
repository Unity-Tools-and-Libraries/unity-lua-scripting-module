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
        public Type TypeToRegister;
        public Func<Script, object, DynValue> ClrToScriptConversion { get; }

        public TypeAdapter(Type typeToRegister, Func<Script, object, DynValue> ClrConverter)
        {
            this.TypeToRegister = typeToRegister;
            ClrToScriptConversion = ClrConverter;
        }


        public class AdapterBuilder
        {
            private Func<Script, object, DynValue> ClrToScriptConversion;
            private Type[] TypesToRegister;

            public AdapterBuilder WithClrConversion(Func<Script, object, DynValue> ClrToScriptConverter)
            {
                ClrToScriptConversion = ClrToScriptConverter;
                return this;
            }

            public TypeAdapter Build(Type forType)
            {
                return new TypeAdapter(forType, ClrToScriptConversion);
            }
        }
    }
}