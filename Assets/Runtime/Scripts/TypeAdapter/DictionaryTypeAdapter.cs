using io.github.thisisnozaku.scripting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattleScript.Interpreter;

public class DictionaryTypeAdapter
{
    public static Func<Script, object, DynValue> Converter = (script, obj) =>
    {
        if(!(obj is IDictionary) && !(obj is IDictionary<string, object>) && !(obj is Dictionary<string, object>))
        {
            throw new InvalidOperationException();
        }
        return DynValue.FromObject(script, new WrappedDictionary(obj as IDictionary));
    };
}
