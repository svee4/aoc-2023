	.file	"test.c"
	.text
	.p2align 4
	.globl	method
	.type	method, @function
method:
.LFB1:
	.cfi_startproc
	endbr64
	movl	$1, %edx
	xorl	%eax, %eax
	movl	$60000000, %edi
	movabsq	$450000000000000, %rsi
	.p2align 4,,10
	.p2align 3
.L3:
	movq	%rdi, %rcx
	subq	%rdx, %rcx
	imulq	%rdx, %rcx
	cmpq	%rsi, %rcx
	setg	%cl
	addq	$1, %rdx
	movzbl	%cl, %ecx
	addq	%rcx, %rax
	cmpq	$60000001, %rdx
	jne	.L3
	ret
	.cfi_endproc
.LFE1:
	.size	method, .-method
	.section	.text.startup,"ax",@progbits
	.p2align 4
	.globl	main
	.type	main, @function
main:
.LFB0:
	.cfi_startproc
	endbr64
	xorl	%eax, %eax
	jmp	method
	.cfi_endproc
.LFE0:
	.size	main, .-main
	.ident	"GCC: (Ubuntu 11.4.0-1ubuntu1~22.04) 11.4.0"
	.section	.note.GNU-stack,"",@progbits
	.section	.note.gnu.property,"a"
	.align 8
	.long	1f - 0f
	.long	4f - 1f
	.long	5
0:
	.string	"GNU"
1:
	.align 8
	.long	0xc0000002
	.long	3f - 2f
2:
	.long	0x3
3:
	.align 8
4:
