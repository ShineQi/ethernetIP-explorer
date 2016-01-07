/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2016 Frederic Chaxel <fchaxel@free.fr>
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*********************************************************************/
namespace System.Net.EnIPStack
{
    // Volume 2 : Table 2-3.2 Encapsulation Commands
    public enum EncapsulationCommands : ushort
    {
        Nop = 0x0000,   // Application keep alive
        ListServices = 0x0004,
        ListIdentity = 0x0063,
        ListInterfaces = 0x0064,
        RegisterSession = 0x0065,
        UnRegisterSession = 0x0066,
        SendRRData = 0x006F,
        SendUnitData = 0x0070,
        IndicateStatus = 0x0072,
        Cancel = 0x0073
    }

    // Volume 2 : Table 2-3.3 Error Codes
    // Another time these documents are a bag of shit
    // this table shows 16 bits status
    public enum EncapsulationStatus : uint
    {
        Success = 0x00000000,
        Unsupported_Command = 0x00000001,
        Insufficient_Memory = 0x00000002,
        Incorrect_Data = 0x00000003,
        Invalid_Session_Handle = 0x000000064,
        Invalid_Length = 0x000000065,
        // Exceptionnel
        // There is no protocol version id in the header !!!
        // Information available only in ListInfo fields (upper protocol)
        Unsupported_Protocol_Revision= 0x000000069
    }

    // Volume 1 : Table 5-1.1 Object Specifications
    public enum CIPObjectLibrary : ushort
    {
        Identity = 0x01,
        MessageRouter = 0x02,
        DeviceNet = 0x03,
        Assembly = 0x04,
        Connection = 0x05,
        ConnectionManager = 0x06,
        Register = 0x07,
        DiscreteInputPoint = 0x08,
        DiscreteOutputPoint = 0x09,
        AnalogInputPoint = 0x0A,
        AnalogOutputPoint = 0x0B,
        PresenceSensing = 0x0E,
        Parameter = 0x0F,
        ParameterGroup = 0x10,
        Group = 0x12,
        DiscreteInputGroup = 0x1D,
        DiscreteOutputGroup = 0x1E,
        DiscreteGroup = 0x1F,
        AnalogInputGroup = 0x20,
        AnalogOutputGroup = 0x21,
        AnalogGroup = 0x22,
        PositionSensor = 0x23,
        PositionControllerSupervisor = 0x24,
        PositionController = 0x25,
        BlockSequencer = 0x26,
        CommandBlock = 0x27,
        MotorData = 0x28,
        ControlSupervisor = 0x29,
        AcDcDrive = 0x2A,
        AcknowledgeHandler = 0x2B,
        Overload = 0x2C,
        SoftStart = 0x2D,
        Selection = 0x2E,
        SDeviceSupervisor = 0x30,
        SAnalogSensor = 0x31,
        SAnalogActuator = 0x32,
        SSingleStageController = 0x33,
        SGasCalibration = 0x34,
        TripPoint = 0x35,
        File = 0x37,
        SPartialPressure = 0x38,
        SafetySupervisor = 0x39,
        SafetyValidator = 0x3A,
        SafetyDiscreteOutputPoint = 0x3B,
        SafetyDiscreteOutputGroup = 0x3C,
        SafetyDiscreteInputPoint = 0x3D,
        SafetyDiscreteInputGroup = 0X3E,
        SafetyDualChannelOutput = 0x3F,
        SSensorCalibration = 0x40,
        EventLog = 0x41,
        MotionAxis = 0x42,
        TimeSync = 0x43,
        Modbus=0x44,
        ControlNet = 0xF0,
        ControlNetKeeper = 0xF1,
        ControlNetScheduling = 0xF2,
        ConnectionConfiguration = 0xF3,
        Port = 0xF4,
        TCPIPInterface = 0xF5,
        EtherNetLink = 0xF6,
        CompoNetLink = 0xF7,
        CompoNetRepeater = 0xF8
    }

