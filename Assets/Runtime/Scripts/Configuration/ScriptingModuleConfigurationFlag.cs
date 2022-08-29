using System;

[Flags]
public enum ScriptingModuleConfigurationFlag
{
    /** Wrap dictionaries in a UserData instead of converting into a new table when entering the scripting 
     * ecosystem. This allows mutating a dictionary within a script.
     */
    DICTIONARY_WRAPPING = 0b00000001
}
