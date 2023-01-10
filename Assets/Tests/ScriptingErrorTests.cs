using MoonSharp.Interpreter;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace io.github.thisisnozaku.scripting
{
    public class ScriptingErrorTests
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
        public void IndexingANilValueThrowsAnError()
        {
            try
            {
                Scripting.EvaluateStringAsScript("return foo.var");
            }
            catch (ScriptRuntimeException ex)
            {
                Assert.AreEqual("attempt to index a nil value at 'return foo.var' from 1:0 to 1:14", ex.Message);
            }
            try
            {
                Scripting.EvaluateStringAsScript("foo = {}; foo.bar = nil; return foo.bar.baz;");

            }
            catch (ScriptRuntimeException ex)
            {
                Assert.AreEqual("attempt to index a nil value at 'return foo.bar.baz' from 1:25 to 1:43", ex.Message);
            }
        }

        [Test]
        public void ErrorMessagePointsAtOnlyWhereErrorOccurred()
        {
            try
            {
                Scripting.EvaluateStringAsScript(
                    "function add(one, two)" + "\n" +
                    "  return one + two" + "\n" +
                    "end" + "\n" + 
                    "three = add(1, 2)" + "\n" +
                    "return foo.var"
                    );
            }
            catch (ScriptRuntimeException ex)
            {
                Assert.AreEqual("attempt to index a nil value at 'return foo.var' from 5:0 to 5:14", ex.Message);
            }
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
        public void TryingToCallNonFunctionThrows()
        {
            var thrown = Assert.Throws<ScriptRuntimeException>(() =>
            {
                Scripting.EvaluateStringAsScript("return 10 * math.pwr(1, 2)");
            });
            Assert.AreEqual("Attempted to call a nil value near 'return 10 * math.pwr(1, 2)' from 1:0 to 1:26", thrown.Message);

            thrown = Assert.Throws<ScriptRuntimeException>(() =>
            {
                Scripting.EvaluateStringAsScript("return foo()");
            });
            Assert.AreEqual("Attempted to call a nil value near 'return foo()' from 1:0 to 1:12", thrown.Message);

            thrown = Assert.Throws<ScriptRuntimeException>(() =>
            {
                    Scripting.EvaluateStringAsScript("foo(bar)");
            });
            Assert.AreEqual("Attempted to call a nil value near 'foo(bar)' from 1:3 to 1:8", thrown.Message);

            thrown = Assert.Throws<ScriptRuntimeException>(() =>
            {
                Scripting.EvaluateStringAsScript("bar = 1; foo(bar, bin, bon, ban)");
            });
            Assert.AreEqual("Attempted to call a nil value near 'foo(bar, bin, bon, ban)' from 1:12 to 1:32", thrown.Message);

            
        }
    }
}