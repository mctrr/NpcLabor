You are not using the latest version of the tool, please update.
ilspycmd : System.InvalidOperationException: Could not find type definition TraitOre in type system.
At line:2 char:1
+ ilspycmd "E:\SteamLibrary\steamapps\common\Elin\Elin_Data\Managed\Eli ...
+ ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : NotSpecified: (System.InvalidO...in type system.:String) [], RemoteException
    + FullyQualifiedErrorId : NativeCommandError
 
   at ICSharpCode.Decompiler.CSharp.CSharpDecompiler.DecompileType(FullTypeName fullTypeName) in /_/ICSharpCode.Decompiler/CSharp/CSharpDecompiler.cs:line 
977
   at ICSharpCode.Decompiler.CSharp.CSharpDecompiler.DecompileTypeAsString(FullTypeName fullTypeName) in /_/ICSharpCode.Decompiler/CSharp/CSharpDecompiler.
cs:line 997
   at ICSharpCode.ILSpyCmd.ILSpyCmdProgram.Decompile(String assemblyFileName, TextWriter output, String typeName) in D:\a\ILSpy\ILSpy\ICSharpCode.ILSpyCmd\
IlspyCmdProgram.cs:line 333
   at ICSharpCode.ILSpyCmd.ILSpyCmdProgram.<OnExecuteAsync>g__PerformPerFileAction|53_0(String fileName, <>c__DisplayClass53_0& ) in D:\a\ILSpy\ILSpy\ICSha
rpCode.ILSpyCmd\IlspyCmdProgram.cs:line 245
   at ICSharpCode.ILSpyCmd.ILSpyCmdProgram.OnExecuteAsync(CommandLineApplication app) in D:\a\ILSpy\ILSpy\ICSharpCode.ILSpyCmd\IlspyCmdProgram.cs:line 166
Latest version is '10.1.1.8388' (yours is '8.2.0.7535-95108c96')
