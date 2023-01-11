using io.github.thisisnozaku.scripting.context;
using io.github.thisisnozaku.scripting.types;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger.DebuggerLogic;
using System;
using System.Collections;
using System.Collections.Generic;

namespace io.github.thisisnozaku.scripting
{
    public class ScriptingModule
    {
        internal Script script;
        public Table Globals => script.Globals;
        /*
         * Function to customize the context that will be used by a script execution.
         */
        public Func<Table, Table> ContextCustomizer { get; set; }

        public ScriptingModule(ScriptingModuleConfigurationFlag configurationFlags = 0)
        {
            script = new Script(CoreModules.Preset_HardSandbox | CoreModules.LoadMethods | CoreModules.Metatables);
            Configure(configurationFlags);
        }

        private void Configure(ScriptingModuleConfigurationFlag configurationFlags)
        {
            if ((configurationFlags & ScriptingModuleConfigurationFlag.DICTIONARY_WRAPPING) != 0)
            {
                UserData.RegisterType<WrappedDictionary>();
                AddTypeAdapter(new TypeAdapter<IDictionary>.AdapterBuilder<IDictionary>()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());

                AddTypeAdapter(new TypeAdapter<IDictionary<string, object>>.AdapterBuilder<IDictionary<string, object>>()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());
                AddTypeAdapter(new TypeAdapter<Dictionary<string, object>>.AdapterBuilder<Dictionary<string, object>>()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());

                AddTypeAdapter(new TypeAdapter<IDictionary<int, object>>.AdapterBuilder<IDictionary<int, object>>()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());
                AddTypeAdapter(new TypeAdapter<Dictionary<int, object>>.AdapterBuilder<Dictionary<int, object>>()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());

                AddTypeAdapter(new TypeAdapter<IDictionary<long, object>>.AdapterBuilder<IDictionary<long, object>>()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());
                AddTypeAdapter(new TypeAdapter<Dictionary<long, object>>.AdapterBuilder<Dictionary<long, object>>()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());
            }
        }

        public void AddTypeAdapter<T>(TypeAdapter<T> typeAdapter)
        {
            UserData.RegisterType<T>();
            if (Script.GlobalOptions.CustomConverters.GetClrToScriptCustomConversion(typeof(T)) != null)
            {
                throw new InvalidOperationException(string.Format("There is already a custom conversion for type {0}", typeof(T)));
            }
            if (typeAdapter.ClrToScriptConverter != null)
            {
                Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<T>(typeAdapter.ClrToScriptConverter);
            }
            foreach(var conversion in typeAdapter.ScriptToClrConverters)
            {
                Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(conversion.Key, typeof(T), conversion.Value);
            }
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
            Table contextTable = script.Globals;
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
                contextTable = newContext;
            }
            if(ContextCustomizer != null)
            {
                contextTable = ContextCustomizer(contextTable);
            }
            return contextTable;
        }
    }
}