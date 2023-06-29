using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattleScript.Interpreter;

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
        public void TryingToEvaluateWrongTypeThrows()
        {
            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                Scripting.Evaluate(DynValue.Nil);
            });
        }
    }
}