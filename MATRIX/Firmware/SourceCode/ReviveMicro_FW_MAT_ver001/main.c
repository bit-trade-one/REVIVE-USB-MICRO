// USB HID core
//	ver 1	2019/06/19	USB core最初のバージョン


/********************************************************************
 FileName:		main.c
 Dependencies:	See INCLUDES section
 Processor:		PIC18, PIC24, and PIC32 USB Microcontrollers
 Hardware:		This demo is natively intended to be used on Microchip USB demo
 				boards supported by the MCHPFSUSB stack.  See release notes for
 				support matrix.  This demo can be modified for use on other hardware
 				platforms.
 Complier:  	Microchip C18 (for PIC18), C30 (for PIC24), C32 (for PIC32)
 Company:		Microchip Technology, Inc.

 Software License Agreement:

 The software supplied herewith by Microchip Technology Incorporated
 (the 鼎ompany・ for its PICｮ Microcontroller is intended and
 supplied to you, the Company痴 customer, for use solely and
 exclusively on Microchip PIC Microcontroller products. The
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
********************************************************************/

#ifndef JOYSTICKMAIN_C
#define JOYSTICKMAIN_C

/** INCLUDES *******************************************************/
#include <adc.h>
#include <string.h>
#include "./USB/usb.h"
#include "HardwareProfile.h"
#include "./USB/usb_function_hid.h"
#include "eeprom.h"

/** CONFIGURATION **************************************************/
#pragma config CPUDIV = NOCLKDIV
#pragma config USBDIV = OFF
#pragma config FOSC   = HS
#pragma config PLLEN  = ON
#pragma config FCMEN  = OFF
#pragma config IESO   = OFF
#pragma config PWRTEN = OFF
#pragma config BOREN  = OFF
#pragma config BORV   = 30
#pragma config WDTEN  = OFF
#pragma config WDTPS  = 32768
#pragma config MCLRE  = OFF
#pragma config HFOFST = OFF
#pragma config STVREN = ON
#pragma config LVP    = OFF
#pragma config XINST  = OFF
#pragma config BBSIZ  = OFF
#pragma config CP0    = OFF
#pragma config CP1    = OFF
#pragma config CPB    = OFF
#pragma config WRT0   = OFF
#pragma config WRT1   = OFF
#pragma config WRTB   = OFF
#pragma config WRTC   = OFF
#pragma config EBTR0  = OFF
#pragma config EBTR1  = OFF
#pragma config EBTRB  = OFF


/** PRIVATE PROTOTYPES *********************************************/
static void InitializeSystem(void);
void ProcessIO(void);
void UserInit(void);
void YourHighPriorityISRCode();
void YourLowPriorityISRCode();

/** DECLARATIONS ***************************************************/
#define MODE_MOUSE		0
#define MODE_KEYBOARD	1
#define	MODE_JOYSTICK	2
#define	EEPROM_DATA_MODE		0	//	0:モード
#define	EEPROM_DATA_VALUE		1	//	1:値
#define	EEPROM_DATA_MODIFIER	2	//	2:Modifier（キーボード用）

#define MASTER_MOUSE_SPEED		50	//	Mouseの移動速度の調整値
#define MASTER_WHEEL_SPEED		1000

#define MOVE_OFF	0
#define MOVE_ON		1

#define NUM_OF_PINS		36

#define PIN_OUT1	LATCbits.LATC5
#define	PIN_OUT2	LATCbits.LATC4
#define	PIN_OUT3	LATCbits.LATC3
#define PIN_OUT4	LATCbits.LATC6
#define	PIN_OUT5	LATCbits.LATC7
#define	PIN_OUT6	LATBbits.LATB7
#define	PIN_IN1		PORTBbits.RB6
#define	PIN_IN2		PORTBbits.RB5
#define	PIN_IN3		PORTBbits.RB4
#define	PIN_IN4		PORTCbits.RC2
#define PIN_IN5		PORTCbits.RC1
#define	PIN_IN6		PORTCbits.RC0


/** VARIABLES ******************************************************/
#pragma udata
char	c_version[]="M_M001";
BYTE mouse_buffer[4];
BYTE joystick_buffer[4];
BYTE keyboard_buffer[8]; 
USB_HANDLE lastTransmission;
USB_HANDLE lastTransmission2;
USB_HANDLE lastINTransmissionKeyboard;
USB_HANDLE lastOUTTransmissionKeyboard;
USB_HANDLE USBOutHandle = 0;
USB_HANDLE USBInHandle = 0;

unsigned char result_button_press_set[6] = {0,0,0,0,0,0};

char mouse_move_up;
char mouse_move_down;
char mouse_move_left;
char mouse_move_right;
char mouse_wheel_up;
char mouse_wheel_down;

int temp_mouse_move_up = 0;
int temp_mouse_move_down = 0;
int temp_mouse_move_left = 0;
int temp_mouse_move_right = 0;
int temp_mouse_wheel_up = 0;
int temp_mouse_wheel_down = 0;

unsigned char speed_mouse_move_up;
unsigned char speed_mouse_move_down;
unsigned char speed_mouse_move_left;
unsigned char speed_mouse_move_right;
unsigned char speed_mouse_wheel_up;
unsigned char speed_mouse_wheel_down;

//ボタン設定用変数

unsigned char eeprom_data[NUM_OF_PINS][3] = {	{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},
												{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},
												{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},
												{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},
												{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},
												{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},{0,0,0},
												};

// usbでの送信に使うバッファはここで宣言
#pragma udata usbram2

BYTE joystick_input[4];
BYTE mouse_input[4];
BYTE hid_report[8];
unsigned char joystick_input_out_flag = 0;
unsigned char mouse_input_out_flag = 0;
unsigned char hid_report_out_flag = 0;
unsigned char ReceivedDataBuffer[64];
unsigned char ToSendDataBuffer[64];

#pragma udata

