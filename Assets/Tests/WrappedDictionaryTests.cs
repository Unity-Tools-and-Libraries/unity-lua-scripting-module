using System.Collections;
using System.Collections.Generic;
using io.github.thisisnozaku.scripting;
using MoonSharp.Interpreter;
using NUnit.Framework;
using UnityEngine;

public class WrappedDictionaryTests
{
        [Test]
        public void ThrowsWhenIndexingWithWrongType()
        {
            var te = new WrappedDictionary(new Dictionary<long, object>());

            var thrown = Assert.Throws<ScriptRuntimeException>(() =>
            {
                te.Index(null, DynValue.FromObject(null, "1"), false);
            });

            Assert.AreEqual("Tried to index a dictionary with a key type of Int64 using an actual key of type String. To avoid unexpectedly, silently failing to find a value, indexing with the right type is strictly enforced.", thrown.Message);
        }
}
