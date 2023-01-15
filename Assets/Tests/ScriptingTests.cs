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
        public void TypeAdaptersCanDefineAScriptObjectToClrTypeConverter()
        {
            Scripting.AddTypeAdapter(new types.TypeAdapter<TestType>.AdapterBuilder<TestType>()
                .WithScriptConversion(DataType.Number, (val) => new TestType())
                .Build());
            Assert.DoesNotThrow(() =>
            {
                Assert.AreEqual(new TestType(), Scripting.EvaluateStringAsScript("return foo", Tuple.Create<string, object>("foo", DynValue.NewNumber(1))).ToObject<TestType>());
            });
        }

        [Test]
        public void FunctionArgumentsPopulateFromContextByName()
        {
            Scripting.EvaluateStringAsScript("function baz(foo, bar) return foo + bar end");

            Assert.AreEqual(3, Scripting.EvaluateStringAsScript("return baz(foo, bar)", new Dictionary<string, object>()
            {
                { "foo", 1 },
                { "bar", 2 }
            }).Number);
            Assert.AreEqual(3, Scripting.Evaluate(DynValue.FromObject(null, Scripting.Globals["baz"]), new Dictionary<string, object>()
            {
                { "foo", 1 },
                { "bar", 2 }
            }, new List<string>() { "foo", "bar" }).Number);
        }

        [Test]
        public void ClrFunctionCanReceiveContextVariable()
        {
            bool called = false;
            Scripting.Globals["foo"] = DynValue.FromObject(null, (Action<Table>)((ctx) => {
                called = true;
                Assert.AreEqual(1, ctx["bar"]);
                Assert.AreEqual(2, ctx["baz"]);
            }));

            Scripting.Evaluate(Scripting.Globals.Get("foo"), new Dictionary<string, object>()
                {
                    { "bar", 1 },
                    { "baz", 2 }
                });
            Assert.IsTrue(called);
        }

        [Test]
        public void ClrFunctionArgumentsCanPopulateFromContextByIndex()
        {
            bool called = false;
            Scripting.Globals["foo"] = DynValue.FromObject(null, (Action<object, object>)((baz, bar) => {
                called = true;
                Assert.AreEqual(1, bar);
                Assert.AreEqual(2, baz);
            }));

            Scripting.Evaluate(Scripting.Globals.Get("foo"), new Dictionary<string, object>()
                {
                    { "bar", 1 },
                    { "baz", 2 }
                }, new List<string>() { "baz", "bar" });
            Assert.IsTrue(called);
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