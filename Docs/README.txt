-------------------------------------------------------------------------------
                   EnIPExplorer- Ethernet/IP Explorer
-------------------------------------------------------------------------------

1.  INTRO

	1.1 ABOUT
		EnIPExplorer is a graphical windows program for exploring and navigating
		Ethernet/IP devices. 
		The project was created in order to test and evaluate the protocol.		
	
		This document is subject to change.

	1.2 CREDITS
		The projected was created by F. Chaxel, in 2016. 
		Graphics are the usual FamFamFam: http://www.famfamfam.com/

		It is inspired by the best Bacnet Explorer, Yabe :
		http://sourceforge.net/projects/yetanotherbacnetexplorer/
		and also Profinet Explorer
		http://sourceforge.net/projects/profinetexplorer/

2.  USAGE

	2.1 Exploration
		- Start EnIPExplorer
		- Select "Open Interface" under "Functions".
		- Press the "Add" button.
		  The program will now open an Udp connection and send
		  out a "ListIdentity" broadcast. If Ethernet/IP devices exist on the 
		  network they will show up in the tree.
		- If you have more than 1 ethernet card, you can also select a local 
		  endpoint ip (before you click the "Add"). Either select one from 
		  the list or write one by hand, if the interface is not displayed. 
		  The program will fetch all "Classes" from the devices and
		  display them in the tree. Nothing is displayed if the needed network 
		  service is not supported by the devices.
	2.2 Read Class data
		- When a device (Plc or other) is selected, the class list is requested
		  through the network and displayed (Read attribut ObjectList n°1 of the 
		  unique instance n°1 of the Message Router class n°2).
		- If a class is selected, a network request is sent and the class data
		  is read and displayed in the properties panel.
		- With the shortcut key CTRL-C one can add classes in the list to try
		  to read values (this will not add the class in the remote device,
		  it's just give you the possibility to read). 
	2.3 Read Class instances data
		- For common classes (Identifity, MessageRouter, ...) one class instance
		  is already visible and could be read out. Select it and the values are 
		  displays in the properties panel.	
		- With the shortcut key CTRL-I one can add instances in the list to try
		  to read values (this will not add the instance in the remote device,
		  it's just give you the possibility to read). 		
	2.4 Attribut data (Read & Write)
		- With the shortcut key CTRL-A, same behaviour as §2.2 and $2.3
		- RawByte data could be modified at this level, and are sent to the 
		  remote device (change a value then hit Enter key).

3.  TESTS
	The EnIPExplorer has been tested with realy a too few number of others tools :
		- Wireshark.
		- Wago 750/881 	

4.  SUPPORT
	There's no support for the project at this time, and certainly never. 
	If you write to me, I'm unlikely to answer. 

5.  REPORT ERRORS
	Yes, there be errors alright. There always are. Many won't be interesting
	though. Eg. if you find a computer that behaves differently from others, 
	I'm unlikely to care. This is not a commercial project and I'm not trying 
	to push it to the greater good of the GPL world. (This may change though.)
	If you find a device that doesn't work with it, it might be interesting.
	But in order for me to fix it, I need either access to the physical device
	or printouts from programs like Wireshark, that displays the error.
	Write to me using the Sourceforge link.

6.  CONTRIBUTE
	Really? You think it's missing something? It's not really meant as a huge 
	glorified project you know, but if you really must, try contacting me
	using the Sourceforge link.
	
7.  MISC
	Project web page is located at: 
	https://sourceforge.net/projects/EnIPExplorer/