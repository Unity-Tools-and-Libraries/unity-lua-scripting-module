using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WattleScript.Interpreter;
using WattleScript.Interpreter.Debugging;

public class BasicDebugger : IDebugger
{
    private Dictionary<string, HashSet<int>> breakpoints = new Dictionary<string, HashSet<int>>();
    private Dictionary<string, Dictionary<int, Action<BasicDebugger>>> breakpointActions = new Dictionary<string, Dictionary<int, Action<BasicDebugger>>>();
    public bool enabled = true;

    private Dictionary<string, SourceCode> sources = new Dictionary<string, SourceCode>();
    
    private DebugService DebugService;

    private SourceCode currentSourceCode;

    /**
     * Event for whenever the debugger is paused.
     */
    public event Action<int, string> OnPause;

    public void AddBreakpoint(string sourceName, int lineNumber, Action<BasicDebugger> actionOnHit = null)
    {
        var source = sources.GetValueOrDefault(sourceName);
        if (source != null)
        {
            int lineOffset = source.Refs[0].FromLine;
            var bps = breakpoints.GetValueOrDefault(sourceName);
            if(bps == null)
            {
                bps = breakpoints[sourceName] = new HashSet<int>();
            }
            bps.Add(lineNumber + lineOffset - 1);
            DebugService.ResetBreakPoints(source, bps);
        }

        if (actionOnHit != null)
        {
            var actions = breakpointActions.GetValueOrDefault(sourceName);
            if (actions == null)
            {
                actions = new Dictionary<int, Action<BasicDebugger>>();
                breakpointActions[sourceName] = actions;
            }
            actions[lineNumber] = actionOnHit;
        }
    }

    private bool shouldPause;

    public DebuggerAction GetAction(int ip, SourceRef sourceref)
    {
        if (enabled)
        {
            bool wasPaused = shouldPause;
            shouldPause = sourceref != null && sourceref.Breakpoint;
            var source = sources.GetOrDefault("");
            if (ip == 0 && source != null)
            {
                DebugService.ResetBreakPoints(source, breakpoints[""]);
            }
            if(shouldPause)
            {
                if (!wasPaused)
                {
                    var actions = breakpointActions.GetValueOrDefault(currentSourceCode.Name);
                    if (actions != null)
                    {
                        foreach (var action in actions)
                        {
                            action.Value(this);
                        }
                    }
                }
                OnPause?.Invoke(sourceref.FromLine, currentSourceCode.Code);
                return new DebuggerAction()
                {
                    Action = DebuggerAction.ActionType.None
                };
            }

            return new DebuggerAction()
            {
                Action = DebuggerAction.ActionType.Run
            };
        } else
        {
            shouldPause = false;
            return new DebuggerAction()
            {
                Action = DebuggerAction.ActionType.Run
            };
        }
        
    }

    public DebuggerCaps GetDebuggerCaps()
    {
        return DebuggerCaps.CanDebugSourceCode |
            DebuggerCaps.HasLineBasedBreakpoints;
    }

    public List<DynamicExpression> GetWatchItems()
    {
        return new List<DynamicExpression>();
    }

    public bool IsPauseRequested()
    {
        return enabled;
    }

    public void RefreshBreakpoints(IEnumerable<SourceRef> refs)
    {
        throw new System.NotImplementedException();
    }

    public void SetByteCode(string[] byteCode)
    {
        throw new System.NotImplementedException();
    }

    public void SetDebugService(DebugService debugService)
    {
        DebugService = debugService;
    }

    public void SetSourceCode(SourceCode sourceCode)
    {
        sources[sourceCode.Name] = sourceCode;
        currentSourceCode = sourceCode;

        var existingBreakpoints = breakpoints.GetValueOrDefault(currentSourceCode.Name);

        if(existingBreakpoints != null)
        {
            DebugService.ResetBreakPoints(currentSourceCode, existingBreakpoints);
        }
    }

    public void SignalExecutionEnded()
    {
        
    }

    public bool SignalRuntimeException(ScriptRuntimeException ex)
    {
        throw new System.NotImplementedException();
    }

    public void Update(WatchType watchType, IEnumerable<WatchItem> items)
    {
        
    }
}
