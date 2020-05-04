Download SnapShot on Windows 10

A lot of files (.resx and some others) are marked block by Windows, and Visual Studio cannot build the projects.

To unblock all files, open a PowerShell window, move to the directory where all files are extracted then type the command :

gci -recurse  | Unblock-File 

![image1](https://github.com/tswaehn/ethernetIP-explorer/blob/master/Docs/image1.png)
![image2](https://github.com/tswaehn/ethernetIP-explorer/blob/master/Docs/image2.png)
![image3](https://github.com/tswaehn/ethernetIP-explorer/blob/master/Docs/image3.png)
![image4](https://github.com/tswaehn/ethernetIP-explorer/blob/master/Docs/image4.png)
![image5](https://github.com/tswaehn/ethernetIP-explorer/blob/master/Docs/image5.png)
![image6](https://github.com/tswaehn/ethernetIP-explorer/blob/master/Docs/image6.png)

---
EtherNet/IP™ is a registered trademark of ODVA, Inc.