    // Volume 1 : Table A-3.1 CIP Service Codes and Names
    // High bit 0 for query, 1 for response
    public enum CIPServiceCodes
    {
        GetAttributesAll = 0x01,
        SetAttributeAll = 0x02,
        GetAttributeList = 0x03,
        SetAttributeList = 0x04,
        Reset = 0x05,
        Start = 0x06,
        Stop = 0x07,
        Create = 0x08,
        Delete = 0x09,
        MultipleServicePacket = 0x0A,
        ApplyAttributes = 0x0D,
        GetAttributeSingle = 0x0E,
        SetAttributeSingle = 0x10,
        FindNextObjectInstance = 0x11,
        Restore = 0x15,
        Save = 0x16,
        NOP = 0x17,
        GetMember = 0x18,
        SetMember = 0x19,
        InsertMember = 0x1A,
        RemoveMember = 0x1B,
        GroupSync = 0x1C,
        ForwardClose = 0x4E,
        UnconnectedSend = 0x52,
        ForwardOpen = 0x54,     // Todo Volume 1 : Table 3-5.16
        LargeForwardOpen = 0x5B

    }

    // Volume 1 : Table 5-2.2 Identity Object Instance Attributes 
    public enum IdentityObjectState
    {
        NonExistant = 0,
        DeviceSelfTesting = 1,
        Standby = 2,
        Operational = 3,
        MajorRecoverableFault = 4,
        MajorUnRecoverableFault = 5,
        Default = 255
    }

    // Volume 1 : Table B-1.1 CIP General Status Codes
    public enum CIPGeneralSatusCode : byte
    {
        Success = 0,
        Connection_failure = 1,
        Resource_unavailable = 2,
        Invalid_parameter_value = 3,
        Path_segment_error = 4,
        Path_destination_unknown = 5,
        Partial_transfer = 6,
        Connection_lost = 7,
        Service_not_supported = 8,
        Invalid_attribute_value = 9,
        Attribute_list_error = 10,
        Already_in_requested_mode_state = 11,
        Object_state_conflict = 12,
        Object_already_exists = 13,
        Attribute_not_settable = 14,
        Privilege_violation = 15,
        Device_state_conflict = 16,
        Reply_data_too_large = 17,
        Fragmentation_of_a_primitive_value = 18,
        Not_enough_data = 19,
        Attribute_not_supported = 20,
        Too_much_data = 21,
        Object_does_not_exist = 22,
        Service_fragmentation_sequence_not_in_progress = 23,
        No_stored_attribute_data = 24,
        Store_operation_failure = 25,
        Routing_failure_request_packet_too_large = 26,
        Routing_failure_response_packet_too_large = 27,
        Missing_attribute_list_entry_data = 28,
        Invalid_attribute_value_list = 29,
        Embedded_service_error = 30,
        Vendor_specific_error = 31,
        Invalid_parameter = 32,
        Write_once_value_or_medium_already_written = 33,
        Invalid_reply_received = 34,
        Buffer_overflow = 35,
        Invalid_message_format = 36,
        Key_failure_in_path = 37,
        Path_size_invalid = 38,
        Unexpected_attribute_in_list = 39,
        Invalid_Member_ID = 40,
        Member_not_settable = 41,
        Group_2_only_server_general_failure = 42,
        Unknown_Modbus_error = 43,
        Attribute_not_gettable = 44
    }

    // Volume 2 : Table 2-6.3 Item ID Numbers
    public enum CommonPacketItemIdNumbers : ushort
    {
        NULL = 0x0000,
        ListIdentityResponse = 0x000C,
        ConnectionBased = 0x00A1,
        ConnectedDataItem = 0x00B1,
        UnConnectedDataItem = 0x00B2,
        ListServicesResponse = 0x0100,
        SocketaddrInfo_O2T = 0x8000,
        SocketaddrInfo_T2O = 0x8001,
        SequencedAddressItem = 0x8002
    }

    // Volume 1 : Figure 3-4.2 Transport Class Trigger Attribute
    public enum TransportClassTriggerAttribute : byte
    {
        Cyclic = 0x00,
        ChangeOfState = 0x10,
        ApplicationObject = 0x20
    }
}