/** VECTOR REMAPPING ***********************************************/
#if defined(__18CXX)
	//On PIC18 devices, addresses 0x00, 0x08, and 0x18 are used for
	//the reset, high priority interrupt, and low priority interrupt
	//vectors.  However, the current Microchip USB bootloader 
	//examples are intended to occupy addresses 0x00-0x7FF or
	//0x00-0xFFF depending on which bootloader is used.  Therefore,
	//the bootloader code remaps these vectors to new locations
	//as indicated below.  This remapping is only necessary if you
	//wish to program the hex file generated from this project with
	//the USB bootloader.  If no bootloader is used, edit the
	//usb_config.h file and comment out the following defines:
	//#define PROGRAMMABLE_WITH_USB_HID_BOOTLOADER
	//#define PROGRAMMABLE_WITH_USB_LEGACY_CUSTOM_CLASS_BOOTLOADER
	
	#if defined(PROGRAMMABLE_WITH_USB_HID_BOOTLOADER)
		#define REMAPPED_RESET_VECTOR_ADDRESS			0x1000
		#define REMAPPED_HIGH_INTERRUPT_VECTOR_ADDRESS	0x1008
		#define REMAPPED_LOW_INTERRUPT_VECTOR_ADDRESS	0x1018
	#elif defined(PROGRAMMABLE_WITH_USB_MCHPUSB_BOOTLOADER)	
		#define REMAPPED_RESET_VECTOR_ADDRESS			0x800
		#define REMAPPED_HIGH_INTERRUPT_VECTOR_ADDRESS	0x808
		#define REMAPPED_LOW_INTERRUPT_VECTOR_ADDRESS	0x818
	#else	
		#define REMAPPED_RESET_VECTOR_ADDRESS			0x00
		#define REMAPPED_HIGH_INTERRUPT_VECTOR_ADDRESS	0x08
		#define REMAPPED_LOW_INTERRUPT_VECTOR_ADDRESS	0x18
	#endif
	
	#if defined(PROGRAMMABLE_WITH_USB_HID_BOOTLOADER)||defined(PROGRAMMABLE_WITH_USB_MCHPUSB_BOOTLOADER)
	extern void _startup (void);        // See c018i.c in your C18 compiler dir
	#pragma code REMAPPED_RESET_VECTOR = REMAPPED_RESET_VECTOR_ADDRESS
	void _reset (void)
	{
	    _asm goto _startup _endasm
	}
	#endif
	#pragma code REMAPPED_HIGH_INTERRUPT_VECTOR = REMAPPED_HIGH_INTERRUPT_VECTOR_ADDRESS
	void Remapped_High_ISR (void)
	{
	     _asm goto YourHighPriorityISRCode _endasm
	}
	#pragma code REMAPPED_LOW_INTERRUPT_VECTOR = REMAPPED_LOW_INTERRUPT_VECTOR_ADDRESS
	void Remapped_Low_ISR (void)
	{
	     _asm goto YourLowPriorityISRCode _endasm
	}
	
	#if defined(PROGRAMMABLE_WITH_USB_HID_BOOTLOADER)||defined(PROGRAMMABLE_WITH_USB_MCHPUSB_BOOTLOADER)
	//Note: If this project is built while one of the bootloaders has
	//been defined, but then the output hex file is not programmed with
	//the bootloader, addresses 0x08 and 0x18 would end up programmed with 0xFFFF.
	//As a result, if an actual interrupt was enabled and occured, the PC would jump
	//to 0x08 (or 0x18) and would begin executing "0xFFFF" (unprogrammed space).  This
	//executes as nop instructions, but the PC would eventually reach the REMAPPED_RESET_VECTOR_ADDRESS
	//(0x1000 or 0x800, depending upon bootloader), and would execute the "goto _startup".  This
	//would effective reset the application.
	
	//To fix this situation, we should always deliberately place a 
	//"goto REMAPPED_HIGH_INTERRUPT_VECTOR_ADDRESS" at address 0x08, and a
	//"goto REMAPPED_LOW_INTERRUPT_VECTOR_ADDRESS" at address 0x18.  When the output
	//hex file of this project is programmed with the bootloader, these sections do not
	//get bootloaded (as they overlap the bootloader space).  If the output hex file is not
	//programmed using the bootloader, then the below goto instructions do get programmed,
	//and the hex file still works like normal.  The below section is only required to fix this
	//scenario.
	#pragma code HIGH_INTERRUPT_VECTOR = 0x08
	void High_ISR (void)
	{
	     _asm goto REMAPPED_HIGH_INTERRUPT_VECTOR_ADDRESS _endasm
	}
	#pragma code LOW_INTERRUPT_VECTOR = 0x18
	void Low_ISR (void)
	{
	     _asm goto REMAPPED_LOW_INTERRUPT_VECTOR_ADDRESS _endasm
	}
	#endif	//end of "#if defined(PROGRAMMABLE_WITH_USB_HID_BOOTLOADER)||defined(PROGRAMMABLE_WITH_USB_LEGACY_CUSTOM_CLASS_BOOTLOADER)"

	#pragma code
	
	
	//These are your actual interrupt handling routines.
	#pragma interrupt YourHighPriorityISRCode
	void YourHighPriorityISRCode()
	{
		//Check which interrupt flag caused the interrupt.
		//Service the interrupt
		//Clear the interrupt flag
		//Etc.
        #if defined(USB_INTERRUPT)
	        USBDeviceTasks();
        #endif
	
	}	//This return will be a "retfie fast", since this is in a #pragma interrupt section 
	#pragma interruptlow YourLowPriorityISRCode
	void YourLowPriorityISRCode()
	{
		//Check which interrupt flag caused the interrupt.
		//Service the interrupt
		//Clear the interrupt flag
		//Etc.
	
	}	//This return will be a "retfie", since this is in a #pragma interruptlow section 
#endif

#pragma code

/********************************************************************
 * Function:        void main(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        Main program entry point.
 *
 * Note:            None
 *******************************************************************/
void main(void)
{   
    InitializeSystem();

    #if defined(USB_INTERRUPT)
        USBDeviceAttach();
    #endif

    while(1)
    {
        #if defined(USB_POLLING)
		// Check bus status and service USB interrupts.
        USBDeviceTasks(); // Interrupt or polling method.  If using polling, must call
        				  // this function periodically.  This function will take care
        				  // of processing and responding to SETUP transactions 
        				  // (such as during the enumeration process when you first
        				  // plug in).  USB hosts require that USB devices should accept
        				  // and process SETUP packets in a timely fashion.  Therefore,
        				  // when using polling, this function should be called 
        				  // frequently (such as once about every 100 microseconds) at any
        				  // time that a SETUP packet might reasonably be expected to
        				  // be sent by the host to your device.  In most cases, the
        				  // USBDeviceTasks() function does not take very long to
        				  // execute (~50 instruction cycles) before it returns.
        #endif
    				  

		// Application-specific tasks.
		// Application related code may be added here, or in the ProcessIO() function.
        ProcessIO();        
    }//end while
}//end main


/********************************************************************
 * Function:        static void InitializeSystem(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        InitializeSystem is a centralize initialization
 *                  routine. All required USB initialization routines
 *                  are called from here.
 *
 *                  User application initialization routine should
 *                  also be called from here.                  
 *
 * Note:            None
 *******************************************************************/
static void InitializeSystem(void)
{
    ADCON1 |= 0x0F;                 // Default all pins to digital

//	The USB specifications require that USB peripheral devices must never source
//	current onto the Vbus pin.  Additionally, USB peripherals should not source
//	current on D+ or D- when the host/hub is not actively powering the Vbus line.
//	When designing a self powered (as opposed to bus powered) USB peripheral
//	device, the firmware should make sure not to turn on the USB module and D+
//	or D- pull up resistor unless Vbus is actively powered.  Therefore, the
//	firmware needs some means to detect when Vbus is being powered by the host.
//	A 5V tolerant I/O pin can be connected to Vbus (through a resistor), and
// 	can be used to detect when Vbus is high (host actively powering), or low
//	(host is shut down or otherwise not supplying power).  The USB firmware
// 	can then periodically poll this I/O pin to know when it is okay to turn on
//	the USB module/D+/D- pull up resistor.  When designing a purely bus powered
//	peripheral device, it is not possible to source current on D+ or D- when the
//	host is not actively providing power on Vbus. Therefore, implementing this
//	bus sense feature is optional.  This firmware can be made to use this bus
//	sense feature by making sure "USE_USB_BUS_SENSE_IO" has been defined in the
//	HardwareProfile.h file.    
    #if defined(USE_USB_BUS_SENSE_IO)
    tris_usb_bus_sense = INPUT_PIN; // See HardwareProfile.h
    #endif
    
//	If the host PC sends a GetStatus (device) request, the firmware must respond
//	and let the host know if the USB peripheral device is currently bus powered
//	or self powered.  See chapter 9 in the official USB specifications for details
//	regarding this request.  If the peripheral device is capable of being both
//	self and bus powered, it should not return a hard coded value for this request.
//	Instead, firmware should check if it is currently self or bus powered, and
//	respond accordingly.  If the hardware has been configured like demonstrated
//	on the PICDEM FS USB Demo Board, an I/O pin can be polled to determine the
//	currently selected power source.  On the PICDEM FS USB Demo Board, "RA2" 
//	is used for	this purpose.  If using this feature, make sure "USE_SELF_POWER_SENSE_IO"
//	has been defined in HardwareProfile.h, and that an appropriate I/O pin has been mapped
//	to it in HardwareProfile.h.
    #if defined(USE_SELF_POWER_SENSE_IO)
    tris_self_power = INPUT_PIN;	// See HardwareProfile.h
    #endif
    
    UserInit();

    USBDeviceInit();	//usb_device.c.  Initializes USB module SFRs and firmware
    					//variables to known states.
}//end InitializeSystem



/******************************************************************************
 * Function:        void UserInit(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        This routine should take care of all of the demo code
 *                  initialization that is required.
 *
 * Note:            
 *
 *****************************************************************************/
void UserInit(void)
{
	int fi,fj;
	char mode_changedp = 1;

	TRISB = 0x70;	//RB4,5,6を入力
	TRISC = 0x07;	//RC0,1,2を入力

	LATC = 0xff;

	ANSEL = 0x00;
	ANSELH = 0x00;	//全てデジタル

    //initialize the variable holding the handle for the last
    // transmission
    lastTransmission = 0;
    lastTransmission2 = 0;
    lastINTransmissionKeyboard = 0;
    lastOUTTransmissionKeyboard = 0;
    USBOutHandle = 0;
    USBInHandle = 0;

    joystick_input[0] = 
    joystick_input[1] = 
    joystick_input[2] =
    joystick_input[3] = 0;
    
    mouse_input[0] = 
    mouse_input[1] = 
    mouse_input[2] = 0;

	hid_report_in[0] = 
	hid_report_in[1] = 
	hid_report_in[2] = 
	hid_report_in[3] = 
	hid_report_in[4] = 
	hid_report_in[5] = 
	hid_report_in[6] = 
	hid_report_in[7] = 0;

	mouse_buffer[0] =
	mouse_buffer[1] =
	mouse_buffer[2] =
	mouse_buffer[3] = 0;

	keyboard_buffer[0] = 
	keyboard_buffer[1] = 
	keyboard_buffer[2] = 
	keyboard_buffer[3] = 
	keyboard_buffer[4] = 
	keyboard_buffer[5] = 
	keyboard_buffer[6] = 
	keyboard_buffer[7] = 0;

	joystick_buffer[0] = 0;
	joystick_buffer[1] = 0;
	joystick_buffer[2] = 0x80;
	joystick_buffer[3] = 0x80;

//EEPROMのボタン設定値を読み込み
	for(fi = 0;fi < NUM_OF_PINS; fi++)
	{
		for(fj = 0;fj < 3;fj++)
		{
			eeprom_data[fi][fj] = ReadEEPROM(fi*3+fj) ;
		}
	}
	
	if(eeprom_data[0][0] != 0x00 && eeprom_data[0][0] != 0x01 && eeprom_data[0][0] != 0x02)
	{//初期化されていたら、全部0にする
		for(fi = 0;fi < NUM_OF_PINS; fi++)
		{
			for(fj = 0;fj < 3;fj++)
			{
				eeprom_data[fi][fj] = 0x00;
				WriteEEPROM(fi*3+fj,0x00);
			}
		}
	}
}//end UserInit


/********************************************************************
 * Function:        void ProcessIO(void)
 *	
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        This function is a place holder for other user
 *                  routines. It is a mixture of both USB and
 *                  non-USB tasks.
 *
 * Note:            None
 *******************************************************************/
