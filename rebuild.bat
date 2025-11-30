@set OLDCD=%CD%
@cd %~dp0

@call SimpleHttpServer\rebuild.bat

@cd %OLDCD%
