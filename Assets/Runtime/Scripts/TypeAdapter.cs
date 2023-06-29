using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattleScript.Interpreter;
using WattleScript.Interpreter.Interop;

namespace io.github.thisisnozaku.scripting.types
{
    /*
     * Packages some definitions to support interop between an application domain type and the scripting engine.
     */
    public class TypeAdapter<T>
    {
        public Func<Script, T, DynValue> ClrToScriptConverter { get; }
        public Dictionary<DataType, Func<DynValue, object>> ScriptToClrConverters { get; }
        public IUserDataDescriptor DataDescriptor;

        public TypeAdapter(Func<Script, T, DynValue> ClrConverter, Dictionary<DataType, Func<DynValue, object>> scriptToClrConverter, IUserDataDescriptor DataDescriptor)
        {
            ClrToScriptConverter = ClrConverter;
            ScriptToClrConverters = scriptToClrConverter;
            this.DataDescriptor = DataDescriptor;
        }


        public class AdapterBuilder
        {
            private Func<Script, T, DynValue> ClrToScriptConverter;
            private Dictionary<DataType, Func<DynValue, object>> ScriptToClrConverters = new Dictionary<DataType, Func<DynValue, object>>();
            private IUserDataDescriptor DataDescriptor;

            public TypeAdapter<T> Build()
            {
                return new TypeAdapter<T>(ClrToScriptConverter, ScriptToClrConverters, DataDescriptor);
            }

            public AdapterBuilder WithClrConversion(Func<Script, T, DynValue> ClrToScriptConverter)
            {
                this.ClrToScriptConverter = ClrToScriptConverter;
                return this;
            }

            public AdapterBuilder WithScriptConversion(DataType scriptType, Func<DynValue, object> ScriptToClrConverter)
            {
                this.ScriptToClrConverters[scriptType] = ScriptToClrConverter;
                return this;
            }

            public AdapterBuilder WithDataDescriptor(IUserDataDescriptor descriptor)
            {
                this.DataDescriptor = descriptor;
                return this;
            }

        }
    }
}