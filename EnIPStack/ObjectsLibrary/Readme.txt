Classes mainly used for decoding (today ... could be change for encoding also).

See Identity.cs :
	CIP_Identity_class : CIPObjectBaseClass
		used for Identity class attribut decoding
		.. nothing new here but additional attributs can be added 
	CIP_Identity_instance : CIPObject
		used for Identity instance attribut decoding
		and individual attribut decoding also
		Takes care to initialize AttIdMax in the constructor		

See DLR.cs
	CIP_DLR_class : CIPObjectBaseClass
		not provided : common CIPObjectBaseClass decoding process
		will be done

	CIP_DLR_instance : CIPObject
		used for DLR instance attribut decoding
		and individual attribut decoding also
		Takes care to initialize AttIdMax in the constructor

Sends me classes to be added : 
	Names must be CIP_xxxx_class & CIP_xxxx_instance where xxxx
	is the exact name found in the CIPObjectLibrary enumeration
	Takes care to identify each attribut [CIPAttributId(xxx)]
	according to the standard.

There is a way to add proprietary classes decoders in externals dll.