void ProcessIO(void)
{
	int fi,fj;
	int pressed_keys;
	char result_button_press;
	char tmp;
	char result;

    // User Application USB tasks
    if((USBDeviceState < CONFIGURED_STATE)||(USBSuspendControl==1)) return;


//--------------------------------------------------------------------
//	ボタン決定部
//	一列目
	PIN_OUT1 = 0;	PIN_OUT2 = 1;	PIN_OUT3 = 1;	PIN_OUT4 = 1;	PIN_OUT5 = 1;	PIN_OUT6 = 1;

	if(!PIN_IN1)	result_button_press_set[0] |= 0x01;
		else		result_button_press_set[0] &= 0xfe;
	if(!PIN_IN2)	result_button_press_set[0] |= 0x02;
		else		result_button_press_set[0] &= 0xfd;
	if(!PIN_IN3)	result_button_press_set[0] |= 0x04;
		else		result_button_press_set[0] &= 0xfb;
	if(!PIN_IN4)	result_button_press_set[0] |= 0x08;
		else		result_button_press_set[0] &= 0xf7;
	if(!PIN_IN5)	result_button_press_set[0] |= 0x10;
		else		result_button_press_set[0] &= 0xef;
	if(!PIN_IN6)	result_button_press_set[0] |= 0x20;
		else		result_button_press_set[0] &= 0xdf;

//	二列目
	PIN_OUT2 = 0;	PIN_OUT1 = 1;	PIN_OUT3 = 1;	PIN_OUT4 = 1;	PIN_OUT5 = 1;	PIN_OUT6 = 1;

	if(!PIN_IN1)	result_button_press_set[1] |= 0x01;
		else		result_button_press_set[1] &= 0xfe;
	if(!PIN_IN2)	result_button_press_set[1] |= 0x02;
		else		result_button_press_set[1] &= 0xfd;
	if(!PIN_IN3)	result_button_press_set[1] |= 0x04;
		else		result_button_press_set[1] &= 0xfb;
	if(!PIN_IN4)	result_button_press_set[1] |= 0x08;
		else		result_button_press_set[1] &= 0xf7;
	if(!PIN_IN5)	result_button_press_set[1] |= 0x10;
		else		result_button_press_set[1] &= 0xef;
	if(!PIN_IN6)	result_button_press_set[1] |= 0x20;
		else		result_button_press_set[1] &= 0xdf;

//	三列目
	PIN_OUT3 = 0;	PIN_OUT2 = 1;	PIN_OUT1 = 1;	PIN_OUT4 = 1;	PIN_OUT5 = 1;	PIN_OUT6 = 1;

	if(!PIN_IN1)	result_button_press_set[2] |= 0x01;
		else		result_button_press_set[2] &= 0xfe;
	if(!PIN_IN2)	result_button_press_set[2] |= 0x02;
		else		result_button_press_set[2] &= 0xfd;
	if(!PIN_IN3)	result_button_press_set[2] |= 0x04;
		else		result_button_press_set[2] &= 0xfb;
	if(!PIN_IN4)	result_button_press_set[2] |= 0x08;
		else		result_button_press_set[2] &= 0xf7;
	if(!PIN_IN5)	result_button_press_set[2] |= 0x10;
		else		result_button_press_set[2] &= 0xef;
	if(!PIN_IN6)	result_button_press_set[2] |= 0x20;
		else		result_button_press_set[2] &= 0xdf;

//	四列目
	PIN_OUT4 = 0;	PIN_OUT3 = 1;	PIN_OUT1 = 1;	PIN_OUT2 = 1;	PIN_OUT5 = 1;	PIN_OUT6 = 1;

	if(!PIN_IN1)	result_button_press_set[3] |= 0x01;
		else		result_button_press_set[3] &= 0xfe;
	if(!PIN_IN2)	result_button_press_set[3] |= 0x02;
		else		result_button_press_set[3] &= 0xfd;
	if(!PIN_IN3)	result_button_press_set[3] |= 0x04;
		else		result_button_press_set[3] &= 0xfb;
	if(!PIN_IN4)	result_button_press_set[3] |= 0x08;
		else		result_button_press_set[3] &= 0xf7;
	if(!PIN_IN5)	result_button_press_set[3] |= 0x10;
		else		result_button_press_set[3] &= 0xef;
	if(!PIN_IN6)	result_button_press_set[3] |= 0x20;
		else		result_button_press_set[3] &= 0xdf;

//	五列目
	PIN_OUT5 = 0;	PIN_OUT4 = 1;	PIN_OUT1 = 1;	PIN_OUT2 = 1;	PIN_OUT3 = 1;	PIN_OUT6 = 1;

	if(!PIN_IN1)	result_button_press_set[4] |= 0x01;
		else		result_button_press_set[4] &= 0xfe;
	if(!PIN_IN2)	result_button_press_set[4] |= 0x02;
		else		result_button_press_set[4] &= 0xfd;
	if(!PIN_IN3)	result_button_press_set[4] |= 0x04;
		else		result_button_press_set[4] &= 0xfb;
	if(!PIN_IN4)	result_button_press_set[4] |= 0x08;
		else		result_button_press_set[4] &= 0xf7;
	if(!PIN_IN5)	result_button_press_set[4] |= 0x10;
		else		result_button_press_set[4] &= 0xef;
	if(!PIN_IN6)	result_button_press_set[4] |= 0x20;
		else		result_button_press_set[4] &= 0xdf;

//	六列目
	PIN_OUT6 = 0;	PIN_OUT5 = 1;	PIN_OUT1 = 1;	PIN_OUT2 = 1;	PIN_OUT3 = 1;	PIN_OUT4 = 1;

	if(!PIN_IN1)	result_button_press_set[5] |= 0x01;
		else		result_button_press_set[5] &= 0xfe;
	if(!PIN_IN2)	result_button_press_set[5] |= 0x02;
		else		result_button_press_set[5] &= 0xfd;
	if(!PIN_IN3)	result_button_press_set[5] |= 0x04;
		else		result_button_press_set[5] &= 0xfb;
	if(!PIN_IN4)	result_button_press_set[5] |= 0x08;
		else		result_button_press_set[5] &= 0xf7;
	if(!PIN_IN5)	result_button_press_set[5] |= 0x10;
		else		result_button_press_set[5] &= 0xef;
	if(!PIN_IN6)	result_button_press_set[5] |= 0x20;
		else		result_button_press_set[5] &= 0xdf;

	PIN_OUT6 = 1;

//何も押されていないときの為にバッファを初期化しておく
	pressed_keys = 2;
	mouse_move_up = MOVE_OFF;
	mouse_move_down = MOVE_OFF;
	mouse_move_left = MOVE_OFF;
	mouse_move_right = MOVE_OFF;
	mouse_wheel_up = MOVE_OFF;
	mouse_wheel_down = MOVE_OFF;
	speed_mouse_move_up = 0;
	speed_mouse_move_down = 0;
	speed_mouse_move_left = 0;
	speed_mouse_move_right = 0;



	for(fi = 0;fi < NUM_OF_PINS ; fi++)
	{
		result_button_press = ((result_button_press_set[(fi/6)] & (0x01 << (fi % 6))) ? 1 : 0);

		switch(eeprom_data[fi][EEPROM_DATA_MODE])
		{
			case MODE_MOUSE:
				if(result_button_press == 1)
				{
					if(eeprom_data[fi][EEPROM_DATA_VALUE] == 0)  // 左クリック
						mouse_buffer[0] |= 1;
					else if(eeprom_data[fi][EEPROM_DATA_VALUE] == 1)  // 右クリック
						mouse_buffer[0] |= 0x02;
					else if(eeprom_data[fi][EEPROM_DATA_VALUE] == 2)  // ホイールクリック
						mouse_buffer[0] |= 0x04;
					else if(eeprom_data[fi][EEPROM_DATA_VALUE] == 3) // 上移動
					{
						mouse_move_up = MOVE_ON;
						if(speed_mouse_move_up == 0)
							speed_mouse_move_up = eeprom_data[fi][EEPROM_DATA_MODIFIER];
					}
					else if(eeprom_data[fi][EEPROM_DATA_VALUE] == 4) // 下移動
					{
						mouse_move_down = MOVE_ON;
						if(speed_mouse_move_down == 0)
							speed_mouse_move_down = eeprom_data[fi][EEPROM_DATA_MODIFIER];
					}
					else if(eeprom_data[fi][EEPROM_DATA_VALUE] == 5) //左移動
					{
						mouse_move_left = MOVE_ON;
						if(speed_mouse_move_left == 0)
							speed_mouse_move_left = eeprom_data[fi][EEPROM_DATA_MODIFIER];
					}
					else if(eeprom_data[fi][EEPROM_DATA_VALUE] == 6) // 右移動
					{
						mouse_move_right = MOVE_ON;
						if(speed_mouse_move_right == 0)
							speed_mouse_move_right = eeprom_data[fi][EEPROM_DATA_MODIFIER];
					}
					else if(eeprom_data[fi][EEPROM_DATA_VALUE] == 7) //ホイール上
					{
						mouse_wheel_up = MOVE_ON;
						speed_mouse_wheel_up = eeprom_data[fi][EEPROM_DATA_MODIFIER];
					}
					else if(eeprom_data[fi][EEPROM_DATA_VALUE] == 8) //ホイール下
					{
						mouse_wheel_down = MOVE_ON;
						speed_mouse_wheel_down = eeprom_data[fi][EEPROM_DATA_MODIFIER];
					}	
					else if(eeprom_data[fi][EEPROM_DATA_VALUE] == 9) //マウス移動速度変更
					{
						speed_mouse_move_up = eeprom_data[fi][EEPROM_DATA_MODIFIER];
						speed_mouse_move_down = eeprom_data[fi][EEPROM_DATA_MODIFIER];
						speed_mouse_move_left = eeprom_data[fi][EEPROM_DATA_MODIFIER];
						speed_mouse_move_right = eeprom_data[fi][EEPROM_DATA_MODIFIER];						
					}
					mouse_input_out_flag = 5;
				}
				break;

			case MODE_KEYBOARD:
				if(result_button_press == 1)
				{
					if(pressed_keys != 8)
					{
						keyboard_buffer[0] |= eeprom_data[fi][EEPROM_DATA_MODIFIER];
						keyboard_buffer[pressed_keys] = eeprom_data[fi][EEPROM_DATA_VALUE];
						pressed_keys++;
					}	
					hid_report_out_flag = 5;	
				}
				break;

			case MODE_JOYSTICK:
				if(result_button_press == 1)
				{
					if(eeprom_data[fi][EEPROM_DATA_VALUE] & 0x01) //上
						joystick_buffer[3] = 0x0;
					if(eeprom_data[fi][EEPROM_DATA_VALUE] & 0x02) //下
						joystick_buffer[3] = 0xff;
					if(eeprom_data[fi][EEPROM_DATA_VALUE] & 0x04) //左
						joystick_buffer[2] = 0x0;
					if(eeprom_data[fi][EEPROM_DATA_VALUE] & 0x08) //右
						joystick_buffer[2] = 0xff;

					joystick_buffer[0] |= eeprom_data[fi][EEPROM_DATA_MODIFIER];
					joystick_buffer[1] |= (eeprom_data[fi][EEPROM_DATA_VALUE] >> 4);
					
					joystick_input_out_flag = 5;
				}
				break;
			default:
				break;
		}
	}	

//---------------------------------------------------------------------
//	USBデータ送信部
    if(!HIDTxHandleBusy(lastTransmission))
    {
	    //マウスの移動処理
	    if(mouse_move_up == MOVE_ON)
	    {	//上移動
			temp_mouse_move_up+=speed_mouse_move_up;
			if(temp_mouse_move_up > MASTER_MOUSE_SPEED)
			{
				mouse_buffer[2] = -1 * (temp_mouse_move_up / MASTER_MOUSE_SPEED);
				temp_mouse_move_up = temp_mouse_move_up % MASTER_MOUSE_SPEED;
			}
		}
		else
			temp_mouse_move_up = 0;

	    if(mouse_move_down == MOVE_ON)
	    {	//下移動
			temp_mouse_move_down+=speed_mouse_move_down;
			if(temp_mouse_move_down > MASTER_MOUSE_SPEED)
			{
				mouse_buffer[2] = (temp_mouse_move_down / MASTER_MOUSE_SPEED);
				temp_mouse_move_down = temp_mouse_move_down % MASTER_MOUSE_SPEED;
			}
		}
		else
			temp_mouse_move_down = 0;

	    if(mouse_move_left == MOVE_ON)
	    {	//左移動
			temp_mouse_move_left+=speed_mouse_move_left;
			if(temp_mouse_move_left > MASTER_MOUSE_SPEED)
			{
				mouse_buffer[1] = -1 * (temp_mouse_move_left / MASTER_MOUSE_SPEED);
				temp_mouse_move_left = temp_mouse_move_left % MASTER_MOUSE_SPEED;
			}
		}
		else
			temp_mouse_move_left = 0;

	    if(mouse_move_right == MOVE_ON)
	    {	//右移動
			temp_mouse_move_right+=speed_mouse_move_right;
			if(temp_mouse_move_right > MASTER_MOUSE_SPEED)
			{
				mouse_buffer[1] = (temp_mouse_move_right / MASTER_MOUSE_SPEED);
				temp_mouse_move_right = temp_mouse_move_right % MASTER_MOUSE_SPEED;
			}
		}
		else
			temp_mouse_move_right = 0;

	    if(mouse_wheel_up == MOVE_ON)
	    {	//ホイール上
			temp_mouse_wheel_up+=speed_mouse_wheel_up;
			if(temp_mouse_wheel_up > MASTER_WHEEL_SPEED)
			{
				mouse_buffer[3] = (temp_mouse_wheel_up / MASTER_WHEEL_SPEED);
				temp_mouse_wheel_up = temp_mouse_wheel_up % MASTER_WHEEL_SPEED;
			}
		}
		else
			temp_mouse_wheel_up = 0;

	    if(mouse_wheel_down == MOVE_ON)
	    {	//ホイール下
			temp_mouse_wheel_down+=speed_mouse_wheel_down;
			if(temp_mouse_wheel_down > MASTER_WHEEL_SPEED)
			{
				mouse_buffer[3] = -1 * (temp_mouse_wheel_down / MASTER_WHEEL_SPEED);
				temp_mouse_wheel_down = temp_mouse_wheel_down % MASTER_WHEEL_SPEED;
			}
		}
		else
			temp_mouse_wheel_down = 0;

        //copy over the data to the HID buffer
        //マウスデータの送信
        mouse_input[0] = mouse_buffer[0];
        mouse_input[1] = mouse_buffer[1];
        mouse_input[2] = mouse_buffer[2];
        mouse_input[3] = mouse_buffer[3];

		mouse_buffer[0] =0;
		mouse_buffer[1] =0;
		mouse_buffer[2] =0;
		mouse_buffer[3] =0;

		if( mouse_input_out_flag > 0 )
		{
	        //Send the 8 byte packet over USB to the host.
	        lastTransmission = HIDTxPacket(HID_EP, (BYTE*)&mouse_input, sizeof(mouse_input));
	        mouse_input_out_flag--;
	 	}       
    }
    if(!HIDTxHandleBusy(lastINTransmissionKeyboard))
    {	       	//Load the HID buffer
    	hid_report_in[0] = keyboard_buffer[0];
    	hid_report_in[1] = keyboard_buffer[1];
		hid_report_in[2] = keyboard_buffer[2];
	    hid_report_in[3] = keyboard_buffer[3];
    	hid_report_in[4] = keyboard_buffer[4];
    	hid_report_in[5] = keyboard_buffer[5];
    	hid_report_in[6] = keyboard_buffer[6];
    	hid_report_in[7] = keyboard_buffer[7];

		keyboard_buffer[0] =
		keyboard_buffer[1] =
		keyboard_buffer[2] =
		keyboard_buffer[3] =
		keyboard_buffer[4] =
		keyboard_buffer[5] =
		keyboard_buffer[6] =
		keyboard_buffer[7] = 0;

		if( hid_report_out_flag > 0 )
		{
	    	//Send the 8 byte packet over USB to the host.
			lastINTransmissionKeyboard = HIDTxPacket(HID_EP3, (BYTE*)hid_report_in, 0x08);
			hid_report_out_flag--;
		}
	}
   if(!HIDTxHandleBusy(lastTransmission2))
    {
        //Buttons
        joystick_input[0] = joystick_buffer[0];
        //X-Y
        joystick_input[1] = joystick_buffer[1];
        joystick_input[2] = joystick_buffer[2];
        joystick_input[3] = joystick_buffer[3];

		joystick_buffer[0] = 0;
		joystick_buffer[1] = 0;
		joystick_buffer[2] = 0x80;
		joystick_buffer[3] = 0x80;

		if( joystick_input_out_flag > 0 )
		{
        	lastTransmission2 = HIDTxPacket(HID_EP2, (BYTE*)&joystick_input, sizeof(joystick_input));
        	joystick_input_out_flag--;
		}      	
    }
    
//---------------------------------------------------------------------
//	USBデータ通信部
    if(!HIDRxHandleBusy(USBOutHandle))				//Check if data was received from the host.
    {
        switch(ReceivedDataBuffer[0])
        {
            case 0x80:  // EEPROMにデータを設定	//ピン番号,データの順で格納
            	eeprom_data[ReceivedDataBuffer[1]][0] = ReceivedDataBuffer[2];
            	eeprom_data[ReceivedDataBuffer[1]][1] = ReceivedDataBuffer[3];
            	eeprom_data[ReceivedDataBuffer[1]][2] = ReceivedDataBuffer[4];
				WriteEEPROM(ReceivedDataBuffer[1]*3,eeprom_data[ReceivedDataBuffer[1]][0]);
				WriteEEPROM(ReceivedDataBuffer[1]*3+1,eeprom_data[ReceivedDataBuffer[1]][1]);
				WriteEEPROM(ReceivedDataBuffer[1]*3+2,eeprom_data[ReceivedDataBuffer[1]][2]);
                break;
            case 0x81:  //ボタンの押下状態を返信
                ToSendDataBuffer[0] = 0x81;				//Echo back to the host PC the command we are fulfilling in the first byte.  In this case, the Get Pushbutton State command.
				ToSendDataBuffer[1] = result_button_press_set[0] & 0x3f;
				ToSendDataBuffer[2] = result_button_press_set[1] & 0x3f;
				ToSendDataBuffer[3] = result_button_press_set[2] & 0x3f;
				ToSendDataBuffer[4] = result_button_press_set[3] & 0x3f;
				ToSendDataBuffer[5] = result_button_press_set[4] & 0x3f;
				ToSendDataBuffer[6] = result_button_press_set[5] & 0x3f;

                if(!HIDTxHandleBusy(USBInHandle))
                {
                    USBInHandle = HIDTxPacket(HID_EP4,(BYTE*)&ToSendDataBuffer[0],64);
                }
                break;

            case 0x37:	//現在の設定状況を返信その1
                {		//データは一度に64byteしか送れないので、二回に分けて送る
	                if(!HIDTxHandleBusy(USBInHandle))
	                {
						ToSendDataBuffer[0] = 0x37;  	//Echo back to the host the command we are fulfilling in the first byte.  In this case, the Read POT (analog voltage) command.
						for(fi = 0;fi < 18; fi++)
						{
							for(fj = 0;fj < 3;fj++)
							{
								ToSendDataBuffer[fi*3+fj+1] = eeprom_data[fi][fj];
							}
						}

		                if(!HIDTxHandleBusy(USBInHandle))
        		        {
	            	        USBInHandle = HIDTxPacket(HID_EP4,(BYTE*)&ToSendDataBuffer[0],64);
	            	    }    
	                }
                }
                break;
            case 0x38:	//現在の設定状況を返信その2
                {
	                if(!HIDTxHandleBusy(USBInHandle))
	                {
						ToSendDataBuffer[0] = 0x38;  	//Echo back to the host the command we are fulfilling in the first byte.  In this case, the Read POT (analog voltage) command.
						for(fi = 0;fi < 18; fi++)
						{
							for(fj = 0;fj < 3;fj++)
							{
								ToSendDataBuffer[fi*3+fj+1] = eeprom_data[(fi+18)][fj];
							}
						}

		                if(!HIDTxHandleBusy(USBInHandle))
        		        {
	            	        USBInHandle = HIDTxPacket(HID_EP4,(BYTE*)&ToSendDataBuffer[0],64);
	            	    }    
	                }
                }
                break;
            case 0x55:	//ソフトリセットをかける
				UCONbits.SUSPND = 0;		//Disable USB module
				UCON = 0x00;				//Disable USB module
				for(fi = 0; fi < 0xFF; fi++)
				{
					WREG = 0xFF;
					while(WREG)
					{
						WREG--;
						_asm
						bra	0	//Equivalent to bra $+2, which takes half as much code as 2 nop instructions
						bra	0	//Equivalent to bra $+2, which takes half as much code as 2 nop instructions
						_endasm	
					}
				}
            	Reset();
            	break;
            case 0x56: // V=0x56 Get Firmware version
                ToSendDataBuffer[0] = 0x56;				//Echo back to the host PC the command we are fulfilling in the first byte.  In this case, the Get Pushbutton State command.
				tmp = strlen(c_version);
				if( 0 < tmp && tmp <= (64-2) )
				{
					for( fi = 0; fi < tmp; fi++ )
					{
						ToSendDataBuffer[fi+1] = c_version[fi];
					}
					// 最後にNULL文字を設定
					ToSendDataBuffer[fi+1] = 0x00;
				}
				else
				{
					//バージョン文字列異常
					ToSendDataBuffer[1] = 0x00;
				}				
                if(!HIDTxHandleBusy(USBInHandle))
                {
                    USBInHandle = HIDTxPacket(HID_EP4,(BYTE*)&ToSendDataBuffer[0],64);
                }
                break;
        }
         //Re-arm the OUT endpoint for the next packet
        USBOutHandle = HIDRxPacket(HID_EP4,(BYTE*)&ReceivedDataBuffer,64);
    }
}




