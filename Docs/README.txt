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
		The projected is created by me, F. Chaxel, in 2016. 
		Graphics are the usual FamFamFam: http://www.famfamfam.com/
		It is inspired by the best Bacnet Explorer, Yabe :
		http://sourceforge.net/projects/yetanotherbacnetexplorer/

2.  USAGE

	2.1 Exploration
		- Start EnIPExplorer
		- Select "Open Interface" under "Functions".
		- Press the "Add" button in the "Interface" field.
		  The program will now open an Udp connection and send
		  out a "ListIdentity" broadcast. If there're any Ethernet/IP devices in the 
		  network they will show up in the tree.
		- If you have more than 1 ethernet card, you can also select a local 
		  endpoint ip. (Before you click the "Add"). Either select one from 
		  the list or write one by hand, if the interface is not displayed. 
		  The program will fetch all "Classes" from the devices and
		  display them in the tree. Nothing is display if the needed network service
		  is not supported by the devices.
	2.2 Read Classe instances
		- To be done in a next version		 			

3.  TESTS
	The EnIPExplorer has been tested with really a too few number others tools :
		- Wireshark.
		- Wago 750/881 	

4.  SUPPORT
	There's no support for the project at this time. 
	If you write to me, I'm unlikely to answer. 

5.  REPORT ERRORS
	Yeh, there be errors alright. There always are. Many won't be interesting
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