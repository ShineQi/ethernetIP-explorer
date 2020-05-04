## EthernetIP Explorer

*  Ethernet/IP Explorer/Browser written in C#. Run on Windows & Linux with mono.
*  ODVA CIP : Common Industrial Protocol.
*  Shows devices on the local network.
*  Displays the classes, instances, attributes.
*  Decodes values using standard decoders and also ones defined by the user.
*  Write attributes.
*  Can send ForwardOpen for T->O and T->O data exchange, also with user defined decoder.
*  Full open source code.

*  Explicit & Implicit messaging basic client source codes.

## Releases & Downloads

*  [EthernetIP-Explorer 1.2.1](https://github.com/tswaehn/ethernetIP-explorer/releases/download/1.2.1/SetupEnIPExplorer_1.2.1.exe)

## screenshots

![image1](/Docs/image1.png)
![image2](/Docs/image2.png)
![image3](/Docs/image3.png)
![image4](/Docs/image4.png)
![image5](/Docs/image5.png)
![image6](/Docs/image6.png)

## Requirement for building

*  Visual Studio
*  C# Plugin
*  Inno Setup (for building the setup installer on windows)


## notes on windows building

Download SnapShot on Windows 10

A lot of files (.resx and some others) are marked block by Windows, and Visual Studio cannot build the projects.

To unblock all files, open a PowerShell window, move to the directory where all files are extracted then type the command :

gci -recurse  | Unblock-File 

---
EtherNet/IP™ is a registered trademark of ODVA, Inc.
