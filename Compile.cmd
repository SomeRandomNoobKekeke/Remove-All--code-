echo off 

SET CompileTo=Remove All
SET ModAssemblyName=RemoveAll
SET ModRootNamespace=RemoveAll

SET CompilerDir="../[ Compiler ]/"
SET ThisFolder="%cd%/"

@REM https://stackoverflow.com/a/60046276
for %%I in ("%~dp0.") do for %%J in ("%%~dpI.") do set ParentFolderName=%%~dpnxJ
@REM echo %ParentFolderName%

SET ModDeployDir="%ParentFolderName%/%CompileTo%/"

cd %CompilerDir%
dotnet build .\Compiler.sln -c Release -p WarningLevel=0 -p:ModAssemblyName=%ModAssemblyName% -p:ModRootNamespace=%ModRootNamespace% -p:SourceModDir=%ThisFolder% -p:ModDeployDir=%ModDeployDir%

pause
