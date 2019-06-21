//	EEPROM“Ç‚İ‘‚«ƒ‰ƒCƒuƒ‰ƒŠ
//		2010/08/11	‰‰ñ

unsigned char ReadEEPROM(int adr);
unsigned char WriteEEPROM(int adr,unsigned char data);
unsigned char ReadEEPROM_Agree(int adr, unsigned char read_count, unsigned char agree_num, unsigned char *val);
unsigned char WriteEEPROM_Agree(int adr, unsigned char data, unsigned char write_count);

