using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattleScript.Interpreter;
using WattleScript.Interpreter.Debugging;

[RequireComponent(typeof(IScriptingHost))]
public class DebuggerComponent : MonoBehaviour
{
    public BasicDebugger debugger;

    public int[] breakpointLines;
    public bool[] breakpointEnabled;

    public string currentSource;

    public bool debuggerEnabled;

    // Start is called before the first frame update
    void Start()
    {
        var scripting = GetComponent<IScriptingHost>().ScriptingModule;
        debugger = new BasicDebugger();
        scripting.AttachDebugger(debugger);
        foreach(var line in breakpointLines)
        {
            debugger.AddBreakpoint("chunk_1", line);
        }
        debugger.OnPause += Debugger_OnPause;
        StartCoroutine(Debug());
    }

    public IEnumerator Debug()
    {
        while (true)
        {
            yield return null;
        }
    }

    private void Debugger_OnPause(int line, string source)
    {
        currentSource = source;
    }

    // Update is called once per frame
    void Update()
    {
        debugger.enabled = debuggerEnabled;
    }

    private void OnValidate()
    {
        if (Application.isPlaying && debugger != null)
        {
            var breakpoints = new HashSet<int>();
            foreach (var line in breakpointLines)
            {
                debugger.AddBreakpoint("chunk_1", line);
            }
        }
    }
}
