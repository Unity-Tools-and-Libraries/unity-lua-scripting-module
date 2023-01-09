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
                Assert.AreEqual("attempt to index a nil value at 'foo.var'", ex.Message);
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
                Assert.AreEqual("attempt to index a nil value at 'foo.var'", ex.Message);
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
                Scripting.EvaluateStringAsScript("foo()");
            });
            Assert.AreEqual("Attempted to call a nil value near 'foo'", thrown.Message);
        }
    }
}