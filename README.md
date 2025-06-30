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

VP.NET.GUI
=======================
![VP.NET.GUI](https://i.imgur.com/TTgdFiJ.png)

GUI Program for the VP.NET lib, supports handling .VP and .VPC files


Current Freatures:<br />
-Loading Multiple VPs and VPC<br />
-Viewing VPs/VPC and navigate the folder structure<br />
-Create new VP/VPCs<br />
-Extracting files from regular vp and vpc<br />
-Compressing and decompressing VP/VPC<br />
-Compressing and decompression .lz41 files<br />
-Modify VP/VPCs<br />
-Enable/Disable VP compression<br />
-Image file previewing supporting: DDS, PCX, TGA, JPG<br />
-Animation previewing supporting: GIF and APNG<br />
-Multimedia previewing supporting: WAV, MP3, OGG, MP4 via LibVLCSharp<br />
-Text previewing supporting: LUA, TBM, TBL, EFF, FS2, FC2<br />
-Supporting for linking external file viewers<br />
<br />
Feature complete as of 0.9.0-Beta<br />
<br />
<br />

Note for developers
=======================
LibVLC libs are only included for Windows on debug builds, for debug builds it is needed to manually copy the dlls to the output directory.<br />
Linux always need to have both vlc and libvlc-dev packages<br />