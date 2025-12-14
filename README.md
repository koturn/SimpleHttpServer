SimpleHttpServer
================

[![.NET](https://github.com/koturn/SimpleHttpServer/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/koturn/SimpleHttpServer/actions/workflows/dotnet.yml)

A simple HTTP server that can be built using the C# compiler preinstalled on Windows.

## Build

Use `rebuild.bat`.

```
> rebuild.bat
```

### Single source compile

Execute following command.
`AssemblyInfo.cs` and `favicon.ico` are not mandatory.

```
> C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc /nologo /w:4 /o /out:SimpleHttpServer.exe Program.cs
```

## Usage

Execute following command, then start listening on port 8000.

```
> SimpleHttpServer
```

If you want to use another port, execute following style.
`PORT` is a port number ranged in 1 ~ 65535.

```
> SimpleHttpServer PORT
```

### Options

Variout options are available.

| Option | Description |
|-|-|
| `-d` | Allow to generate [`/.well-known/appspecific/com.chrome.devtools.json`](https://chromium.googlesource.com/devtools/devtools-frontend/+/main/docs/ecosystem/automatic_workspace_folders.md "Chromium DevTools Ecosystem Guide - Automatic Workspace Folders"). |
| `-g` | Use "+" as host part. |
| `-h` | Show help message and exit program. |
| `-H HOST` | Use specified host as host part. |
| `-l DIR` | Use specified local directory as the root. |
| `-r DIR` | Use specified directory for the part of directory of prefix. |
| `-R` | Treat the prefix root directory as the local root directory. |
| `-t` | Use `http://+:80/Temporary_Listen_Addresses/` as prefix. |
| `-w` | Launch default web browser after starting listening. |
| `--legacy-index-page` | Generate index page written in HTML4.01. |
| `--log-format-common` | Write logs in common log format. |
| `--log-format-combined` | Write logs in combined log format. (Default) |

## LICENSE

This software is released under the MIT License, see [LICENSE](LICENSE "LICENSE").
