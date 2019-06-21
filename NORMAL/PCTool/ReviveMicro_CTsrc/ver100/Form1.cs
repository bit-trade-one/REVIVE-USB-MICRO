//-------------------------------------------------------------------------------------------------------------------------------------------------------------------
//-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------
/********************************************************************
 FileName:		Form1.cs
 Dependencies:	When compiled, needs .NET framework 2.0 redistributable to run.
 Hardware:		Need a free USB port to connect USB peripheral device
				programmed with appropriate Generic HID firmware.  VID and
				PID in firmware must match the VID and PID in this
				program.
 Compiler:  	Microsoft Visual C# 2005 Express Edition (or better)
 Company:		Microchip Technology, Inc.

 Software License Agreement:

 The software supplied herewith by Microchip Technology Incorporated
 (the 鼎ompany・ for its PICｮ Microcontroller is intended and
 supplied to you, the Company痴 customer, for use solely and
 exclusively with Microchip PIC Microcontroller products. The
 software is owned by the Company and/or its supplier, and is
 protected under applicable copyright laws. All rights are reserved.
 Any use in violation of the foregoing restrictions may subject the
 user to criminal sanctions under applicable laws, as well as to
 civil liability for the breach of the terms and conditions of this
 license.

 THIS SOFTWARE IS PROVIDED IN AN 鄭S IS・CONDITION. NO WARRANTIES,
 WHETHER EXPRESS, IMPLIED OR STATUTORY, INCLUDING, BUT NOT LIMITED
 TO, IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 PARTICULAR PURPOSE APPLY TO THIS SOFTWARE. THE COMPANY SHALL NOT,
 IN ANY CIRCUMSTANCES, BE LIABLE FOR SPECIAL, INCIDENTAL OR
 CONSEQUENTIAL DAMAGES, FOR ANY REASON WHATSOEVER.

********************************************************************
 File Description:

 Change History:
  Rev   Date         Description
  2.5a	07/17/2009	 Initial Release.  Ported from HID PnP Demo
                     application source, which was originally 
                     written in MSVC++ 2005 Express Edition.
********************************************************************
NOTE:	All user made code contained in this project is in the Form1.cs file.
		All other code and files were generated automatically by either the
		new project wizard, or by the development environment (ex: code is
		automatically generated if you create a new button on the form, and
		then double click on it, which creates a click event handler
		function).  User developed code (code not developed by the IDE) has
        been marked in "cut and paste blocks" to make it easier to identify.
********************************************************************/

//NOTE: In order for this program to "find" a USB device with a given VID and PID, 
//both the VID and PID in the USB device descriptor (in the USB firmware on the 
//microcontroller), as well as in this PC application source code, must match.
//To change the VID/PID in this PC application source code, scroll down to the 
//CheckIfPresentAndGetUSBDevicePath() function, and change the line that currently
//reads:

//   String DeviceIDToFind = "Vid_04d8&Pid_003f";


//NOTE 2: This C# program makes use of several functions in setupapi.dll and
//other Win32 DLLs.  However, one cannot call the functions directly in a 
//32-bit DLL if the project is built in "Any CPU" mode, when run on a 64-bit OS.
//When configured to build an "Any CPU" executable, the executable will "become"
//a 64-bit executable when run on a 64-bit OS.  On a 32-bit OS, it will run as 
//a 32-bit executable, and the pointer sizes and other aspects of this 
//application will be compatible with calling 32-bit DLLs.

//Therefore, on a 64-bit OS, this application will not work unless it is built in
//"x86" mode.  When built in this mode, the exectuable always runs in 32-bit mode
//even on a 64-bit OS.  This allows this application to make 32-bit DLL function 
//calls, when run on either a 32-bit or 64-bit OS.

//By default, on a new project, C# normally wants to build in "Any CPU" mode.  
//To switch to "x86" mode, open the "Configuration Manager" window.  In the 
//"Active solution platform:" drop down box, select "x86".  If this option does
//not appear, select: "<New...>" and then select the x86 option in the 
//"Type or select the new platform:" drop down box.  

//-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------------------------------------------------------------------------



using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;


namespace HID_PnP_Demo
{
    public partial class Form1 : Form
    {

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------

        //Constant definitions from setupapi.h, which we aren't allowed to include directly since this is C#
        internal const uint DIGCF_PRESENT = 0x02;
        internal const uint DIGCF_DEVICEINTERFACE = 0x10;
        //Constants for CreateFile() and other file I/O functions
        internal const short FILE_ATTRIBUTE_NORMAL = 0x80;
        internal const short INVALID_HANDLE_VALUE = -1;
        internal const uint GENERIC_READ = 0x80000000;
        internal const uint GENERIC_WRITE = 0x40000000;
        internal const uint CREATE_NEW = 1;
        internal const uint CREATE_ALWAYS = 2;
        internal const uint OPEN_EXISTING = 3;
        internal const uint FILE_SHARE_READ = 0x00000001;
        internal const uint FILE_SHARE_WRITE = 0x00000002;
        //Constant definitions for certain WM_DEVICECHANGE messages
        internal const uint WM_DEVICECHANGE = 0x0219;
        internal const uint DBT_DEVICEARRIVAL = 0x8000;
        internal const uint DBT_DEVICEREMOVEPENDING = 0x8003;
        internal const uint DBT_DEVICEREMOVECOMPLETE = 0x8004;
        internal const uint DBT_CONFIGCHANGED = 0x0018;
        //Other constant definitions
        internal const uint DBT_DEVTYP_DEVICEINTERFACE = 0x05;
        internal const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0x00;
        internal const uint ERROR_SUCCESS = 0x00;
        internal const uint ERROR_NO_MORE_ITEMS = 0x00000103;
        internal const uint SPDRP_HARDWAREID = 0x00000001;

        //Various structure definitions for structures that this code will be using
        internal struct SP_DEVICE_INTERFACE_DATA
        {
            internal uint cbSize;               //DWORD
            internal Guid InterfaceClassGuid;   //GUID
            internal uint Flags;                //DWORD
            internal uint Reserved;             //ULONG_PTR MSDN says ULONG_PTR is "typedef unsigned __int3264 ULONG_PTR;"  
        }

        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            internal uint cbSize;               //DWORD
            internal char[] DevicePath;         //TCHAR array of any size
        }
        
        internal struct SP_DEVINFO_DATA
        {
            internal uint cbSize;       //DWORD
            internal Guid ClassGuid;    //GUID
            internal uint DevInst;      //DWORD
            internal uint Reserved;     //ULONG_PTR  MSDN says ULONG_PTR is "typedef unsigned __int3264 ULONG_PTR;"  
        }

        internal struct DEV_BROADCAST_DEVICEINTERFACE
        {
            internal uint dbcc_size;            //DWORD
            internal uint dbcc_devicetype;      //DWORD
            internal uint dbcc_reserved;        //DWORD
            internal Guid dbcc_classguid;       //GUID
            internal char[] dbcc_name;          //TCHAR array
        }

