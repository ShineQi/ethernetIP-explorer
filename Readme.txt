Download SnapShot on Windows 10

A lot of files (.resx and some others) are marked block by Windows, and Visual Studio cannot build the projects.

To unblock all files, open a PowerShell window, move to the directory where all files are extracted then type the command :

gci -recurse  | Unblock-File 

![image1](Docs/image1.png)

---
EtherNet/IP™ is a registered trademark of ODVA, Inc.
