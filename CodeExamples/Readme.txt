------------------------------------------------------------------------------------------------
				Some codes based on EnIPStack
------------------------------------------------------------------------------------------------

SampleClient
	Device discovery.
	Read a simple attribute data, on a Wago Plc.
	Write a simple attribute data.
SampleClient2
    Device discovery.
    Read a simple attribute data, on an Eurotherm device
    using a InstanceDecoder class for value
    decoding instead of raw data.

Class1Sampleclient : Obsolete don't use it, it will be soon removes
	Read an attribute using Tcp.
	Advise in P2P for it

Class1Sampleclient2
	Some device don't accept ForwardOpen for only one attribute :
	... Config, Output and Input attributes are required on them.
	One or two attributes can be null, so all these options are OK
	(Config,Output) (Config,Input) (Output) (Input) (Output,Input)
	and (Config,Output,Input)
	This code, is working without modification with OpENer basic sample.