// ******************************************************************************************************
// ************** USB Callback Functions ****************************************************************
// ******************************************************************************************************
// The USB firmware stack will call the callback functions USBCBxxx() in response to certain USB related
// events.  For example, if the host PC is powering down, it will stop sending out Start of Frame (SOF)
// packets to your device.  In response to this, all USB devices are supposed to decrease their power
// consumption from the USB Vbus to <2.5mA each.  The USB module detects this condition (which according
// to the USB specifications is 3+ms of no bus activity/SOF packets) and then calls the USBCBSuspend()
// function.  You should modify these callback functions to take appropriate actions for each of these
// conditions.  For example, in the USBCBSuspend(), you may wish to add code that will decrease power
// consumption from Vbus to <2.5mA (such as by clock switching, turning off LEDs, putting the
// microcontroller to sleep, etc.).  Then, in the USBCBWakeFromSuspend() function, you may then wish to
// add code that undoes the power saving things done in the USBCBSuspend() function.

// The USBCBSendResume() function is special, in that the USB stack will not automatically call this
// function.  This function is meant to be called from the application firmware instead.  See the
// additional comments near the function.

/******************************************************************************
 * Function:        void USBCBSuspend(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        Call back that is invoked when a USB suspend is detected
 *
 * Note:            None
 *****************************************************************************/
