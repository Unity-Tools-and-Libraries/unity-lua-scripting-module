using System;
using System.Collections;
using System.Collections.Generic;
using io.github.thisisnozaku.scripting;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using NUnit.Framework;
using UnityEngine;
using static io.github.thisisnozaku.scripting.TestingScripts;
namespace io.github.thisisnozaku.scripting
{
    public class TypeAdapterTests
    {
        private ScriptingModule Scripting;
        [SetUp]
        public void setup()
        {
            Scripting = new ScriptingModule(ScriptingModuleConfigurationFlag.DICTIONARY_WRAPPING);
        }

        [TearDown]
        public void cleanup()
        {
            Script.GlobalOptions.CustomConverters.Clear();
        }
        [Test]
        public void TypeAdaptersRegisterTheirTypeWithUserData()
        {
            Scripting.AddTypeAdapter(new types.TypeAdapter<TestType>.AdapterBuilder().Build());
            Assert.DoesNotThrow(() =>
            {
                Scripting.EvaluateStringAsScript("print('hello world')", Tuple.Create<string, object>("", new TestType()));
            });
        }

        [Test]
        public void TypeAdaptersCanDefineAClrObjectToScriptTypeConverter()
        {
            Scripting.AddTypeAdapter(new types.TypeAdapter<TestType>.AdapterBuilder()
                .WithClrConversion((script, obj) => DynValue.FromObject(script, 1))
                .Build());
            Assert.DoesNotThrow(() =>
            {
                Scripting.EvaluateStringAsScript("print('hello world')", Tuple.Create<string, object>("", new TestType()));
            });
        }

        [Test]
        public void TypeAdaptersCanDefineAScriptObjectToClrTypeConverter()
        {
            Scripting.AddTypeAdapter(new types.TypeAdapter<TestType>.AdapterBuilder()
                .WithScriptConversion(DataType.Number, (val) => new TestType())
                .Build());
            Assert.DoesNotThrow(() =>
            {
                Assert.AreEqual(new TestType(), Scripting.EvaluateStringAsScript("return foo", Tuple.Create<string, object>("foo", DynValue.NewNumber(1))).ToObject<TestType>());
            });
        }

        [Test]
        public void TypeAdaptersCanDefineMetaMethods()
        {
            Scripting.AddTypeAdapter(new types.TypeAdapter<TestType>.AdapterBuilder()
                .WithScriptConversion(DataType.Number, (val) => new TestType())
                .WithDataDescriptor(new StandardUserDataDescriptor(typeof(TestType), InteropAccessMode.Default))
                .Build());

            Assert.AreEqual(new TestType(2), Scripting.EvaluateStringAsScript("return foo + bar", new Dictionary<string, object>()
            {
                { "foo", new TestType(1) },
                { "bar", new TestType(1) }
            }).ToObject());
        }
    }
}