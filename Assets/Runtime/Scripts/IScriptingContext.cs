using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace io.github.thisisnozaku.scripting.context
{
    /*
     * Interface for an object which can serve as the context for a script.
     */
    public interface IScriptingContext
    {
        Dictionary<string, object> GetContextVariables();
    }
}