void USBCBSuspend(void)
{
	//Example power saving code.  Insert appropriate code here for the desired
	//application behavior.  If the microcontroller will be put to sleep, a
	//process similar to that shown below may be used:
	
	//ConfigureIOPinsForLowPower();
	//SaveStateOfAllInterruptEnableBits();
	//DisableAllInterruptEnableBits();
	//EnableOnlyTheInterruptsWhichWillBeUsedToWakeTheMicro();	//should enable at least USBActivityIF as a wake source
	//Sleep();
	//RestoreStateOfAllPreviouslySavedInterruptEnableBits();	//Preferrably, this should be done in the USBCBWakeFromSuspend() function instead.
	//RestoreIOPinsToNormal();									//Preferrably, this should be done in the USBCBWakeFromSuspend() function instead.

	//IMPORTANT NOTE: Do not clear the USBActivityIF (ACTVIF) bit here.  This bit is 
	//cleared inside the usb_device.c file.  Clearing USBActivityIF here will cause 
	//things to not work as intended.	
	

    #if defined(__C30__)
    #if 0
        U1EIR = 0xFFFF;
        U1IR = 0xFFFF;
        U1OTGIR = 0xFFFF;
        IFS5bits.USB1IF = 0;
        IEC5bits.USB1IE = 1;
        U1OTGIEbits.ACTVIE = 1;
        U1OTGIRbits.ACTVIF = 1;
        Sleep();
    #endif
    #endif
}


