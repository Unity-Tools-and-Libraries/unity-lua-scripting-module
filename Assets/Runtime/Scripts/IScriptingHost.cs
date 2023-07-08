using System.Collections;
using System.Collections.Generic;
using io.github.thisisnozaku.scripting;
using UnityEngine;

/**
 * Place on a MonoBehaviour that holds a ScriptingModule instance.
 */
public interface IScriptingHost
{
    ScriptingModule ScriptingModule { get; }
}
