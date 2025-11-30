@set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc
@set OLDCD=%CD%
@cd %~dp0

%CSC% /nologo /w:4 /o /out:..\SimpleHttpServer.exe ^
  /d:NETFRAMEWORK,NET20,NET35,NET40 ^
  /win32icon:favicon.ico ^
  Program.cs ^
  AssemblyInfo.cs

@cd %OLDCD%
