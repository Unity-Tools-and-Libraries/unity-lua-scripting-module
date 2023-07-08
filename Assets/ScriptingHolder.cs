using System.Collections;
using System.Collections.Generic;
using io.github.thisisnozaku.scripting;
using UnityEngine;

public class ScriptingHolder : MonoBehaviour, IScriptingHost
{
    public ScriptingModule ScriptingModule { get; private set; }
    private BasicDebugger debugger;
    [TextArea]
    public string script;

    public bool execute;
    // Start is called before the first frame update
    void Awake()
    {
        ScriptingModule = new ScriptingModule();
    }

    // Update is called once per frame
    void Update()
    {
        if(execute)
        {
            ScriptingModule.EvaluateStringAsScript(script);
            execute = false;
        }
    }

    void Execute()
    {

    }
}
