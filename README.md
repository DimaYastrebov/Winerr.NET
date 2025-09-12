# Winerr.NET

> A powerful and accurate OS error window generator built with .NET.

To get started, go to the [Releases page](https://github.com/DimaYastrebov/Winerr.NET/releases) and download the application archive for your system's architecture (e.g., `win-x64`, `win-x86`). Unpack it and use your terminal to work with `Winerr.NET.Cli.exe`.

Example generation command. Use `\n` for a line break. To display the literal text `\n`, escape it: `\\n`.
```shell
Winerr.NET.Cli.exe generate --style "Win7_Aero" --title "Winerr.NET Example" --content "This is an example window generated with Winerr.NET.\n\nThis example uses a double line break." --icon 48 --output "example.png" --buttons "[{\"Text\":\"OK\",\"Type\":\"Recommended\",\"Mnemonic\":true},{\"Text\":\"Cancel\",\"Type\":\"Default\"}]"
```

For a full list of commands and parameters, use the `--help` flag.
The list of available styles can be found using the `list-styles` command or on **the project's Wiki**.
