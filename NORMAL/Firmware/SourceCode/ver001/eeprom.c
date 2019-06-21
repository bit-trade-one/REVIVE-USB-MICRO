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

unsigned char WriteEEPROM(int adr,unsigned char data)
{
	unsigned char uc_ret = 0;
	
	if(EECON1bits.WR == 0)
	{
		EEADR = adr;
		EEDATA = data;
		EECON1 = 0x04;
		EECON2 = 0x55;
		EECON2 = 0xAA;
		EECON1bits.WR = 1;
	}
	else
	{
		uc_ret = 1;
	}	
	while(EECON1bits.WR);
	return uc_ret;
}

unsigned char ReadEEPROM_Agree(int adr, unsigned char read_count, unsigned char agree_num, unsigned char *val)
{
	unsigned char uc_ret = 1;	// NG
	unsigned char count = 0;
	unsigned char fi, fj;
	unsigned char fi_val;
	*val = 0;
	
	for( fi = 0; fi < read_count; fi++ )
	{
		count = 0;
		fi_val = ReadEEPROM(adr + fi);
		for( fj = fi+1; fj < read_count; fj++)
		{
			if(fi_val == ReadEEPROM(adr + fj))
			{
				count++;
			}	
		}	
		if(count >= (agree_num - 1))
		{
			// OK
			*val = fi_val;
			uc_ret = 0;
			break;
		}
	}
	
	return uc_ret;
}	

unsigned char WriteEEPROM_Agree(int adr, unsigned char data, unsigned char write_count)
{
	unsigned char uc_ret = 0;
	unsigned char fi;
	for( fi = 0; fi < write_count; fi++ )
	{
		if(WriteEEPROM(adr + fi, data) != 0)
		{
			uc_ret = 1;
		}
	}
	return uc_ret;
}


