//	EEPROM“Ç‚İ‘‚«ƒ‰ƒCƒuƒ‰ƒŠ
//		2010/08/11	‰‰ñ
#include <p18f14k50.h>
#include "eeprom.h"

unsigned char ReadEEPROM(int adr)
{
	EEADR = adr;
	EECON1 = 0x00;
	EECON1bits.RD = 1;
	return(EEDATA);
}

void WriteEEPROM(int adr,unsigned char data)
{
	EEADR = adr;
	EEDATA = data;
	EECON1 = 0x04;
	EECON2 = 0x55;
	EECON2 = 0xAA;
	EECON1bits.WR = 1;
	while(EECON1bits.WR);
}

