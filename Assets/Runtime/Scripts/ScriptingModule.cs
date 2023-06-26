using io.github.thisisnozaku.scripting.context;
using io.github.thisisnozaku.scripting.types;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
                AddTypeAdapter(new TypeAdapter<IDictionary>.AdapterBuilder()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());

                AddTypeAdapter(new TypeAdapter<IDictionary<string, object>>.AdapterBuilder()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());
                AddTypeAdapter(new TypeAdapter<Dictionary<string, object>>.AdapterBuilder()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());

                AddTypeAdapter(new TypeAdapter<IDictionary<int, object>>.AdapterBuilder()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());
                AddTypeAdapter(new TypeAdapter<Dictionary<int, object>>.AdapterBuilder()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());

                AddTypeAdapter(new TypeAdapter<IDictionary<long, object>>.AdapterBuilder()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());
                AddTypeAdapter(new TypeAdapter<Dictionary<long, object>>.AdapterBuilder()
                    .WithClrConversion(DictionaryTypeAdapter.Converter).Build());
            }
        }

        public void AddTypeAdapter<T>(TypeAdapter<T> typeAdapter)
        {
            UserData.UnregisterType<T>();
            UserData.RegisterType<T>(typeAdapter.DataDescriptor);
            if (Script.GlobalOptions.CustomConverters.GetClrToScriptCustomConversion(typeof(T)) != null)
            {
                UnityEngine.Debug.LogWarning("Overriding a custom conversion for " + typeof(T));
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

        /**
         * Loads the given script and returns a DynValue which can be evaluated
         * to invoke the script.
         * 
         * Use this to 
         */
        public DynValue LoadString(string script)
        {
            if (script == null)
            {
                throw new ArgumentNullException("script");
            }
            return this.script.LoadString(script);
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

        public DynValue Evaluate(DynValue toEvaluate, IScriptingContext context, List<string> argumentContextMap = null)
        {
            return Evaluate(toEvaluate, context.GetContextVariables(), argumentContextMap);
        }

        public DynValue Evaluate(DynValue toEvaluate, IDictionary<string, object> localContext = null, List<string> argumentContextMap = null)
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
                    {
                        if(argumentContextMap != null)
                        {
                            var args = MapArguments(localContext, argumentContextMap);
                            result = script.Call(toEvaluate.Callback, args);
                        } else
                        {
                            result = script.Call(toEvaluate, SetupContext(localContext));
                        }
                        break;
                    }
                case DataType.Function:
                    var arguments = MapArguments(localContext, argumentContextMap);
                    result = script.Call(toEvaluate.Function, arguments);
                    break;
                default:
                    throw new InvalidOperationException(String.Format("The DynValue must contains a string to interpret or a function to call, but was {0}", toEvaluate.Type));
            }
            return result;
        }

        private object[] MapArguments(IDictionary<string, object> context, List<string> argumentContextMap)
        {
            if(argumentContextMap != null)
            {
                return argumentContextMap.Select(a => context[a]).ToArray();
            } else
            {
                List<object> arguments = new List<object>();
                if (context != null)
                {
                    foreach (var entry in context)
                    {
                        int index = -1;
                        if (!int.TryParse(entry.Key, out index))
                        {
                            throw new InvalidOperationException(string.Format("No mapping was provided from context names to argument array indexes, so tried parsing {0} as an int. ", entry.Key));
                        }
                        arguments.Insert(index, entry.Value);
                    }
                }
                return arguments.ToArray();
            }
            
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