/******************************************************************************
 * Function:        void _USB1Interrupt(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        This function is called when the USB interrupt bit is set
 *					In this example the interrupt is only used when the device
 *					goes to sleep when it receives a USB suspend command
 *
 * Note:            None
 *****************************************************************************/
#if 0
void __attribute__ ((interrupt)) _USB1Interrupt(void)
{
    #if !defined(self_powered)
        if(U1OTGIRbits.ACTVIF)
        {       
            IEC5bits.USB1IE = 0;
            U1OTGIEbits.ACTVIE = 0;
            IFS5bits.USB1IF = 0;
        
            //USBClearInterruptFlag(USBActivityIFReg,USBActivityIFBitNum);
            USBClearInterruptFlag(USBIdleIFReg,USBIdleIFBitNum);
            //USBSuspendControl = 0;
        }
    #endif
}
#endif

/******************************************************************************
 * Function:        void USBCBWakeFromSuspend(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        The host may put USB peripheral devices in low power
 *					suspend mode (by "sending" 3+ms of idle).  Once in suspend
 *					mode, the host may wake the device back up by sending non-
 *					idle state signalling.
 *					
 *					This call back is invoked when a wakeup from USB suspend 
 *					is detected.
 *
 * Note:            None
 *****************************************************************************/
void USBCBWakeFromSuspend(void)
{
	// If clock switching or other power savings measures were taken when
	// executing the USBCBSuspend() function, now would be a good time to
	// switch back to normal full power run mode conditions.  The host allows
	// a few milliseconds of wakeup time, after which the device must be 
	// fully back to normal, and capable of receiving and processing USB
	// packets.  In order to do this, the USB module must receive proper
	// clocking (IE: 48MHz clock must be available to SIE for full speed USB
	// operation).
}

/********************************************************************
 * Function:        void USBCB_SOF_Handler(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        The USB host sends out a SOF packet to full-speed
 *                  devices every 1 ms. This interrupt may be useful
 *                  for isochronous pipes. End designers should
 *                  implement callback routine as necessary.
 *
 * Note:            None
 *******************************************************************/
void USBCB_SOF_Handler(void)
{
    // No need to clear UIRbits.SOFIF to 0 here.
    // Callback caller is already doing that.
}

/*******************************************************************
 * Function:        void USBCBErrorHandler(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        The purpose of this callback is mainly for
 *                  debugging during development. Check UEIR to see
 *                  which error causes the interrupt.
 *
 * Note:            None
 *******************************************************************/
void USBCBErrorHandler(void)
{
    // No need to clear UEIR to 0 here.
    // Callback caller is already doing that.

	// Typically, user firmware does not need to do anything special
	// if a USB error occurs.  For example, if the host sends an OUT
	// packet to your device, but the packet gets corrupted (ex:
	// because of a bad connection, or the user unplugs the
	// USB cable during the transmission) this will typically set
	// one or more USB error interrupt flags.  Nothing specific
	// needs to be done however, since the SIE will automatically
	// send a "NAK" packet to the host.  In response to this, the
	// host will normally retry to send the packet again, and no
	// data loss occurs.  The system will typically recover
	// automatically, without the need for application firmware
	// intervention.
	
	// Nevertheless, this callback function is provided, such as
	// for debugging purposes.
}


