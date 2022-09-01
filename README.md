# unity-simple-logging
A module to wrap and extends the Moonsharp Lua scripting engine.

## Adding to your project via Unity Package Manager
Open **Window > Package Manager** in your editor.

Click the **+** in the top left and select **Add package from git URL...**

Paste *https://github.com/ThisIsNoZaku/unity-lua-scripting-module.git?path=Assets* and click **Add**.

## Usage
Create a using statement for `io.github.thisisnozaku.scripting`.
Create the module with `new ScriptingModule()`

## Using The Scripting Module
For details on how to script in Lua, search online for tutorials.

For details on Moonshpar, details and tutorials are available at [https://www.moonsharp.org/](https://www.moonsharp.org/).

Call `EvaluateStringAsScript`, passing in the string to run the script using the global table as the context.

The `Evaluate` method takes a Moonsharp `DynValue` wrapping either a string, which is evaluated as a script, or a callback around a C# function, which calls that function and passes the context as the sole argument. 

You may pass in an instance of `IDictionary<string, object>`, `KeyValuePair<string, object>` or `Tuple<string, object>` as the optional second argument. This will place the value(s) into the context of the script, making them accessible by name.

The module uses the class `TypeAdapter` to bundle configuration for interop between the C# domain and Lua. It's usage is optional.
