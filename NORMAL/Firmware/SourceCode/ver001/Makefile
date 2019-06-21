# MPLAB IDE generated this makefile for use with GNU make.
# Project: Revive.mcp
# Date: Mon May 13 15:49:32 2019

AS = MPASMWIN.exe
CC = mcc18.exe
LD = mplink.exe
AR = mplib.exe
RM = rm

REVIVE_USB.cof : usb_descriptors.o main.o usb_device.o usb_function_hid.o eeprom.o
	$(LD) /p18F14K50 /l"C:\MCC18\lib" "rm18f14k50.lkr" "usb_descriptors.o" "main.o" "usb_device.o" "usb_function_hid.o" "eeprom.o" /u_CRUNTIME /z__MPLAB_BUILD=1 /m"Revive.map" /w /o"REVIVE_USB.cof"

usb_descriptors.o : usb_descriptors.c ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdio.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdlib.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/string.h Microchip/Include/USB/usb_common.h Microchip/Include/USB/usb_device.h Microchip/Include/USB/usb_hal.h usb_descriptors.c Microchip/Include/USB/usb.h Microchip/Include/GenericTypeDefs.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stddef.h Microchip/Include/Compiler.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/p18cxxx.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/p18f14k50.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdarg.h usb_config.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/limits.h Microchip/Include/USB/usb_ch9.h Microchip/Include/USB/usb_hal_pic18.h Microchip/Include/USB/usb_function_hid.h
	$(CC) -p=18F14K50 /i".\Microchip\Include" -I"C:\MCC18\h" "usb_descriptors.c" -fo="usb_descriptors.o"

main.o : main.c ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/adc.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/string.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdio.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdlib.h Microchip/Include/USB/usb_common.h Microchip/Include/USB/usb_device.h Microchip/Include/USB/usb_hal.h eeprom.h main.c ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/p18f14k50.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/pconfig.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/p18cxxx.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stddef.h Microchip/Include/USB/usb.h Microchip/Include/GenericTypeDefs.h Microchip/Include/Compiler.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdarg.h usb_config.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/limits.h Microchip/Include/USB/usb_ch9.h Microchip/Include/USB/usb_hal_pic18.h HardwareProfile.h HardwareProfile\ -\ Low\ Pin\ Count\ USB\ Development\ Kit.h Microchip/Include/USB/usb_function_hid.h
	$(CC) -p=18F14K50 /i".\Microchip\Include" -I"C:\MCC18\h" "main.c" -fo="main.o"

usb_device.o : Microchip/USB/usb_device.c ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdio.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdlib.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/string.h Microchip/Include/USB/usb_common.h Microchip/Include/USB/usb_device.h Microchip/Include/USB/usb_hal.h Microchip/USB/usb_device.c Microchip/Include/USB/usb.h Microchip/Include/GenericTypeDefs.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stddef.h Microchip/Include/Compiler.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/p18cxxx.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/p18f14k50.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdarg.h usb_config.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/limits.h Microchip/Include/USB/usb_ch9.h Microchip/Include/USB/usb_hal_pic18.h HardwareProfile.h HardwareProfile\ -\ Low\ Pin\ Count\ USB\ Development\ Kit.h Microchip/USB/usb_device_local.h
	$(CC) -p=18F14K50 /i".\Microchip\Include" -I"C:\MCC18\h" "Microchip\USB\usb_device.c" -fo="usb_device.o"

usb_function_hid.o : Microchip/USB/HID\ Device\ Driver/usb_function_hid.c ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdio.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdlib.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/string.h Microchip/Include/USB/usb_common.h Microchip/Include/USB/usb_device.h Microchip/Include/USB/usb_hal.h Microchip/USB/HID\ Device\ Driver/usb_function_hid.c Microchip/Include/GenericTypeDefs.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stddef.h Microchip/Include/Compiler.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/p18cxxx.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/p18f14k50.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/stdarg.h Microchip/Include/USB/usb.h usb_config.h ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/limits.h Microchip/Include/USB/usb_ch9.h Microchip/Include/USB/usb_hal_pic18.h Microchip/Include/USB/usb_function_hid.h
	$(CC) -p=18F14K50 /i".\Microchip\Include" -I"C:\MCC18\h" "Microchip\USB\HID Device Driver\usb_function_hid.c" -fo="usb_function_hid.o"

eeprom.o : eeprom.c eeprom.h eeprom.c ../../../../../../Program\ Files\ (x86)/Microchip/mplabc18/v3.47/h/p18f14k50.h
	$(CC) -p=18F14K50 /i".\Microchip\Include" -I"C:\MCC18\h" "eeprom.c" -fo="eeprom.o"

clean : 
	$(RM) "usb_descriptors.o" "main.o" "usb_device.o" "usb_function_hid.o" "eeprom.o" "REVIVE_USB.cof" "REVIVE_USB.hex" "REVIVE_USB.map"