/*******************************************************************
 * Function:        void USBCBCheckOtherReq(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        When SETUP packets arrive from the host, some
 * 					firmware must process the request and respond
 *					appropriately to fulfill the request.  Some of
 *					the SETUP packets will be for standard
 *					USB "chapter 9" (as in, fulfilling chapter 9 of
 *					the official USB specifications) requests, while
 *					others may be specific to the USB device class
 *					that is being implemented.  For example, a HID
 *					class device needs to be able to respond to
 *					"GET REPORT" type of requests.  This
 *					is not a standard USB chapter 9 request, and 
 *					therefore not handled by usb_device.c.  Instead
 *					this request should be handled by class specific 
 *					firmware, such as that contained in usb_function_hid.c.
 *
 * Note:            None
 *******************************************************************/
void USBCBCheckOtherReq(void)
{
    USBCheckHIDRequest();
}//end


/*******************************************************************
 * Function:        void USBCBStdSetDscHandler(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        The USBCBStdSetDscHandler() callback function is
 *					called when a SETUP, bRequest: SET_DESCRIPTOR request
 *					arrives.  Typically SET_DESCRIPTOR requests are
 *					not used in most applications, and it is
 *					optional to support this type of request.
 *
 * Note:            None
 *******************************************************************/
void USBCBStdSetDscHandler(void)
{
    // Must claim session ownership if supporting this request
}//end


/*******************************************************************
 * Function:        void USBCBInitEP(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        This function is called when the device becomes
 *                  initialized, which occurs after the host sends a
 * 					SET_CONFIGURATION (wValue not = 0) request.  This 
 *					callback function should initialize the endpoints 
 *					for the device's usage according to the current 
 *					configuration.
 *
 * Note:            None
 *******************************************************************/
void USBCBInitEP(void)
{
    //enable the HID endpoint
    USBEnableEndpoint(HID_EP,USB_OUT_ENABLED|USB_IN_ENABLED|USB_HANDSHAKE_ENABLED|USB_DISALLOW_SETUP);
    USBEnableEndpoint(HID_EP2,USB_OUT_ENABLED|USB_IN_ENABLED|USB_HANDSHAKE_ENABLED|USB_DISALLOW_SETUP);
    USBEnableEndpoint(HID_EP3,USB_OUT_ENABLED|USB_IN_ENABLED|USB_HANDSHAKE_ENABLED|USB_DISALLOW_SETUP);
    USBEnableEndpoint(HID_EP4,USB_OUT_ENABLED|USB_IN_ENABLED|USB_HANDSHAKE_ENABLED|USB_DISALLOW_SETUP);
}

/********************************************************************
 * Function:        void USBCBSendResume(void)
 *
 * PreCondition:    None
 *
 * Input:           None
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        The USB specifications allow some types of USB
 * 					peripheral devices to wake up a host PC (such
 *					as if it is in a low power suspend to RAM state).
 *					This can be a very useful feature in some
 *					USB applications, such as an Infrared remote
 *					control	receiver.  If a user presses the "power"
 *					button on a remote control, it is nice that the
 *					IR receiver can detect this signalling, and then
 *					send a USB "command" to the PC to wake up.
 *					
 *					The USBCBSendResume() "callback" function is used
 *					to send this special USB signalling which wakes 
 *					up the PC.  This function may be called by
 *					application firmware to wake up the PC.  This
 *					function should only be called when:
 *					
 *					1.  The USB driver used on the host PC supports
 *						the remote wakeup capability.
 *					2.  The USB configuration descriptor indicates
 *						the device is remote wakeup capable in the
 *						bmAttributes field.
 *					3.  The USB host PC is currently sleeping,
 *						and has previously sent your device a SET 
 *						FEATURE setup packet which "armed" the
 *						remote wakeup capability.   
 *
 *					This callback should send a RESUME signal that
 *                  has the period of 1-15ms.
 *
 * Note:            Interrupt vs. Polling
 *                  -Primary clock
 *                  -Secondary clock ***** MAKE NOTES ABOUT THIS *******
 *                   > Can switch to primary first by calling USBCBWakeFromSuspend()
 
 *                  The modifiable section in this routine should be changed
 *                  to meet the application needs. Current implementation
 *                  temporary blocks other functions from executing for a
 *                  period of 1-13 ms depending on the core frequency.
 *
 *                  According to USB 2.0 specification section 7.1.7.7,
 *                  "The remote wakeup device must hold the resume signaling
 *                  for at lest 1 ms but for no more than 15 ms."
 *                  The idea here is to use a delay counter loop, using a
 *                  common value that would work over a wide range of core
 *                  frequencies.
 *                  That value selected is 1800. See table below:
 *                  ==========================================================
 *                  Core Freq(MHz)      MIP         RESUME Signal Period (ms)
 *                  ==========================================================
 *                      48              12          1.05
 *                       4              1           12.6
 *                  ==========================================================
 *                  * These timing could be incorrect when using code
 *                    optimization or extended instruction mode,
 *                    or when having other interrupts enabled.
 *                    Make sure to verify using the MPLAB SIM's Stopwatch
 *                    and verify the actual signal on an oscilloscope.
 *******************************************************************/
void USBCBSendResume(void)
{
    static WORD delay_count;
    
    USBResumeControl = 1;                // Start RESUME signaling
    
    delay_count = 1800U;                // Set RESUME line for 1-13 ms
    do
    {
        delay_count--;
    }while(delay_count);
    USBResumeControl = 0;
}


/*******************************************************************
 * Function:        BOOL USER_USB_CALLBACK_EVENT_HANDLER(
 *                        USB_EVENT event, void *pdata, WORD size)
 *
 * PreCondition:    None
 *
 * Input:           USB_EVENT event - the type of event
 *                  void *pdata - pointer to the event data
 *                  WORD size - size of the event data
 *
 * Output:          None
 *
 * Side Effects:    None
 *
 * Overview:        This function is called from the USB stack to
 *                  notify a user application that a USB event
 *                  occured.  This callback is in interrupt context
 *                  when the USB_INTERRUPT option is selected.
 *
 * Note:            None
 *******************************************************************/
BOOL USER_USB_CALLBACK_EVENT_HANDLER(USB_EVENT event, void *pdata, WORD size)
{
    switch(event)
    {
        case EVENT_CONFIGURED: 
            USBCBInitEP();
            break;
        case EVENT_SET_DESCRIPTOR:
            USBCBStdSetDscHandler();
            break;
        case EVENT_EP0_REQUEST:
            USBCBCheckOtherReq();
            break;
        case EVENT_SOF:
            USBCB_SOF_Handler();
            break;
        case EVENT_SUSPEND:
            USBCBSuspend();
            break;
        case EVENT_RESUME:
            USBCBWakeFromSuspend();
            break;
        case EVENT_BUS_ERROR:
            USBCBErrorHandler();
            break;
        case EVENT_TRANSFER:
            Nop();
            break;
        default:
            break;
    }      
    return TRUE; 
}

/** EOF main.c *************************************************/
#endif