        //DLL Imports.  Need these to access various C style unmanaged functions contained in their respective DLL files.
        //--------------------------------------------------------------------------------------------------------------
        //Returns a HDEVINFO type for a device information set.  We will need the 
        //HDEVINFO as in input parameter for calling many of the other SetupDixxx() functions.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr SetupDiGetClassDevs(
            ref Guid ClassGuid,     //LPGUID    Input: Need to supply the class GUID. 
            IntPtr Enumerator,      //PCTSTR    Input: Use NULL here, not important for our purposes
            IntPtr hwndParent,      //HWND      Input: Use NULL here, not important for our purposes
            uint Flags);            //DWORD     Input: Flags describing what kind of filtering to use.

	    //Gives us "PSP_DEVICE_INTERFACE_DATA" which contains the Interface specific GUID (different
	    //from class GUID).  We need the interface GUID to get the device path.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr DeviceInfoSet,           //Input: Give it the HDEVINFO we got from SetupDiGetClassDevs()
            IntPtr DeviceInfoData,          //Input (optional)
            ref Guid InterfaceClassGuid,    //Input 
            uint MemberIndex,               //Input: "Index" of the device you are interested in getting the path for.
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);    //Output: This function fills in an "SP_DEVICE_INTERFACE_DATA" structure.

        //SetupDiDestroyDeviceInfoList() frees up memory by destroying a DeviceInfoList
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiDestroyDeviceInfoList(
            IntPtr DeviceInfoSet);          //Input: Give it a handle to a device info list to deallocate from RAM.

        //SetupDiEnumDeviceInfo() fills in an "SP_DEVINFO_DATA" structure, which we need for SetupDiGetDeviceRegistryProperty()
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            ref SP_DEVINFO_DATA DeviceInterfaceData);

        //SetupDiGetDeviceRegistryProperty() gives us the hardware ID, which we use to check to see if it has matching VID/PID
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property,
            ref uint PropertyRegDataType,
            IntPtr PropertyBuffer,
            uint PropertyBufferSize,
            ref uint RequiredSize);

        //SetupDiGetDeviceInterfaceDetail() gives us a device path, which is needed before CreateFile() can be used.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr DeviceInfoSet,                   //Input: Wants HDEVINFO which can be obtained from SetupDiGetClassDevs()
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,                    //Input: Pointer to an structure which defines the device interface.  
            IntPtr  DeviceInterfaceDetailData,      //Output: Pointer to a SP_DEVICE_INTERFACE_DETAIL_DATA structure, which will receive the device path.
            uint DeviceInterfaceDetailDataSize,     //Input: Number of bytes to retrieve.
            ref uint RequiredSize,                  //Output (optional): The number of bytes needed to hold the entire struct 
            IntPtr DeviceInfoData);                 //Output (optional): Pointer to a SP_DEVINFO_DATA structure

        //Overload for SetupDiGetDeviceInterfaceDetail().  Need this one since we can't pass NULL pointers directly in C#.
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr DeviceInfoSet,                   //Input: Wants HDEVINFO which can be obtained from SetupDiGetClassDevs()
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,               //Input: Pointer to an structure which defines the device interface.  
            IntPtr DeviceInterfaceDetailData,       //Output: Pointer to a SP_DEVICE_INTERFACE_DETAIL_DATA structure, which will contain the device path.
            uint DeviceInterfaceDetailDataSize,     //Input: Number of bytes to retrieve.
            IntPtr RequiredSize,                    //Output (optional): Pointer to a DWORD to tell you the number of bytes needed to hold the entire struct 
            IntPtr DeviceInfoData);                 //Output (optional): Pointer to a SP_DEVINFO_DATA structure

        //Need this function for receiving all of the WM_DEVICECHANGE messages.  See MSDN documentation for
        //description of what this function does/how to use it. Note: name is remapped "RegisterDeviceNotificationUM" to
        //avoid possible build error conflicts.
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr RegisterDeviceNotification(
            IntPtr hRecipient,
            IntPtr NotificationFilter,
            uint Flags);

        //Takes in a device path and opens a handle to the device.
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode, 
            IntPtr lpSecurityAttributes, 
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes, 
            IntPtr hTemplateFile);

        //Uses a handle (created with CreateFile()), and lets us write USB data to the device.
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool WriteFile(
            SafeFileHandle hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            ref uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        //Uses a handle (created with CreateFile()), and lets us read USB data from the device.
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool ReadFile(
            SafeFileHandle hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToRead,
            ref uint lpNumberOfBytesRead,
            IntPtr lpOverlapped);



	    //--------------- Global Varibles Section ------------------
	    //USB related variables that need to have wide scope.
	    bool AttachedState = false;						//Need to keep track of the USB device attachment status for proper plug and play operation.
	    bool AttachedButBroken = false;
        SafeFileHandle WriteHandleToUSBDevice = null;
        SafeFileHandle ReadHandleToUSBDevice = null;
        String DevicePath = null;   //Need the find the proper device path before you can open file handles.

        int NUM_OF_BUTTONS = 12;

	    //Variables used by the application/form updates.
        byte[] eeprom_data = new byte[64]{0,0,0,0,0,0,0,0,0,0,
                                          0,0,0,0,0,0,0,0,0,0,
                                          0,0,0,0,0,0,0,0,0,0,
                                          0,0,0,0,0,0,0,0,0,0,
                                          0,0,0,0,0,0,0,0,0,0,
                                          0,0,0,0,0,0,0,0,0,0,
                                          0,0,0,0};
        bool[] ChangeAssign = new bool[12] { true, true, true, true, true, true, true, true, true, true, true ,true };
        bool ConnectFirstTime = true;
        bool Changevalue_btn_pressed = false;
        bool Reset_btn_pressed = false;

        byte DeviceType_selected = 0;
        byte MouseValue_selected = 0;
        byte KeyboardValue_selected = 0;
        byte KeyboardModifier_selected = 0;
        byte LeverValue_selected = 0;
        byte ButtonValue_selected = 0;
        byte ButtonValue_selected2 = 0;
        uint PushButonState = 0;
        int SetPin_selected = 0;
        byte MouseMoveValue_selected = 80;
        byte Now_Background_image = 1;  //どっちの背景が表示されているか。0:未接続 1:接続済み
        int StatusBoxChange = 99;
        bool ReadFromDevice = true;

        //バーチャルキーコードとUSBキーコードの変換用配列
        byte[] VKtoUSBkey = new byte[256]{
            0x00,   //0
            0x00,   //1
            0x00,   //2
            0x00,   //3
            0x00,   //4
            0x00,   //5
            0x00,   //6
            0x00,   //7
            0x2A,   //8
            0x2B,   //9
            0x00,   //10
            0x00,   //11
            0x00,   //12
            0x28,   //13
            0x00,   //14
            0x00,   //15
            0xE1,   //16
            0xE0,   //17
            0xE2,   //18
            0x48,   //19
            0x39,   //20
            0x88,   //21
            0x00,   //22
            0x00,   //23
            0x00,   //24
            0x35,   //25
            0x00,   //26
            0x29,   //27
            0x8A,   //28
            0x8B,   //29
            0x00,   //30
            0x00,   //31
            0x2C,   //32
            0x4B,   //33
            0x4E,   //34
            0x4D,   //35
            0x4A,   //36
            0x50,   //37
            0x52,   //38
            0x4F,   //39
            0x51,   //40
            0x00,   //41
            0x00,   //42
            0x00,   //43
            0x46,   //44
            0x49,   //45
            0x4C,   //46
            0x00,   //47
            0x27,   //48
            0x1E,   //49
            0x1F,   //50
            0x20,   //51
            0x21,   //52
            0x22,   //53
            0x23,   //54
            0x24,   //55
            0x25,   //56
            0x26,   //57
            0x00,   //58
            0x00,   //59
            0x00,   //60
            0x00,   //61
            0x00,   //62
            0x00,   //63
            0x00,   //64
            0x04,   //65
            0x05,   //66
            0x06,   //67
            0x07,   //68
            0x08,   //69
            0x09,   //70
            0x0A,   //71
            0x0B,   //72
            0x0C,   //73
            0x0D,   //74
            0x0E,   //75
            0x0F,   //76
            0x10,   //77
            0x11,   //78
            0x12,   //79
            0x13,   //80
            0x14,   //81
            0x15,   //82
            0x16,   //83
            0x17,   //84
            0x18,   //85
            0x19,   //86
            0x1A,   //87
            0x1B,   //88
            0x1C,   //89
            0x1D,   //90
            0xE3,   //91
            0xE7,   //92
            0x65,   //93
            0x00,   //94
            0x00,   //95
            0x62,   //96
            0x59,   //97
            0x5A,   //98
            0x5B,   //99
            0x5C,   //100
            0x5D,   //101
            0x5E,   //102
            0x5F,   //103
            0x60,   //104
            0x61,   //105
            0x55,   //106
            0x57,   //107
            0x85,   //108
            0x56,   //109
            0x63,   //110
            0x54,   //111
            0x3A,   //112
            0x3B,   //113
            0x3C,   //114
            0x3D,   //115
            0x3E,   //116
            0x3F,   //117
            0x40,   //118
            0x41,   //119
            0x42,   //120
            0x43,   //121
            0x44,   //122
            0x45,   //123
            0x68,   //124
            0x69,   //125
            0x6A,   //126
            0x6B,   //127
            0x6C,   //128
            0x6D,   //129
            0x6E,   //130
            0x6F,   //131
            0x70,   //132
            0x71,   //133
            0x72,   //134
            0x73,   //135
            0x00,   //136
            0x00,   //137
            0x00,   //138
            0x00,   //139
            0x00,   //140
            0x00,   //141
            0x00,   //142
            0x00,   //143
            0x53,   //144
            0x47,   //145
            0x67,   //146
            0x00,   //147
            0x00,   //148
            0x00,   //149
            0x00,   //150
            0x00,   //151
            0x00,   //152
            0x00,   //153
            0x00,   //154
            0x00,   //155
            0x00,   //156
            0x00,   //157
            0x00,   //158
            0x00,   //159
            0xE1,   //160
            0xE5,   //161
            0xE0,   //162
            0xE4,   //163
            0xE2,   //164
            0xE6,   //165
            0x00,   //166
            0x00,   //167
            0x00,   //168
            0x00,   //169
            0x00,   //170
            0x00,   //171
            0x00,   //172
            0x00,   //173
            0x00,   //174
            0x00,   //175
            0x00,   //176
            0x00,   //177
            0x00,   //178
            0x00,   //179
            0x00,   //180
            0x00,   //181
            0x00,   //182
            0x00,   //183
            0x00,   //184
            0x00,   //185
            0x34,   //186
            0x33,   //187
            0x36,   //188
            0x2D,   //189
            0x37,   //190
            0x38,   //191
            0x2F,   //192
            0x00,   //193
            0x00,   //194
            0x00,   //195
            0x00,   //196
            0x00,   //197
            0x00,   //198
            0x00,   //199
            0x00,   //200
            0x00,   //201
            0x00,   //202
            0x00,   //203
            0x00,   //204
            0x00,   //205
            0x00,   //206
            0x00,   //207
            0x00,   //208
            0x00,   //209
            0x00,   //210
            0x00,   //211
            0x00,   //212
            0x00,   //213
            0x00,   //214
            0x00,   //215
            0x00,   //216
            0x00,   //217
            0x00,   //218
            0x30,   //219
            0x89,   //220
            0x32,   //221
            0x2E,   //222
            0x00,   //223
            0x00,   //224
            0x00,   //225
            0x87,   //226
            0x00,   //227
            0x00,   //228
            0x00,   //229
            0x00,   //230
            0x00,   //231
            0x00,   //232
            0x00,   //233
            0x00,   //234
            0x00,   //235
            0x00,   //236
            0x00,   //237
            0x00,   //238
            0x00,   //239
            0x39,   //240
            0x00,   //241
            0x39,   //242
            0x35,   //243
            0x35,   //244
            0x00,   //245
            0x00,   //246
            0x00,   //247
            0x00,   //248
            0x00,   //249
            0x00,   //250
            0x00,   //251
            0x00,   //252
            0x00,   //253
            0x00,   //254
            0x00,   //255
       };
#if true
        //USBキーコードの名称配列
        public string[] USB_KeyCode_Name = new string[256]{
            "",   //0
            "",   //1
            "",   //2
            "",   //3
            "A",   //4
            "B",   //5
            "C",   //6
            "D",   //7
            "E",   //8
            "F",   //9
            "G",   //10
            "H",   //11
            "I",   //12
            "J",   //13
            "K",   //14
            "L",   //15
            "M",   //16
            "N",   //17
            "O",   //18
            "P",   //19
            "Q",   //20
            "R",   //21
            "S",   //22
            "T",   //23
            "U",   //24
            "V",   //25
            "W",   //26
            "X",   //27
            "Y",   //28
            "Z",   //29
            "1",   //30
            "2",   //31
            "3",   //32
            "4",   //33
            "5",   //34
            "6",   //35
            "7",   //36
            "8",   //37
            "9",   //38
            "0",   //39
            "Enter",   //40
            "ESC",   //41
            "BS",   //42
            "Tab",   //43
            "Space",   //44
            "-",   //45
            "^",   //46
            "@",   //47
            "[",   //48
            "",   //49
            "]",   //50
            ";",   //51
            ":",   //52
            "ZenHan",   //53
            ",",   //54
            ".",   //55
            "/",   //56
            "CapsLK",   //57
            "F1",   //58
            "F2",   //59
            "F3",   //60
            "F4",   //61
            "F5",   //62
            "F6",   //63
            "F7",   //64
            "F8",   //65
            "F9",   //66
            "F10",   //67
            "F11",   //68
            "F12",   //69
            "PrtSC",   //70
            "ScLK",   //71
            "Pause",   //72
            "Insert",   //73
            "Home",   //74
            "pgUp",   //75
            "Delete",   //76
            "End",   //77
            "pgDown",   //78
            "→",   //79
            "←",   //80
            "↓",   //81
            "↑",   //82
            "NumLK",   //83
            "Num/",   //84
            "Num*",   //85
            "Num-",   //86
            "Num+",   //87
            "NumENT",   //88
            "Num1",   //89
            "Num2",   //90
            "Num3",   //91
            "Num4",   //92
            "Num5",   //93
            "Num6",   //94
            "Num7",   //95
            "Num8",   //96
            "Num9",   //97
            "Num0",   //98
            "Num.",   //99
            "",   //100
            "Menu",   //101
            "",   //102
            "",   //103
            "",   //104
            "",   //105
            "",   //106
            "",   //107
            "",   //108
            "",   //109
            "",   //110
            "",   //111
            "",   //112
            "",   //113
            "",   //114
            "",   //115
            "",   //116
            "",   //117
            "",   //118
            "",   //119
            "",   //120
            "",   //121
            "",   //122
            "",   //123
            "",   //124
            "",   //125
            "",   //126
            "",   //127
            "",   //128
            "",   //129
            "",   //130
            "",   //131
            "",   //132
            "",   //133
            "",   //134
            "BackSL",   //135
            "k/Hira",   //136
            "￥",   //137
            "変換",   //138
            "無変換",   //139
            "",   //140
            "",   //141
            "",   //142
            "",   //143
            "",   //144
            "",   //145
            "",   //146
            "",   //147
            "",   //148
            "",   //149
            "",   //150
            "",   //151
            "",   //152
            "",   //153
            "",   //154
            "",   //155
            "",   //156
            "",   //157
            "",   //158
            "",   //159
            "",   //160
            "",   //161
            "",   //162
            "",   //163
            "",   //164
            "",   //165
            "",   //166
            "",   //167
            "",   //168
            "",   //169
            "",   //170
            "",   //171
            "",   //172
            "",   //173
            "",   //174
            "",   //175
            "",   //176
            "",   //177
            "",   //178
            "",   //179
            "",   //180
            "",   //181
            "",   //182
            "",   //183
            "",   //184
            "",   //185
            "",   //186
            "",   //187
            "",   //188
            "",   //189
            "",   //190
            "",   //191
            "",   //192
            "",   //193
            "",   //194
            "",   //195
            "",   //196
            "",   //197
            "",   //198
            "",   //199
            "",   //200
            "",   //201
            "",   //202
            "",   //203
            "",   //204
            "",   //205
            "",   //206
            "",   //207
            "",   //208
            "",   //209
            "",   //210
            "",   //211
            "",   //212
            "",   //213
            "",   //214
            "",   //215
            "",   //216
            "",   //217
            "",   //218
            "",   //219
            "",   //220
            "",   //221
            "",   //222
            "",   //223
            "Ctrl L",   //224
            "Shift L",   //225
            "Alt L",   //226
            "Win L",   //227
            "Ctrl R",   //228
            "Shift R",   //229
            "Alt R",   //230
            "Win R",   //231
            "",   //232
            "",   //233
            "",   //234
            "",   //235
            "",   //236
            "",   //237
            "",   //238
            "",   //239
            "",   //240
            "",   //241
            "",   //242
            "",   //243
            "",   //244
            "",   //245
            "",   //246
            "",   //247
            "",   //248
            "",   //249
            "",   //250
            "",   //251
            "",   //252
            "",   //253
            "",   //254
            "",   //255
       };
#endif


        //Globally Unique Identifier (GUID) for HID class devices.  Windows uses GUIDs to identify things.
        Guid InterfaceClassGuid = new Guid(0x4d1e55b2, 0xf16f, 0x11cf, 0x88, 0xcb, 0x00, 0x11, 0x11, 0x00, 0x00, 0x30); 
	    //--------------- End of Global Varibles ------------------

        //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------


        //Need to check "Allow unsafe code" checkbox in build properties to use unsafe keyword.  Unsafe is needed to
        //properly interact with the unmanged C++ style APIs used to find and connect with the USB device.
        public unsafe Form1()
        {
            InitializeComponent();

            
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
            //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------
			//Additional constructor code

            //Initialize tool tips, to provide pop up help when the mouse cursor is moved over objects on the form.
            ToggleLEDToolTip.SetToolTip(this.Changevalue_btn, "Sends a packet of data to the USB device.");

            //Register for WM_DEVICECHANGE notifications.  This code uses these messages to detect plug and play connection/disconnection events for USB devices
            DEV_BROADCAST_DEVICEINTERFACE DeviceBroadcastHeader = new DEV_BROADCAST_DEVICEINTERFACE();
            DeviceBroadcastHeader.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            DeviceBroadcastHeader.dbcc_size = (uint)Marshal.SizeOf(DeviceBroadcastHeader);
            DeviceBroadcastHeader.dbcc_reserved = 0;	//Reserved says not to use...
            DeviceBroadcastHeader.dbcc_classguid = InterfaceClassGuid;

            //Need to get the address of the DeviceBroadcastHeader to call RegisterDeviceNotification(), but
            //can't use "&DeviceBroadcastHeader".  Instead, using a roundabout means to get the address by 
            //making a duplicate copy using Marshal.StructureToPtr().
            IntPtr pDeviceBroadcastHeader = IntPtr.Zero;  //Make a pointer.
            pDeviceBroadcastHeader = Marshal.AllocHGlobal(Marshal.SizeOf(DeviceBroadcastHeader)); //allocate memory for a new DEV_BROADCAST_DEVICEINTERFACE structure, and return the address 
            Marshal.StructureToPtr(DeviceBroadcastHeader, pDeviceBroadcastHeader, false);  //Copies the DeviceBroadcastHeader structure into the memory already allocated at DeviceBroadcastHeaderWithPointer
            RegisterDeviceNotification(this.Handle, pDeviceBroadcastHeader, DEVICE_NOTIFY_WINDOW_HANDLE);
 

			//Now make an initial attempt to find the USB device, if it was already connected to the PC and enumerated prior to launching the application.
			//If it is connected and present, we should open read and write handles to the device so we can communicate with it later.
			//If it was not connected, we will have to wait until the user plugs the device in, and the WM_DEVICECHANGE callback function can process
			//the message and again search for the device.
			if(CheckIfPresentAndGetUSBDevicePath())	//Check and make sure at least one device with matching VID/PID is attached
			{
				uint ErrorStatusWrite;
				uint ErrorStatusRead;


				//We now have the proper device path, and we can finally open read and write handles to the device.
                WriteHandleToUSBDevice = CreateFile(DevicePath, GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                ErrorStatusWrite = (uint)Marshal.GetLastWin32Error();
                ReadHandleToUSBDevice = CreateFile(DevicePath, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                ErrorStatusRead = (uint)Marshal.GetLastWin32Error();

				if((ErrorStatusWrite == ERROR_SUCCESS) && (ErrorStatusRead == ERROR_SUCCESS))
				{
					AttachedState = true;		//Let the rest of the PC application know the USB device is connected, and it is safe to read/write to it
					AttachedButBroken = false;
                    StatusBox_lbl.Text = "デバイス検出済";
				}
				else //for some reason the device was physically plugged in, but one or both of the read/write handles didn't open successfully...
				{
					AttachedState = false;		//Let the rest of this application known not to read/write to the device.
					AttachedButBroken = true;	//Flag so that next time a WM_DEVICECHANGE message occurs, can retry to re-open read/write pipes
					if(ErrorStatusWrite == ERROR_SUCCESS)
						WriteHandleToUSBDevice.Close();
					if(ErrorStatusRead == ERROR_SUCCESS)
						ReadHandleToUSBDevice.Close();
				}
			}
			else	//Device must not be connected (or not programmed with correct firmware)
			{
				AttachedState = false;
				AttachedButBroken = false;
			}

            if (AttachedState == true)
            {
                StatusBox_lbl.Text = "デバイス検出済";
            }
            else
            {
                StatusBox_lbl.Text = "デバイス未検出";
            }

			ReadWriteThread.RunWorkerAsync();	//Recommend performing USB read/write operations in a separate thread.  Otherwise,
												//the Read/Write operations are effectively blocking functions and can lock up the
												//user interface if the I/O operations take a long time to complete.

            //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------

        //FUNCTION:	CheckIfPresentAndGetUSBDevicePath()
        //PURPOSE:	Check if a USB device is currently plugged in with a matching VID and PID
        //INPUT:	Uses globally declared String DevicePath, globally declared GUID, and the MY_DEVICE_ID constant.
        //OUTPUT:	Returns BOOL.  TRUE when device with matching VID/PID found.  FALSE if device with VID/PID could not be found.
        //			When returns TRUE, the globally accessable "DetailedInterfaceDataStructure" will contain the device path
        //			to the USB device with the matching VID/PID.

        bool CheckIfPresentAndGetUSBDevicePath()
        {
		    /* 
		    Before we can "connect" our application to our USB embedded device, we must first find the device.
		    A USB bus can have many devices simultaneously connected, so somehow we have to find our device only.
		    This is done with the Vendor ID (VID) and Product ID (PID).  Each USB product line should have
		    a unique combination of VID and PID.  

		    Microsoft has created a number of functions which are useful for finding plug and play devices.  Documentation
		    for each function used can be found in the MSDN library.  We will be using the following functions (unmanaged C functions):

		    SetupDiGetClassDevs()					//provided by setupapi.dll, which comes with Windows
		    SetupDiEnumDeviceInterfaces()			//provided by setupapi.dll, which comes with Windows
		    GetLastError()							//provided by kernel32.dll, which comes with Windows
		    SetupDiDestroyDeviceInfoList()			//provided by setupapi.dll, which comes with Windows
		    SetupDiGetDeviceInterfaceDetail()		//provided by setupapi.dll, which comes with Windows
		    SetupDiGetDeviceRegistryProperty()		//provided by setupapi.dll, which comes with Windows
		    CreateFile()							//provided by kernel32.dll, which comes with Windows

            In order to call these unmanaged functions, the Marshal class is very useful.
             
		    We will also be using the following unusual data types and structures.  Documentation can also be found in
		    the MSDN library:

		    PSP_DEVICE_INTERFACE_DATA
		    PSP_DEVICE_INTERFACE_DETAIL_DATA
		    SP_DEVINFO_DATA
		    HDEVINFO
		    HANDLE
		    GUID

		    The ultimate objective of the following code is to get the device path, which will be used elsewhere for getting
		    read and write handles to the USB device.  Once the read/write handles are opened, only then can this
		    PC application begin reading/writing to the USB device using the WriteFile() and ReadFile() functions.

		    Getting the device path is a multi-step round about process, which requires calling several of the
		    SetupDixxx() functions provided by setupapi.dll.
		    */

            try
            {
		        IntPtr DeviceInfoTable = IntPtr.Zero;
		        SP_DEVICE_INTERFACE_DATA InterfaceDataStructure = new SP_DEVICE_INTERFACE_DATA();
                SP_DEVICE_INTERFACE_DETAIL_DATA DetailedInterfaceDataStructure = new SP_DEVICE_INTERFACE_DETAIL_DATA();
                SP_DEVINFO_DATA DevInfoData = new SP_DEVINFO_DATA();

		        uint InterfaceIndex = 0;
		        uint dwRegType = 0;
		        uint dwRegSize = 0;
                uint dwRegSize2 = 0;
		        uint StructureSize = 0;
		        IntPtr PropertyValueBuffer = IntPtr.Zero;
		        bool MatchFound = false;
                bool MatchFound2 = false;
                uint ErrorStatus;
		        uint LoopCounter = 0;

                //Use the formatting: "Vid_xxxx&Pid_xxxx" where xxxx is a 16-bit hexadecimal number.
                //Make sure the value appearing in the parathesis matches the USB device descriptor
                //of the device that this aplication is intending to find.
                String DeviceIDToFind = "Vid_22ea&Pid_0059";
                String DeviceIDToFind2 = "Mi_03";


		        //First populate a list of plugged in devices (by specifying "DIGCF_PRESENT"), which are of the specified class GUID. 
		        DeviceInfoTable = SetupDiGetClassDevs(ref InterfaceClassGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                if(DeviceInfoTable != IntPtr.Zero)
                {
		            //Now look through the list we just populated.  We are trying to see if any of them match our device. 
		            while(true)
		            {
                        InterfaceDataStructure.cbSize = (uint)Marshal.SizeOf(InterfaceDataStructure);
			            if(SetupDiEnumDeviceInterfaces(DeviceInfoTable, IntPtr.Zero, ref InterfaceClassGuid, InterfaceIndex, ref InterfaceDataStructure))
			            {
                            ErrorStatus = (uint)Marshal.GetLastWin32Error();
                            if (ErrorStatus == ERROR_NO_MORE_ITEMS)	//Did we reach the end of the list of matching devices in the DeviceInfoTable?
				            {	//Cound not find the device.  Must not have been attached.
					            SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
					            return false;		
				            }
			            }
			            else	//Else some other kind of unknown error ocurred...
			            {
                            ErrorStatus = (uint)Marshal.GetLastWin32Error();
				            SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
				            return false;	
			            }

			            //Now retrieve the hardware ID from the registry.  The hardware ID contains the VID and PID, which we will then 
			            //check to see if it is the correct device or not.

			            //Initialize an appropriate SP_DEVINFO_DATA structure.  We need this structure for SetupDiGetDeviceRegistryProperty().
                        DevInfoData.cbSize = (uint)Marshal.SizeOf(DevInfoData);
			            SetupDiEnumDeviceInfo(DeviceInfoTable, InterfaceIndex, ref DevInfoData);

			            //First query for the size of the hardware ID, so we can know how big a buffer to allocate for the data.
			            SetupDiGetDeviceRegistryProperty(DeviceInfoTable, ref DevInfoData, SPDRP_HARDWAREID, ref dwRegType, IntPtr.Zero, 0, ref dwRegSize);

			            //Allocate a buffer for the hardware ID.
                        //Should normally work, but could throw exception "OutOfMemoryException" if not enough resources available.
                        PropertyValueBuffer = Marshal.AllocHGlobal((int)dwRegSize);

			            //Retrieve the hardware IDs for the current device we are looking at.  PropertyValueBuffer gets filled with a 
			            //REG_MULTI_SZ (array of null terminated strings).  To find a device, we only care about the very first string in the
			            //buffer, which will be the "device ID".  The device ID is a string which contains the VID and PID, in the example 
			            //format "Vid_04d8&Pid_003f".
                        SetupDiGetDeviceRegistryProperty(DeviceInfoTable, ref DevInfoData, SPDRP_HARDWAREID, ref dwRegType, PropertyValueBuffer, dwRegSize, ref dwRegSize2);

			            //Now check if the first string in the hardware ID matches the device ID of the USB device we are trying to find.
			            String DeviceIDFromRegistry = Marshal.PtrToStringUni(PropertyValueBuffer); //Make a new string, fill it with the contents from the PropertyValueBuffer

			            Marshal.FreeHGlobal(PropertyValueBuffer);		//No longer need the PropertyValueBuffer, free the memory to prevent potential memory leaks

			            //Convert both strings to lower case.  This makes the code more robust/portable accross OS Versions
			            DeviceIDFromRegistry = DeviceIDFromRegistry.ToLowerInvariant();	
			            DeviceIDToFind = DeviceIDToFind.ToLowerInvariant();
                        DeviceIDToFind2 = DeviceIDToFind2.ToLowerInvariant();
                        //Now check if the hardware ID we are looking at contains the correct VID/PID
			            MatchFound = DeviceIDFromRegistry.Contains(DeviceIDToFind);
                        MatchFound2 = DeviceIDFromRegistry.Contains(DeviceIDToFind2);
                        if (MatchFound == true && MatchFound2 == true)
			            {
                            //Device must have been found.  In order to open I/O file handle(s), we will need the actual device path first.
				            //We can get the path by calling SetupDiGetDeviceInterfaceDetail(), however, we have to call this function twice:  The first
				            //time to get the size of the required structure/buffer to hold the detailed interface data, then a second time to actually 
				            //get the structure (after we have allocated enough memory for the structure.)
                            DetailedInterfaceDataStructure.cbSize = (uint)Marshal.SizeOf(DetailedInterfaceDataStructure);
				            //First call populates "StructureSize" with the correct value
				            SetupDiGetDeviceInterfaceDetail(DeviceInfoTable, ref InterfaceDataStructure, IntPtr.Zero, 0, ref StructureSize, IntPtr.Zero);
                            //Need to call SetupDiGetDeviceInterfaceDetail() again, this time specifying a pointer to a SP_DEVICE_INTERFACE_DETAIL_DATA buffer with the correct size of RAM allocated.
                            //First need to allocate the unmanaged buffer and get a pointer to it.
                            IntPtr pUnmanagedDetailedInterfaceDataStructure = IntPtr.Zero;  //Declare a pointer.
                            pUnmanagedDetailedInterfaceDataStructure = Marshal.AllocHGlobal((int)StructureSize);    //Reserve some unmanaged memory for the structure.
                            DetailedInterfaceDataStructure.cbSize = 6; //Initialize the cbSize parameter (4 bytes for DWORD + 2 bytes for unicode null terminator)
                            Marshal.StructureToPtr(DetailedInterfaceDataStructure, pUnmanagedDetailedInterfaceDataStructure, false); //Copy managed structure contents into the unmanaged memory buffer.

                            //Now call SetupDiGetDeviceInterfaceDetail() a second time to receive the device path in the structure at pUnmanagedDetailedInterfaceDataStructure.
                            if (SetupDiGetDeviceInterfaceDetail(DeviceInfoTable, ref InterfaceDataStructure, pUnmanagedDetailedInterfaceDataStructure, StructureSize, IntPtr.Zero, IntPtr.Zero))
                            {
                                //Need to extract the path information from the unmanaged "structure".  The path starts at (pUnmanagedDetailedInterfaceDataStructure + sizeof(DWORD)).
                                IntPtr pToDevicePath = new IntPtr((uint)pUnmanagedDetailedInterfaceDataStructure.ToInt32() + 4);  //Add 4 to the pointer (to get the pointer to point to the path, instead of the DWORD cbSize parameter)
                                DevicePath = Marshal.PtrToStringUni(pToDevicePath); //Now copy the path information into the globally defined DevicePath String.
                                
                                //We now have the proper device path, and we can finally use the path to open I/O handle(s) to the device.
                                SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure we no longer need.
                                Marshal.FreeHGlobal(pUnmanagedDetailedInterfaceDataStructure);  //No longer need this unmanaged SP_DEVICE_INTERFACE_DETAIL_DATA buffer.  We already extracted the path information.
                                return true;    //Returning the device path in the global DevicePath String
                            }
                            else //Some unknown failure occurred
                            {
                                uint ErrorCode = (uint)Marshal.GetLastWin32Error();
                                SetupDiDestroyDeviceInfoList(DeviceInfoTable);	//Clean up the old structure.
                                Marshal.FreeHGlobal(pUnmanagedDetailedInterfaceDataStructure);  //No longer need this unmanaged SP_DEVICE_INTERFACE_DETAIL_DATA buffer.  We already extracted the path information.
                                return false;    
                            }
                        }

			            InterfaceIndex++;	
			            //Keep looping until we either find a device with matching VID and PID, or until we run out of devices to check.
			            //However, just in case some unexpected error occurs, keep track of the number of loops executed.
			            //If the number of loops exceeds a very large number, exit anyway, to prevent inadvertent infinite looping.
			            LoopCounter++;
			            if(LoopCounter == 10000000)	//Surely there aren't more than 10 million devices attached to any forseeable PC...
			            {
				            return false;
			            }
		            }//end of while(true)
                }
                return false;
            }//end of try
            catch
            {
                //Something went wrong if PC gets here.  Maybe a Marshal.AllocHGlobal() failed due to insufficient resources or something.
			    return false;	
            }
        }
        //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------
        //This is a callback function that gets called when a Windows message is received by the form.
        //We will receive various different types of messages, but the ones we really want to use are the WM_DEVICECHANGE messages.
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DEVICECHANGE)
            {
                if (((int)m.WParam == DBT_DEVICEARRIVAL) || ((int)m.WParam == DBT_DEVICEREMOVEPENDING) || ((int)m.WParam == DBT_DEVICEREMOVECOMPLETE) || ((int)m.WParam == DBT_CONFIGCHANGED))
                {
                    //WM_DEVICECHANGE messages by themselves are quite generic, and can be caused by a number of different
                    //sources, not just your USB hardware device.  Therefore, must check to find out if any changes relavant
                    //to your device (with known VID/PID) took place before doing any kind of opening or closing of handles/endpoints.
                    //(the message could have been totally unrelated to your application/USB device)

                    if (CheckIfPresentAndGetUSBDevicePath())	//Check and make sure at least one device with matching VID/PID is attached
                    {
                        //If executes to here, this means the device is currently attached and was found.
                        //This code needs to decide however what to do, based on whether or not the device was previously known to be
                        //attached or not.
                        if ((AttachedState == false) || (AttachedButBroken == true))	//Check the previous attachment state
                        {
                            uint ErrorStatusWrite;
                            uint ErrorStatusRead;

                            //We obtained the proper device path (from CheckIfPresentAndGetUSBDevicePath() function call), and it
                            //is now possible to open read and write handles to the device.
                            WriteHandleToUSBDevice = CreateFile(DevicePath, GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                            ErrorStatusWrite = (uint)Marshal.GetLastWin32Error();
                            ReadHandleToUSBDevice = CreateFile(DevicePath, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                            ErrorStatusRead = (uint)Marshal.GetLastWin32Error();

                            if ((ErrorStatusWrite == ERROR_SUCCESS) && (ErrorStatusRead == ERROR_SUCCESS))
                            {
                                AttachedState = true;		//Let the rest of the PC application know the USB device is connected, and it is safe to read/write to it
                                AttachedButBroken = false;
                                StatusBox_lbl.Text = "デバイス検出済";
                            }
                            else //for some reason the device was physically plugged in, but one or both of the read/write handles didn't open successfully...
                            {
                                AttachedState = false;		//Let the rest of this application known not to read/write to the device.
                                AttachedButBroken = true;	//Flag so that next time a WM_DEVICECHANGE message occurs, can retry to re-open read/write pipes
                                if (ErrorStatusWrite == ERROR_SUCCESS)
                                    WriteHandleToUSBDevice.Close();
                                if (ErrorStatusRead == ERROR_SUCCESS)
                                    ReadHandleToUSBDevice.Close();
                            }
                        }
                        //else we did find the device, but AttachedState was already true.  In this case, don't do anything to the read/write handles,
                        //since the WM_DEVICECHANGE message presumably wasn't caused by our USB device.  
                    }
                    else	//Device must not be connected (or not programmed with correct firmware)
                    {
                        if (AttachedState == true)		//If it is currently set to true, that means the device was just now disconnected
                        {
                            AttachedState = false;
                            WriteHandleToUSBDevice.Close();
                            ReadHandleToUSBDevice.Close();
                        }
                        AttachedState = false;
                        AttachedButBroken = false;
                    }
                }
            } //end of: if(m.Msg == WM_DEVICECHANGE)

            base.WndProc(ref m);
        } //end of: WndProc() function
        //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void Changevalue_btn_Click(object sender, EventArgs e)
        {
            Changevalue_btn_pressed = true;
        }


        private void ReadWriteThread_DoWork(object sender, DoWorkEventArgs e)
        {
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
            //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------

            /*This thread does the actual USB read/write operations (but only when AttachedState == true) to the USB device.
            It is generally preferrable to write applications so that read and write operations are handled in a separate
            thread from the main form.  This makes it so that the main form can remain responsive, even if the I/O operations
            take a very long time to complete.

            Since this is a separate thread, this code below executes independently from the rest of the
            code in this application.  All this thread does is read and write to the USB device.  It does not update
            the form directly with the new information it obtains (such as the ANxx/POT Voltage or pushbutton state).
            The information that this thread obtains is stored in atomic global variables.
            Form updates are handled by the FormUpdateTimer Tick event handler function.

            This application sends packets to the endpoint buffer on the USB device by using the "WriteFile()" function.
            This application receives packets from the endpoint buffer on the USB device by using the "ReadFile()" function.
            Both of these functions are documented in the MSDN library.  Calling ReadFile() is a not perfectly straight
            foward in C# environment, since one of the input parameters is a pointer to a buffer that gets filled by ReadFile().
            The ReadFile() function is therefore called through a wrapper function ReadFileManagedBuffer().

            All ReadFile() and WriteFile() operations in this example project are synchronous.  They are blocking function
            calls and only return when they are complete, or if they fail because of some event, such as the user unplugging
            the device.  It is possible to call these functions with "overlapped" structures, and use them as non-blocking
            asynchronous I/O function calls.  

            Note:  This code may perform differently on some machines when the USB device is plugged into the host through a
            USB 2.0 hub, as opposed to a direct connection to a root port on the PC.  In some cases the data rate may be slower
            when the device is connected through a USB 2.0 hub.  This performance difference is believed to be caused by
            the issue described in Microsoft knowledge base article 940021:
            http://support.microsoft.com/kb/940021/en-us 

            Higher effective bandwidth (up to the maximum offered by interrupt endpoints), both when connected
            directly and through a USB 2.0 hub, can generally be achieved by queuing up multiple pending read and/or
            write requests simultaneously.  This can be done when using	asynchronous I/O operations (calling ReadFile() and
            WriteFile()	with overlapped structures).  The Microchip	HID USB Bootloader application uses asynchronous I/O
            for some USB operations and the source code can be used	as an example.*/


            Byte[] OUTBuffer = new byte[65];	//Allocate a memory buffer equal to the OUT endpoint size + 1
		    Byte[] INBuffer = new byte[65];		//Allocate a memory buffer equal to the IN endpoint size + 1
		    uint BytesWritten = 0;
		    uint BytesRead = 0;

		    while(true)
		    {
                try
                {
                    if (AttachedState == true)	//Do not try to use the read/write handles unless the USB device is attached and ready
                    {
                        //Get ANxx/POT Voltage value from the microcontroller firmware.  Note: some demo boards may not have a pot
                        //on them.  In this case, the firmware may be configured to read an ANxx I/O pin voltage with the ADC
                        //instead.  If this is the case, apply a proper voltage to the pin.  See the firmware for exact implementation.

                        if (ReadFromDevice == true)
                        {
                            ReadFromDevice = false;
                            OUTBuffer[0] = 0x00;	//The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
                            OUTBuffer[1] = 0x37;	//READ_POT command (see the firmware source code), gets 10-bit ADC Value
                            //Initialize the rest of the 64-byte packet to "0xFF".  Binary '1' bits do not use as much power, and do not cause as much EMI
                            //when they move across the USB cable.  USB traffic is "NRZI" encoded, where '1' bits do not cause the D+/D- signals to toggle states.
                            //This initialization is not strictly necessary however.
                            for (uint i = 2; i < 65; i++)
                                OUTBuffer[i] = 0xFF;

                            //To get the ADCValue, first, we send a packet with our "READ_POT" command in it.
                            if (WriteFile(WriteHandleToUSBDevice, OUTBuffer, 65, ref BytesWritten, IntPtr.Zero))	//Blocking function, unless an "overlapped" structure is used
                            {
                                INBuffer[0] = 0;
                                //Now get the response packet from the firmware.
                                if (ReadFileManagedBuffer(ReadHandleToUSBDevice, INBuffer, 65, ref BytesRead, IntPtr.Zero))		//Blocking function, unless an "overlapped" structure is used	
                                {
                                    //INBuffer[0] is the report ID, which we don't care about.
                                    //INBuffer[1] is an echo back of the command (see microcontroller firmware).
                                    if (INBuffer[1] == 0x37)
                                    {
                                        for (int fi = 0; fi < (NUM_OF_BUTTONS * 3) ; fi++)
                                        {
                                            eeprom_data[fi] = INBuffer[fi + 2];
                                        }
                                        for (int fi = 0; fi < (NUM_OF_BUTTONS * 3); fi++)
                                        {
                                            if (((fi % 3) == 0) && eeprom_data[fi] > 2)
                                            {   //EEPROMの内容が壊れていないか、チェック
                                                eeprom_data[fi] = 0;
                                                eeprom_data[fi + 1] = 0;
                                                eeprom_data[fi + 2] = 0;
                                            }
                                        }
                                    }
                                }
                            }
                        }


                        //Get the pushbutton state from the microcontroller firmware.
                        OUTBuffer[0] = 0;			//The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
                        OUTBuffer[1] = 0x81;		//0x81 is the "Get Pushbutton State" command in the firmware
                        for (uint i = 2; i < 65; i++)	//This loop is not strictly necessary.  Simply initializes unused bytes to
                            OUTBuffer[i] = 0xFF;				//0xFF for lower EMI and power consumption when driving the USB cable.

                        //To get the pushbutton state, first, we send a packet with our "Get Pushbutton State" command in it.
                        if (WriteFile(WriteHandleToUSBDevice, OUTBuffer, 65, ref BytesWritten, IntPtr.Zero))	//Blocking function, unless an "overlapped" structure is used
                        {
                            //Now get the response packet from the firmware.
                            INBuffer[0] = 0;
                            {
                                if (ReadFileManagedBuffer(ReadHandleToUSBDevice, INBuffer, 65, ref BytesRead, IntPtr.Zero))	//Blocking function, unless an "overlapped" structure is used
                                {
                                    //INBuffer[0] is the report ID, which we don't care about.
                                    //INBuffer[1] is an echo back of the command (see microcontroller firmware).
                                    //INBuffer[2] contains the I/O port pin value for the pushbutton (see microcontroller firmware).  
                                    if (INBuffer[1] == 0x81)
                                    {
                                        PushButonState = (uint)(INBuffer[2] + (INBuffer[3] << 8));
                                    }
                                }
                            }
                        }



                        //Check if this thread should send a Toggle LED(s) command to the firmware.  ToggleLEDsPending will get set
                        //by the ToggleLEDs_btn click event handler function if the user presses the button on the form.
                        if (Changevalue_btn_pressed == true)
                        {
                            for (uint i = 0; i < 65; i++)	//This loop is not strictly necessary.  Simply initializes unused bytes to
                                OUTBuffer[i] = 0xFF;		//0xFF for lower EMI and power consumption when driving the USB cable.

                            OUTBuffer[0] = 0;				//The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
                            OUTBuffer[1] = 0x80;			//0x80 is the "Toggle LED(s)" command in the firmware

                            // EEPROMデータの後ろに書き換えコードを追加
                            OUTBuffer[38] = 0x05;
                            OUTBuffer[39] = 0xAA;
                            OUTBuffer[40] = 0x55;
                            OUTBuffer[41] = 0x0A;
                            OUTBuffer[42] = 0x0F;

                            for (int fi = 0; fi < (NUM_OF_BUTTONS * 3); fi++)
                            {
                                OUTBuffer[fi + 2] = eeprom_data[fi];
                            }

                            OUTBuffer[SetPin_selected * 3 +  2] = DeviceType_selected;
                            eeprom_data[SetPin_selected * 3] = DeviceType_selected;

                            StatusBoxChange = SetPin_selected;

                            switch (DeviceType_selected)
                            {
                                case 0:
                                    OUTBuffer[SetPin_selected * 3 + 3] = MouseValue_selected;
                                    OUTBuffer[SetPin_selected * 3 + 4] = MouseMoveValue_selected;
                                    eeprom_data[SetPin_selected * 3 + 1] = MouseValue_selected;
                                    eeprom_data[SetPin_selected * 3 + 2] = MouseMoveValue_selected;
                                    break;
                                case 1:
                                    OUTBuffer[SetPin_selected * 3 +  3] = KeyboardValue_selected;
                                    OUTBuffer[SetPin_selected * 3 +  4] = KeyboardModifier_selected;

                                    eeprom_data[SetPin_selected * 3 +  1] = KeyboardValue_selected;
                                    eeprom_data[SetPin_selected * 3 +  2] = KeyboardModifier_selected;
                                    break;
                                case 2:
                                    OUTBuffer[SetPin_selected * 3 + 3] = LeverValue_selected;
                                    OUTBuffer[SetPin_selected * 3 + 3] |= ButtonValue_selected2;
                                    OUTBuffer[SetPin_selected * 3 + 4] = ButtonValue_selected;

                                    eeprom_data[SetPin_selected * 3 + 1] = LeverValue_selected;
                                    eeprom_data[SetPin_selected * 3 + 1] |= ButtonValue_selected2;
                                    eeprom_data[SetPin_selected * 3 + 2] = ButtonValue_selected;
                                    break;
                            }

                            //Now send the packet to the USB firmware on the microcontroller
                            WriteFile(WriteHandleToUSBDevice, OUTBuffer, 65, ref BytesWritten, IntPtr.Zero);	//Blocking function, unless an "overlapped" structure is used
                            Changevalue_btn_pressed = false;

                            ChangeAssign[SetPin_selected] = true;
                            ConnectFirstTime = true;
                        }

                        if (Reset_btn_pressed == true)
                        {
                            for (uint i = 0; i < 65; i++)	//This loop is not strictly necessary.  Simply initializes unused bytes to
                                OUTBuffer[i] = 0xFF;		//0xFF for lower EMI and power consumption when driving the USB cable.

                            OUTBuffer[0] = 0x0;				//The first byte is the "Report ID" and does not get sent over the USB bus.  Always set = 0.
                            OUTBuffer[1] = 0x55;

                            WriteFile(WriteHandleToUSBDevice, OUTBuffer, 65, ref BytesWritten, IntPtr.Zero);	//Blocking function, unless an "overlapped" structure is used
                            Reset_btn_pressed = false;
                        }

                    } //end of: if(AttachedState == true)
                    else
                    {
                        Thread.Sleep(5);	//Add a small delay.  Otherwise, this while(true) loop can execute very fast and cause 
                                            //high CPU utilization, with no particular benefit to the application.
                    }
                }
                catch
                {
                    //Exceptions can occur during the read or write operations.  For example,
                    //exceptions may occur if for instance the USB device is physically unplugged
                    //from the host while the above read/write functions are executing.

                    //Don't need to do anything special in this case.  The application will automatically
                    //re-establish communications based on the global AttachedState boolean variable used
                    //in conjunction with the WM_DEVICECHANGE messages to dyanmically respond to Plug and Play
                    //USB connection events.
                }

		    } //end of while(true) loop
            //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        }



        private void FormUpdateTimer_Tick(object sender, EventArgs e)
        {
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
            //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------
            //This timer tick event handler function is used to update the user interface on the form, based on data
            //obtained asynchronously by the ReadWriteThread and the WM_DEVICECHANGE event handler functions.

            //Check if user interface on the form should be enabled or not, based on the attachment state of the USB device.
            if (AttachedState == true)
            {
                //Device is connected and ready to communicate, enable user interface on the form 
                StatusBox_lbl.Text = "デバイス検出済";

                if (Now_Background_image == 0)
                {
//                    this.BackgroundImage = global::Revive_Micro_CT.Properties.Resources.BG1_connect;
                    BackGround_pb.Image = global::Revive_Micro_CT.Properties.Resources.NORMAL_BG_ON;
                    Now_Background_image = 1;
                }

                Label[] templbls = new Label[] { DeviceAssign_lbl1, DeviceAssign_lbl2, DeviceAssign_lbl3, DeviceAssign_lbl4, DeviceAssign_lbl5, DeviceAssign_lbl6, DeviceAssign_lbl7, DeviceAssign_lbl8, DeviceAssign_lbl9, DeviceAssign_lbl10, DeviceAssign_lbl11, DeviceAssign_lbl12};
                Label[] templbls2 = new Label[] { devicetype_lbl1, devicetype_lbl2, devicetype_lbl3, devicetype_lbl4, devicetype_lbl5, devicetype_lbl6, devicetype_lbl7, devicetype_lbl8, devicetype_lbl9, devicetype_lbl10, devicetype_lbl11, devicetype_lbl12};
                

                if (ConnectFirstTime == true)
                {
                    //                    devicetype_combox.Text = st_device_type[eeprom_data[0]].text;

                    Changevalue_btn.Enabled = true;
                    SetPin_combox.Enabled = true;
                    devicetype_combox.Enabled = true;

                    Status_C_pb.Visible = true;
                    Status_NC_pb.Visible = false;

                    SetPin_combox.SelectedIndex = SetPin_selected;
                    devicetype_combox.SelectedIndex = (int)eeprom_data[SetPin_selected * 3];

                    for (uint fi = 0; fi < NUM_OF_BUTTONS; fi++)
                    {
                        if (ChangeAssign[fi] == true)
                        {
                            ChangeAssign[fi] = false;
                            switch (eeprom_data[fi * 3])
                            {
                                case 0:
                                    templbls2[fi].Text = "マウス";

                                    switch (eeprom_data[fi * 3 + 1])
                                    {
                                        case 0:
                                            templbls[fi].Text = "左クリック";
                                            break;
                                        case 1:
                                            templbls[fi].Text = "右クリック";
                                            break;
                                        case 2:
                                            templbls[fi].Text = "ホイールクリック";
                                            break;
                                        case 3:
                                            templbls[fi].Text = eeprom_data[fi * 3 + 2].ToString();
                                            templbls[fi].Text = templbls[fi].Text.Insert(0, "上移動 ");
                                            break;
                                        case 4:
                                            templbls[fi].Text = eeprom_data[fi * 3 + 2].ToString();
                                            templbls[fi].Text = templbls[fi].Text.Insert(0, "下移動 ");
                                            break;
                                        case 5:
                                            templbls[fi].Text = eeprom_data[fi * 3 + 2].ToString();
                                            templbls[fi].Text = templbls[fi].Text.Insert(0, "左移動 ");
                                            break;
                                        case 6:
                                            templbls[fi].Text = eeprom_data[fi * 3 + 2].ToString();
                                            templbls[fi].Text = templbls[fi].Text.Insert(0, "右移動 ");
                                            break;
                                        case 7:
                                            templbls[fi].Text = eeprom_data[fi * 3 + 2].ToString();
                                            templbls[fi].Text = templbls[fi].Text.Insert(0, "ホイール上 ");
                                            break;
                                        case 8:
                                            templbls[fi].Text = eeprom_data[fi * 3 + 2].ToString();
                                            templbls[fi].Text = templbls[fi].Text.Insert(0, "ホイール下 ");
                                            break;
                                        case 9:
                                            templbls[fi].Text = eeprom_data[fi * 3 + 2].ToString();
                                            templbls[fi].Text = templbls[fi].Text.Insert(0, "移動速度変更 ");
                                            break;

                                    }
                                    break;
                                case 1:
                                    templbls2[fi].Text = "キーボード";

                                    for (uint i = 0; i < 256; i++)
                                    {
                                        if (eeprom_data[fi * 3 + 1] == VKtoUSBkey[i])
                                        {
                                            templbls[fi].Text = USB_KeyCode_Name[VKtoUSBkey[i]];
                                        }
                                    }

                                    if ((eeprom_data[fi * 3 + 2] & 0x01) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, "Ctrl + ");

                                    if ((eeprom_data[fi * 3 + 2] & 0x02) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, "Shift + ");

                                    if ((eeprom_data[fi * 3 + 2] & 0x04) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, "Alt + ");

                                    if ((eeprom_data[fi * 3 + 2] & 0x08) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, "Win + ");

                                    break;
                                case 2:
                                    templbls2[fi].Text = "ジョイパッド";
                                    templbls[fi].Text = "";
                                    if ((eeprom_data[fi * 3 + 1] & 0x01) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " 上");

                                    if ((eeprom_data[fi * 3 + 1] & 0x02) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " 下");

                                    if ((eeprom_data[fi * 3 + 1] & 0x04) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " 左");

                                    if ((eeprom_data[fi * 3 + 1] & 0x08) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " 右");

                                    if ((eeprom_data[fi * 3 + 1] & 0x80) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B12");

                                    if ((eeprom_data[fi * 3 + 1] & 0x40) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B11");

                                    if ((eeprom_data[fi * 3 + 1] & 0x20) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B10");

                                    if ((eeprom_data[fi * 3 + 1] & 0x10) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B9");

                                    if ((eeprom_data[fi * 3 + 2] & 0x80) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B8");

                                    if ((eeprom_data[fi * 3 + 2] & 0x40) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B7");

                                    if ((eeprom_data[fi * 3 + 2] & 0x20) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B6");

                                    if ((eeprom_data[fi * 3 + 2] & 0x10) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B5");

                                    if ((eeprom_data[fi * 3 + 2] & 0x08) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B4");

                                    if ((eeprom_data[fi * 3 + 2] & 0x04) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B3");

                                    if ((eeprom_data[fi * 3 + 2] & 0x02) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B2");

                                    if ((eeprom_data[fi * 3 + 2] & 0x01) != 0)
                                        templbls[fi].Text = templbls[fi].Text.Insert(0, " B1");

                                    break;
                            }
                        }
                    }
                    ConnectFirstTime = false;
                }
                if (StatusBoxChange != 99)
                {
                    StatusBox_lbl2.Text = " ]に設定しました";
                    StatusBox_lbl2.Text = StatusBox_lbl2.Text.Insert(0, templbls[StatusBoxChange].Text);
                    StatusBox_lbl2.Text = StatusBox_lbl2.Text.Insert(0, " / ");
                    StatusBox_lbl2.Text = StatusBox_lbl2.Text.Insert(0, templbls2[StatusBoxChange].Text);
                    StatusBox_lbl2.Text = StatusBox_lbl2.Text.Insert(0, "を[ ");
                    StatusBox_lbl2.Text = StatusBox_lbl2.Text.Insert(0, SetPin_combox.Text );
                    StatusBoxChange = 99;
                }
            }
            if ((AttachedState == false) || (AttachedButBroken == true))
            {
                //Device not available to communicate. Disable user interface on the form.
                StatusBox_lbl.Text = "デバイス未検出";
                if (Now_Background_image == 1)
                {
                    BackGround_pb.Image = global::Revive_Micro_CT.Properties.Resources.NORMAL_BG_OFF;
//                    this.BackgroundImage = global::Revive_Micro_CT.Properties.Resources.BG1_notconnect;
                    Now_Background_image = 0;
                    Status_NC_pb.Visible = true;
                    Status_C_pb.Visible = false;

                    Changevalue_btn.Enabled = false;
                    SetPin_combox.Enabled = false;
                    devicetype_combox.Enabled = false;
                    update_btn.Enabled = false;
                }

                for (int fi = 0; fi < NUM_OF_BUTTONS; fi++)
                    ChangeAssign[fi] = true;
                ConnectFirstTime = true;
            }

            PictureBox[] TempButton = new PictureBox[] { ButtonPressIcon1, ButtonPressIcon2, ButtonPressIcon3, ButtonPressIcon4, ButtonPressIcon5, ButtonPressIcon6, ButtonPressIcon7, ButtonPressIcon8, ButtonPressIcon9, ButtonPressIcon10, ButtonPressIcon11, ButtonPressIcon12};
            PictureBox[] TempPin = new PictureBox[] {Pin01B_pb,Pin02B_pb,Pin03B_pb,Pin04B_pb,Pin05B_pb,Pin06B_pb,Pin07B_pb,Pin08B_pb,Pin09B_pb,Pin10B_pb,Pin11B_pb,Pin12B_pb};
            for (int i = 0; i < TempPin.Length; i++)
            {
                TempPin[i].Parent = BackGround_pb;
            }
            for (int i = 0; i < TempButton.Length; i++) {
                TempButton[i].Parent = BackGround_pb;           
            }

            //Update the various status indicators on the form with the latest info obtained from the ReadWriteThread()
            if (AttachedState == true)
            {
                update_btn.Enabled = true;
                for (int fi = 0; fi < NUM_OF_BUTTONS; fi++)
                {
                    if ((PushButonState & (0x01 << fi)) != 0)
                    {
                        TempButton[fi].Visible = true;
//                        SetPin_combox.SelectedIndex = fi;
                    }
                    else
                        TempButton[fi].Visible = false;
                }
            }
 
            //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
            //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------BEGIN CUT AND PASTE BLOCK-----------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------------------
        //FUNCTION:	ReadFileManagedBuffer()
        //PURPOSE:	Wrapper function to call ReadFile()
        //
        //INPUT:	Uses managed versions of the same input parameters as ReadFile() uses.
        //
        //OUTPUT:	Returns boolean indicating if the function call was successful or not.
        //          Also returns data in the byte[] INBuffer, and the number of bytes read. 
        //
        //Notes:    Wrapper function used to call the ReadFile() function.  ReadFile() takes a pointer to an unmanaged buffer and deposits
        //          the bytes read into the buffer.  However, can't pass a pointer to a managed buffer directly to ReadFile().
        //          This ReadFileManagedBuffer() is a wrapper function to make it so application code can call ReadFile() easier
        //          by specifying a managed buffer.
        //--------------------------------------------------------------------------------------------------------------------------
        public unsafe bool ReadFileManagedBuffer(SafeFileHandle hFile, byte[] INBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, IntPtr lpOverlapped)
        {
            IntPtr pINBuffer = IntPtr.Zero;

            try
            {
                pINBuffer = Marshal.AllocHGlobal((int)nNumberOfBytesToRead);    //Allocate some unmanged RAM for the receive data buffer.

                if (ReadFile(hFile, pINBuffer, nNumberOfBytesToRead, ref lpNumberOfBytesRead, lpOverlapped))
                {
                    Marshal.Copy(pINBuffer, INBuffer, 0, (int)lpNumberOfBytesRead);    //Copy over the data from unmanged memory into the managed byte[] INBuffer
                    Marshal.FreeHGlobal(pINBuffer);
                    return true;
                }
                else
                {
                    Marshal.FreeHGlobal(pINBuffer);
                    return false;
                }

            }
            catch
            {
                if (pINBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pINBuffer);
                }
                return false;
            }
        }

        private void devicetype_combox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeviceType_selected = (byte)devicetype_combox.SelectedIndex;

            switch (DeviceType_selected)
            {
                case 0:
                    mousevalue_combx.Visible = true;
                    KeyboardValue_txtbx.Visible = false;
                    Alt_cbox.Visible = false;
                    Ctrl_cbox.Visible = false;
                    Shift_cbox.Visible = false;
                    Win_cbox.Visible = false;
                    LeverDown_cbox.Visible = false;
                    LeverLeft_cbox.Visible = false;
                    LeverRight_cbox.Visible = false;
                    LeverUp_cbox.Visible = false;
                    Button1_cbox.Visible = false;
                    Button2_cbox.Visible = false;
                    Button3_cbox.Visible = false;
                    Button4_cbox.Visible = false;
                    Button5_cbox.Visible = false;
                    Button6_cbox.Visible = false;
                    Button7_cbox.Visible = false;
                    Button8_cbox.Visible = false;
                    Button9_cbox.Visible = false;
                    Button10_cbox.Visible = false;
                    Button11_cbox.Visible = false;
                    Button12_cbox.Visible = false;
                    Arrow_Keyboard_pb.Visible = false;

                    Arrow_Com_pb.Visible = false;

                    if (eeprom_data[SetPin_selected * 3 + 1] > 9)
                        eeprom_data[SetPin_selected * 3 + 1] = 0;

                    mousevalue_combx.Items.Add("ダミー");
                    mousevalue_combx.SelectedIndex = 10;
                    mousevalue_combx.SelectedIndex = eeprom_data[SetPin_selected * 3 + 1];
                    mousevalue_combx.Items.Remove("ダミー");

                    break;
                case 1:
                    mousevalue_combx.Visible = false;
                    KeyboardValue_txtbx.Visible = true;
                    Alt_cbox.Visible = true;
                    Ctrl_cbox.Visible = true;
                    Shift_cbox.Visible = true;
                    Win_cbox.Visible = true;
                    LeverDown_cbox.Visible = false;
                    LeverLeft_cbox.Visible = false;
                    LeverRight_cbox.Visible = false;
                    LeverUp_cbox.Visible = false;
                    Button1_cbox.Visible = false;
                    Button2_cbox.Visible = false;
                    Button3_cbox.Visible = false;
                    Button4_cbox.Visible = false;
                    Button5_cbox.Visible = false;
                    Button6_cbox.Visible = false;
                    Button7_cbox.Visible = false;
                    Button8_cbox.Visible = false;
                    Button9_cbox.Visible = false;
                    Button10_cbox.Visible = false;
                    Button11_cbox.Visible = false;
                    Button12_cbox.Visible = false;
                    Arrow_Keyboard_pb.Visible = true;

                    Arrow_Mouse1_pb.Visible = false;
                    Arrow_Mouse2_pb.Visible = false;
                    Arrow_Mouse3_pb.Visible = false;
                    Speed_Mouse4_pb.Visible = false;
                    Speed_Mouse5_pb.Visible = false;
                    Arrow_Com_pb.Visible = true;

                    for (uint i = 0; i < 256; i++)
                    {
                        if (eeprom_data[SetPin_selected * 3 + 1] == VKtoUSBkey[i])
                        {
                            KeyboardValue_txtbx.Text = USB_KeyCode_Name[VKtoUSBkey[i]];
                        }
                    }
                    if (eeprom_data[SetPin_selected * 3 + 1] == 0)
                        KeyboardValue_txtbx.Text = "ここに入力";

                    if ((eeprom_data[SetPin_selected * 3 + 2] & 0x01) != 0)
                        Ctrl_cbox.Checked = true;
                    else
                        Ctrl_cbox.Checked = false;

                    if ((eeprom_data[SetPin_selected * 3 +  2] & 0x02) != 0)
                        Shift_cbox.Checked = true;
                    else
                        Shift_cbox.Checked = false;

                    if ((eeprom_data[SetPin_selected * 3 + 2] & 0x04) != 0)
                        Alt_cbox.Checked = true;
                    else
                        Alt_cbox.Checked = false;

                    if ((eeprom_data[SetPin_selected * 3 + 2] & 0x08) != 0)
                        Win_cbox.Checked = true;
                    else
                        Win_cbox.Checked = false;

                    MouseMove_UD.Visible = false;

                    break;
                case 2:
                    mousevalue_combx.Visible = false;
                    KeyboardValue_txtbx.Visible = false;
                    Alt_cbox.Visible = false;
                    Ctrl_cbox.Visible = false;
                    Shift_cbox.Visible = false;
                    Win_cbox.Visible = false;
                    LeverDown_cbox.Visible = true;
                    LeverLeft_cbox.Visible = true;
                    LeverRight_cbox.Visible = true;
                    LeverUp_cbox.Visible = true;
                    Button1_cbox.Visible = true;
                    Button2_cbox.Visible = true;
                    Button3_cbox.Visible = true;
                    Button4_cbox.Visible = true;
                    Button5_cbox.Visible = true;
                    Button6_cbox.Visible = true;
                    Button7_cbox.Visible = true;
                    Button8_cbox.Visible = true;
                    Button9_cbox.Visible = true;
                    Button10_cbox.Visible = true;
                    Button11_cbox.Visible = true;
                    Button12_cbox.Visible = true;

                    Arrow_Mouse1_pb.Visible = false;
                    Arrow_Mouse2_pb.Visible = false;
                    Arrow_Mouse3_pb.Visible = false;
                    Speed_Mouse4_pb.Visible = false;
                    Speed_Mouse5_pb.Visible = false;
                    Arrow_Com_pb.Visible = true;

                    ButtonValue_selected = 0;
                    ButtonValue_selected2 = 0;
                    LeverUp_cbox.Checked = false;
                    LeverDown_cbox.Checked = false;
                    LeverLeft_cbox.Checked = false;
                    LeverRight_cbox.Checked = false;
                    Button12_cbox.Checked = false;
                    Button11_cbox.Checked = false;
                    Button10_cbox.Checked = false;
                    Button9_cbox.Checked = false;
                    Button8_cbox.Checked = false;
                    Button7_cbox.Checked = false;
                    Button6_cbox.Checked = false;
                    Button5_cbox.Checked = false;
                    Button4_cbox.Checked = false;
                    Button3_cbox.Checked = false;
                    Button2_cbox.Checked = false;
                    Button1_cbox.Checked = false;

                    Arrow_Keyboard_pb.Visible = false;

                    if ((eeprom_data[SetPin_selected * 3 + 1] & 0x01) != 0)
                        LeverUp_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 1] & 0x02) != 0)
                        LeverDown_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 1] & 0x04) != 0)
                        LeverLeft_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 1] & 0x08) != 0)
                        LeverRight_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 1] & 0x80) != 0)
                        Button12_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 1] & 0x40) != 0)
                        Button11_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 1] & 0x20) != 0)
                        Button10_cbox.Checked = true;
                    
                    if ((eeprom_data[SetPin_selected * 3 + 1] & 0x10) != 0)
                        Button9_cbox.Checked = true;
                    
                    if ((eeprom_data[SetPin_selected * 3 + 2] & 0x80) != 0)
                        Button8_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 2] & 0x40) != 0)
                        Button7_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 2] & 0x20) != 0)
                        Button6_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 2] & 0x10) != 0)
                        Button5_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 2] & 0x08) != 0)
                        Button4_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 2] & 0x04) != 0)
                        Button3_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 2] & 0x02) != 0)
                        Button2_cbox.Checked = true;

                    if ((eeprom_data[SetPin_selected * 3 + 2] & 0x01) != 0)
                        Button1_cbox.Checked = true;

                    MouseMove_UD.Visible = false;

                    break;
            }
        }

        private void mousevalue_combx_SelectedIndexChanged(object sender, EventArgs e)
        {
            MouseValue_selected = (byte)mousevalue_combx.SelectedIndex;

            if (MouseValue_selected > 2)
            {
                MouseMove_UD.Visible = true;
                Arrow_Mouse1_pb.Visible = true;
                Arrow_Mouse2_pb.Visible = false;
                Arrow_Mouse3_pb.Visible = true;
                Speed_Mouse4_pb.Visible = true;
                Speed_Mouse5_pb.Visible = true;
                if (eeprom_data[SetPin_selected * 3 + 2] == 0)
                    eeprom_data[SetPin_selected * 3 + 2] = 50;
                MouseMove_UD.Value = eeprom_data[SetPin_selected * 3 + 2];
            }
            else
            {
                Arrow_Mouse1_pb.Visible = false;
                Arrow_Mouse2_pb.Visible = true;
                Arrow_Mouse3_pb.Visible = false;
                Speed_Mouse4_pb.Visible = false;
                Speed_Mouse5_pb.Visible = false;
                MouseMove_UD.Visible = false;
            }
        }

        private void KeyboardValue_txtbx_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Tab)
                {
                    KeyboardValue_txtbx.Text = USB_KeyCode_Name[VKtoUSBkey[e.KeyValue.GetHashCode()]];
                    KeyboardValue_selected = VKtoUSBkey[e.KeyValue.GetHashCode()];
                    e.IsInputKey = true;
                }
            }
            catch
            {
            }
        }

        private void KeyboardValue_txtbx_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                //if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Alt)
                //{
                //    e.Handled = true;
                //}
                //else
                //{
                //    KeyboardValue_txtbx.Text = USB_KeyCode_Name[VKtoUSBkey[e.KeyValue.GetHashCode()]];
                //    KeyboardValue_selected = VKtoUSBkey[e.KeyValue.GetHashCode()];
                //    e.SuppressKeyPress = true;
                //}
                KeyboardValue_txtbx.Text = USB_KeyCode_Name[VKtoUSBkey[e.KeyValue.GetHashCode()]];
                KeyboardValue_selected = VKtoUSBkey[e.KeyValue.GetHashCode()];
                e.SuppressKeyPress = true;
            }
            catch
            {
            }
        }

        private void KeyboardValue_txtbx_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.PrintScreen)
                {
                    KeyboardValue_txtbx.Text = USB_KeyCode_Name[VKtoUSBkey[e.KeyValue.GetHashCode()]];
                    KeyboardValue_selected = VKtoUSBkey[e.KeyValue.GetHashCode()];
                    e.SuppressKeyPress = true;
                }
            }
            catch
            {
            }
        }

        private void Ctrl_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Ctrl_cbox.Checked)
                KeyboardModifier_selected |= 0x01;
            else
                KeyboardModifier_selected &= 0xfe;
        }

        private void Alt_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Alt_cbox.Checked)
                KeyboardModifier_selected |= 0x04;
            else
                KeyboardModifier_selected &= 0xfb;
        }

        private void Shift_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Shift_cbox.Checked)
                KeyboardModifier_selected |= 0x02;
            else
                KeyboardModifier_selected &= 0xfd;
        }

        private void Win_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Win_cbox.Checked)
                KeyboardModifier_selected |= 0x08;
            else
                KeyboardModifier_selected &= 0xf7;
        }

        private void LeverUp_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (LeverUp_cbox.Checked)
                LeverValue_selected |= 0x01;
            else
                LeverValue_selected &= 0xfe;
        }

        private void LeverDown_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (LeverDown_cbox.Checked)
                LeverValue_selected |= 0x02;
            else
                LeverValue_selected &= 0xfd;
        }

        private void LeverLeft_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (LeverLeft_cbox.Checked)
                LeverValue_selected |= 0x04;
            else
                LeverValue_selected &= 0xfb;
        }

        private void LeverRight_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (LeverRight_cbox.Checked)
                LeverValue_selected |= 0x08;
            else
                LeverValue_selected &= 0xf7;
        }

        private void Button1_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button1_cbox.Checked)
                ButtonValue_selected |= 0x01;
            else
                ButtonValue_selected &= 0xfe;
        }

        private void Button2_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button2_cbox.Checked)
                ButtonValue_selected |= 0x02;
            else
                ButtonValue_selected &= 0xfd;
        }

        private void Button3_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button3_cbox.Checked)
                ButtonValue_selected |= 0x04;
            else
                ButtonValue_selected &= 0xfb;
        }

        private void Button4_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button4_cbox.Checked)
                ButtonValue_selected |= 0x08;
            else
                ButtonValue_selected &= 0xf7;
        }

        private void Button5_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button5_cbox.Checked)
                ButtonValue_selected |= 0x10;
            else
                ButtonValue_selected &= 0xef;
        }

        private void Button6_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button6_cbox.Checked)
                ButtonValue_selected |= 0x20;
            else
                ButtonValue_selected &= 0xdf;
        }

        private void Onetime_cbox_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void SetPin_combox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PictureBox[] tmp_picB = new PictureBox[] { Pin01B_pb, Pin02B_pb, Pin03B_pb, Pin04B_pb, Pin05B_pb, Pin06B_pb, Pin07B_pb, Pin08B_pb, Pin09B_pb, Pin10B_pb, Pin11B_pb, Pin12B_pb };
            System.Drawing.Bitmap[] resources = new System.Drawing.Bitmap[]{
            Revive_Micro_CT.Properties.Resources.NORMAL_01_ON,
            Revive_Micro_CT.Properties.Resources.NORMAL_02_ON,
            Revive_Micro_CT.Properties.Resources.NORMAL_03_ON,
            Revive_Micro_CT.Properties.Resources.NORMAL_04_ON,
            Revive_Micro_CT.Properties.Resources.NORMAL_05_ON,
            Revive_Micro_CT.Properties.Resources.NORMAL_06_ON,
            Revive_Micro_CT.Properties.Resources.NORMAL_07_ON,
            Revive_Micro_CT.Properties.Resources.NORMAL_08_ON,
            Revive_Micro_CT.Properties.Resources.NORMAL_09_ON,
            Revive_Micro_CT.Properties.Resources.NORMAL_10_ON,
            Revive_Micro_CT.Properties.Resources.NORMAL_11_ON,
            Revive_Micro_CT.Properties.Resources.NORMAL_12_ON,
            };

            tmp_picB[SetPin_selected].Image = null;

            SetPin_selected = SetPin_combox.SelectedIndex;

            tmp_picB[SetPin_selected].Image = resources[SetPin_selected];
            devicetype_combox.Items.Add("ダミー");
            devicetype_combox.SelectedIndex = 3;
            devicetype_combox.SelectedIndex = eeprom_data[SetPin_selected * 3];
            devicetype_combox.Items.Remove("ダミー");
        }

        private void PIN1_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 0;
        }

        private void PIN2_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 1;
        }

        private void PIN3_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 2;
        }

        private void PIN4_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 3;
        }

        private void PIN5_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 4;
        }

        private void PIN6_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 5;
        }

        private void PIN7_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 6;
        }

        private void PIN8_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 7;
        }

        private void PIN9_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 8;
        }

        private void PIN10_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 9;
        }

        private void PIN11_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 10;
        }

        private void PIN12_lbl_Click(object sender, EventArgs e)
        {
            SetPin_combox.SelectedIndex = 11;
        }

        private void Button7_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button7_cbox.Checked)
                ButtonValue_selected |= 0x40;
            else
                ButtonValue_selected &= 0xbf;
        }

        private void Button8_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button8_cbox.Checked)
                ButtonValue_selected |= 0x80;
            else
                ButtonValue_selected &= 0x7f;
        }

        private void Button9_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button9_cbox.Checked)
                ButtonValue_selected2 |= 0x10;
            else
                ButtonValue_selected2 &= 0xef;
        }

        private void Button10_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button10_cbox.Checked)
                ButtonValue_selected2 |= 0x20;
            else
                ButtonValue_selected2 &= 0xdf;
        }

        private void Button11_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button11_cbox.Checked)
                ButtonValue_selected2 |= 0x40;
            else
                ButtonValue_selected2 &= 0xbf;
        }

        private void Button12_cbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Button12_cbox.Checked)
                ButtonValue_selected2 |= 0x80;
            else
                ButtonValue_selected2 &= 0x7f;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            MouseMoveValue_selected = (byte)MouseMove_UD.Value;
        }

        private void ButtonRightA_pb_Click(object sender, EventArgs e)
        {
            if(Now_Background_image == 1)
                SetPin_combox.SelectedIndex = 3;
        }

        private void ButtonUpA_pb_Click(object sender, EventArgs e)
        {
            if (Now_Background_image == 1)
                SetPin_combox.SelectedIndex = 0;
        }

        private void ButtonLeftA_pb_Click(object sender, EventArgs e)
        {
            if (Now_Background_image == 1)
                SetPin_combox.SelectedIndex = 2;
        }

        private void ButtonDownA_pb_Click(object sender, EventArgs e)
        {
            if (Now_Background_image == 1)
                SetPin_combox.SelectedIndex = 1;
        }

        private void Button01A_pb_Click(object sender, EventArgs e)
        {
            if (Now_Background_image == 1)
                SetPin_combox.SelectedIndex = 4;
        }

        private void Button02A_pb_Click(object sender, EventArgs e)
        {
            if (Now_Background_image == 1)
                SetPin_combox.SelectedIndex = 5;
        }

        private void Button03A_pb_Click(object sender, EventArgs e)
        {
            if (Now_Background_image == 1)
                SetPin_combox.SelectedIndex = 6;
        }

        private void Status_NC_pb_Click(object sender, EventArgs e)
        {

        }

        private void update_btn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("ファームウェア書き換えを行いますか？この処理はマニュアルを読みながら行って下さい。", "ファームウェア書き換え", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Reset_btn_pressed = true;
            }
        }

        private void Changevalue_btn_MouseEnter(object sender, EventArgs e)
        {
            Changevalue_btn.BackgroundImage = Revive_Micro_CT.Properties.Resources.DONE_MOUSEON;
        }

        private void Changevalue_btn_MouseLeave(object sender, EventArgs e)
        {
            Changevalue_btn.BackgroundImage = Revive_Micro_CT.Properties.Resources.DONE;
        }




        //-------------------------------------------------------END CUT AND PASTE BLOCK-------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------
    } //public partial class Form1 : Form
} //namespace HID_PnP_Demo