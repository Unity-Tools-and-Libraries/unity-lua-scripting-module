using System.Collections;
using System.Collections.Generic;
using System.Threading;
using io.github.thisisnozaku.scripting;
using NUnit.Framework;
using UnityEngine;
using WattleScript.Interpreter.Debugging;

namespace io.github.thisisnozaku.scripting.debugging
{
    public class ScriptingDebuggingTests
    {
        private ScriptingModule scripting = new ScriptingModule();
        private BasicDebugger debugger;

        [SetUp]
        public void Setup()
        {
            debugger = new BasicDebugger();
        }

        [Test]
        public void CanAttachDebugger()
        {
            scripting.AttachDebugger(new BasicDebugger());
        }

        [Test]
        public void DebuggerWillPauseOnBreakpointLine()
        {

            scripting.AttachDebugger(debugger);

            bool breakpointHit = false;
            var exec = scripting.LoadString(@"
local foo = 1
return foo", "foobar");
            debugger.AddBreakpoint("foobar", 1, (db) =>
            {
                db.enabled = false;
                breakpointHit = true;
            });
            scripting.Evaluate(exec);
            Assert.IsTrue(breakpointHit);
        }
    }
}