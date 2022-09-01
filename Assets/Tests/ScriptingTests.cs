using System;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using NUnit.Framework;

namespace io.github.thisisnozaku.scripting
{
    public class TestingScripts
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
        public void CanAssignBasicValue()
        {

            Scripting.EvaluateStringAsScript("foo = 1;");
            Assert.AreEqual(1, Scripting.Globals.Get("foo").Number);
        }

        [Test]
        public void CanGetValue()
        {
            Scripting.Globals["foo"] = 1;
            Assert.AreEqual(1, Scripting.EvaluateStringAsScript("return foo").Number);
        }

        [Test]
        public void WhenADictionaryGoesIntoScriptWrapIt()
        {
            var dict = new Dictionary<string, object>();
            var b = dict is IDictionary || dict is IDictionary<string, object>;
            Scripting.Globals["foo"] = new Dictionary<string, object>();
            Assert.AreEqual(DataType.UserData, Scripting.Globals.Get("foo").Type);
        }

        [Test]
        public void CanAssignValueInTable()
        {
            Scripting.Globals["foo"] = new Dictionary<string, object>();
            Scripting.EvaluateStringAsScript("foo.bar = 1");
            Assert.AreEqual(1, Scripting.EvaluateStringAsScript("return foo.bar").Number);
        }


        [Test]
        public void ContextCanBeASingleKeyValuePair()
        {
            Assert.IsTrue(Scripting.EvaluateStringAsScript("return value", new KeyValuePair<string, object>("value", true)).Boolean);
        }

        [Test]
        public void ErrorFunctionInScriptThrows()
        {
            Assert.Throws(typeof(ScriptRuntimeException), () =>
            {
                Scripting.EvaluateStringAsScript("error('foo')");
            });
        }

        [Test]
        public void TryingToEvaluateNullForScriptThrows()
        {
            Assert.Throws(typeof(ArgumentNullException), () =>
            {
                Scripting.EvaluateStringAsScript(null);
            });
        }

        [Test]
        public void TryingToEvaluateNullObjectThrows()
        {
            Assert.Throws(typeof(ArgumentNullException), () =>
            {
                Scripting.Evaluate(null);
            });
        }

        [Test]
        public void TryingToEvaluateWrongTypeThrows()
        {
            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                Scripting.Evaluate(DynValue.Nil);
            });
        }

        [Test]
        public void CallbackReceivedContextDictionaryAsArg()
        {
            var context = new Dictionary<string, object>();
            Scripting.Evaluate(DynValue.FromObject(null, (Action<IDictionary<string, object>>)(ctx =>
            {
                Assert.IsTrue(ctx is Dictionary<string, object>);
            })), context);
        }

        [Test]
        public void TryingToInsertIntoNonTableThrows()
        {
            var table = false;
            Assert.Throws(typeof(ScriptRuntimeException), () =>
            {
                Scripting.EvaluateStringAsScript("table.insert(tbl, 'foo')", new Dictionary<string, object>() {
                    { "tbl", table }
                    });
            });
        }

        [Test]
        public void CanConfigureTheMetatableUsedByContext()
        {
            Scripting.ContextCustomizer = (ctx) =>
            {
                ctx.MetaTable = new Table(null);
                ctx.MetaTable.Set("__index", DynValue.NewCallback((ctx, args) => {
                    return DynValue.NewNumber(1);
                }));
                return ctx;
            };
            Assert.AreEqual(1, Scripting.EvaluateStringAsScript("return foo").Number);
        }

        [Test]
        public void TypeAdaptersRegisterTheirTypeWithUserData()
        {
            Scripting.AddTypeAdapter(new types.TypeAdapter<TestType>.AdapterBuilder<TestType>().Build());
            Assert.DoesNotThrow(() =>
            {
                Scripting.EvaluateStringAsScript("print('hello world')", Tuple.Create<string, object>("", new TestType()));
            });
        }

        [Test]
        public void TypeAdaptersCanDefineAClrObjectToScriptTypeConverter()
        {
            Scripting.AddTypeAdapter(new types.TypeAdapter<TestType>.AdapterBuilder<TestType>()
                .WithClrConversion((script, obj) => DynValue.FromObject(script, 1))
                .Build());
            Assert.DoesNotThrow(() =>
            {
                Scripting.EvaluateStringAsScript("print('hello world')", Tuple.Create<string, object>("", new TestType()));
            });
        }

        [Test]
        public void TestAdaptersCanDefineAScriptObjectToClrTypeConverter()
        {
            Scripting.AddTypeAdapter(new types.TypeAdapter<TestType>.AdapterBuilder<TestType>()
                .WithScriptConversion(DataType.Number, (val) => new TestType())
                .Build());
            Assert.DoesNotThrow(() =>
            {
                Assert.AreEqual(new TestType(), Scripting.EvaluateStringAsScript("return foo", Tuple.Create<string, object>("foo", DynValue.NewNumber(1))).ToObject<TestType>());
            });
        }

        public class TestType
        {
            public override bool Equals(object obj)
            {
                return obj.GetType() == GetType();
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}