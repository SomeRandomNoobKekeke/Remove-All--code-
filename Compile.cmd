@REM Put this in your "in memory" mod folder
echo off 

@REM Set those vars
@REM Note: it's space sensitive
SET CompileTo=Remove All [compiled]
SET ModAssemblyName=RemoveAll
SET ModRootNamespace=RemoveAll

@REM Folder paths should end in /
@REM Path to compiler, i store it in LocalMods/[ Compiler ]
SET CompilerDir="../[ Compiler ]/"
@REM This directory
SET SourceModDir="%cd%/"

@REM https://stackoverflow.com/a/60046276
for %%I in ("%~dp0.") do for %%J in ("%%~dpI.") do set ParentFolder=%%~dpnxJ
@REM echo %ParentFolder%

SET ModDeployDir="%ParentFolder%/%CompileTo%/"

@REM you can set -p WarningLevel=4 and remove /clp:ErrorsOnly if you like warnings
@REM -maxcpucount:1 disables building multiple projects in parallel, on my pc they constanly block each other
cd %CompilerDir%
dotnet build .\Compiler.sln /clp:ErrorsOnly -p WarningLevel=0 -maxcpucount:1 -p:ModAssemblyName=%ModAssemblyName% -p:ModRootNamespace=%ModRootNamespace% -p:SourceModDir=%SourceModDir% -p:ModDeployDir=%ModDeployDir%

pause
