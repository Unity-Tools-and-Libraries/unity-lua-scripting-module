using io.github.thisisnozaku.scripting.context;
using io.github.thisisnozaku.scripting.types;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;

namespace io.github.thisisnozaku.scripting
{
    public class ScriptingModule
    {
        internal Script script;
        public Table Globals => script.Globals;

        public ScriptingModule(ScriptingModuleConfigurationFlag configurationFlags = 0)
        {
            script = new Script(CoreModules.Preset_HardSandbox);
            Configure(configurationFlags);
        }

        private void Configure(ScriptingModuleConfigurationFlag configurationFlags)
        {
            if ((configurationFlags & ScriptingModuleConfigurationFlag.DICTIONARY_WRAPPING) != 0)
            {
                UserData.RegisterType<WrappedDictionary>();
                AddTypeAdapter(new TypeAdapter.AdapterBuilder()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build(typeof(IDictionary)));
                AddTypeAdapter(new TypeAdapter.AdapterBuilder()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build(typeof(IDictionary<string, object>)));
                AddTypeAdapter(new TypeAdapter.AdapterBuilder()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build(typeof(Dictionary<string, object>)));
            }
        }

        public void AddTypeAdapter(TypeAdapter typeAdapter)
        {
            UserData.RegisterType(typeAdapter.TypeToRegister);
            if (Script.GlobalOptions.CustomConverters.GetClrToScriptCustomConversion(typeAdapter.TypeToRegister) != null)
            {
                throw new InvalidOperationException(string.Format("There is already a custom conversion for type {0}", typeAdapter.TypeToRegister));
            }
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion(typeAdapter.TypeToRegister, typeAdapter.ClrToScriptConversion);
        }

        public DynValue EvaluateStringAsScript(string script, IDictionary<string, object> localContext = null)
        {
            if (script == null)
            {
                throw new ArgumentNullException("script");
            }
            return Evaluate(DynValue.NewString(script), localContext);
        }
        public DynValue EvaluateStringAsScript(string script, KeyValuePair<string, object> localContext)
        {
            return EvaluateStringAsScript(script, new Dictionary<string, object>() {
                { localContext.Key, localContext.Value }});
        }
        public DynValue EvaluateStringAsScript(string script, Tuple<string, object> localContext)
        {
            return EvaluateStringAsScript(script, new Dictionary<string, object>() {
                { localContext.Item1, localContext.Item2 }});
        }

        public DynValue Evaluate(DynValue toEvaluate, IScriptingContext context)
        {
            return Evaluate(toEvaluate, context.GetContextVariables());
        }

        public DynValue Evaluate(DynValue toEvaluate, IDictionary<string, object> localContext = null)
        {
            if (toEvaluate == null)
            {
                throw new ArgumentNullException("valueExpression");
            }
            DynValue result;
            switch (toEvaluate.Type)
            {
                case DataType.String:
                    result = script.DoString(toEvaluate.String, SetupContext(localContext));
                    break;
                case DataType.ClrFunction:
                    result = script.Call(toEvaluate.Callback, SetupContext(localContext));
                    break;
                default:
                    throw new InvalidOperationException(String.Format("The DynValue must contains a string to interpret or a function to call, but was {0}", toEvaluate.Type));
            }
            return result;
        }

        private Table SetupContext(IDictionary<string, object> localContext)
        {
            if (localContext != null)
            {
                var newContext = new Table(script);
                if (localContext != null)
                {
                    foreach (var property in localContext)
                    {
                        newContext[property.Key] = property.Value;
                    }
                }

                foreach (var global in script.Globals.Keys)
                {
                    newContext[global] = script.Globals[global];
                }
                return newContext;
            }
            else
            {
                return null;
            }
        }
    }
}