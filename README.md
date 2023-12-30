C# .NET 6 library to handle .VP Volition file container
=======================
VP.NET is a .NET 6.0 library to handle VP containers with built-in 
LZ4 compression support provided by modified version of the 3rd party IonKiwi.lz4 lib.

Work on this lib is mainly intended to support the Knossos.NET project. But a GUI is in the works 
to provide a multi-platform GUI VP viewer and creator. The GUI will be in for the next version.

How to use
----------------------------
**Examples**

The examples has been written on a console test app available here:<br />
https://github.com/Shivansps/VP.NET/blob/main/VP.NET.Examples/Program.cs

<br />
<br />
# VP.NET.GUI<br />
![VP.NET.GUI](https://i.imgur.com/Seau5Jv.png)
<br />
GUI Program for the VP.NET lib, supports handling .VP and .VPC files
<br />
<br />
Current Freatures:<br />
-Loading Multiple VPs and VPC<br />
-Viewing VPs/VPC and navigate the folder structure<br />
-Extracting files from regular vp and vpc<br />
-Compressing and decompressing VP/VPC<br />
-Comrpessing and decompression .lz41 files<br />