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
    public class TypeAdapter<T>
    {
        public Func<Script, T, DynValue> ClrToScriptConverter { get; }
        public Dictionary<DataType, Func<DynValue, object>> ScriptToClrConverters { get; }

        public TypeAdapter(Func<Script, T, DynValue> ClrConverter, Dictionary<DataType, Func<DynValue, object>> scriptToClrConverter)
        {
            ClrToScriptConverter = ClrConverter;
            ScriptToClrConverters = scriptToClrConverter;
        }


        public class AdapterBuilder<T>
        {
            private Func<Script, T, DynValue> ClrToScriptConverter;
            private Dictionary<DataType, Func<DynValue, object>> ScriptToClrConverters = new Dictionary<DataType, Func<DynValue, object>>();

            public AdapterBuilder<T> WithClrConversion(Func<Script, T, DynValue> ClrToScriptConverter)
            {
                this.ClrToScriptConverter = ClrToScriptConverter;
                return this;
            }

            public TypeAdapter<T> Build()
            {
                return new TypeAdapter<T>(ClrToScriptConverter, ScriptToClrConverters);
            }

            public AdapterBuilder<T> WithScriptConversion(DataType scriptType, Func<DynValue, object> ScriptToClrConverter)
            {
                this.ScriptToClrConverters[scriptType] = ScriptToClrConverter;
                return this;
            }

        }
    }
}