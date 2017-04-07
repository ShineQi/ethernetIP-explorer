Classes mainly used for decoding (today ... could be change for encoding also).

See Identity.cs :
	CIP_Identity_class : CIPObjectBaseClass
		used for Identity class attribut's decoding
		.. nothing new than in CIPObjectBaseClass here 
		but additional attributs can be added 
	CIP_Identity_instance : CIPObject
		used for Identity instance attribut's decoding
		and individual attribut's decoding also
		Takes care to initialize AttIdMax in the constructor
		Looks attributs 4 for structure		

See DLR.cs
	CIP_DLR_class : CIPObjectBaseClass
		not provided : common CIPObjectBaseClass decoding process
		will be done

	CIP_DLR_instance : CIPObject
		used for DLR instance attribut's decoding
		and individual attribut's decoding also
		Takes care to initialize AttIdMax in the constructor

see TCPIPInterface
	CIP_TCPIPInterface_instance : CIPObject
		see attribut 4 with 2 fields without structure (flat)

Send me classes to be added : 
	Names must be CIP_xxxx_class & CIP_xxxx_instance where xxxx
	is the exact name found in the CIPObjectLibrary enumeration
	Takes care to identify each attribut [CIPAttributId(xxx)]
	according to the standard.

