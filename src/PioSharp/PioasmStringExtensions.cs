using OneOf;

namespace PioSharp
{
    public static class PioasmStringExtensions
    {
        public static string ToPioasmString(this PioInstruction instruction, OneOf<string, Func<int, string>>? destinationResolver = null)
        {
            return instruction.Type switch
            {
                PioInstructionTypes.JMP => $"jmp {(instruction.JumpConditions != PioJumpConditions.Always ? $"{instruction.JumpConditions.ToPioasmString()} " : "")}{ResolveJumpDestination(instruction.JumpAddress, destinationResolver)}",
                PioInstructionTypes.WAIT => $"wait {instruction.WaitPolarity.ToPioasmString()} {instruction.WaitSource.ToPioasmString()}{(instruction.WaitSource == PioWaitSources.Irq ? $" {ResolveIrqNumberAndRel(instruction.Index)}" : "")}", // TODO: test properly
                PioInstructionTypes.IN => $"in {instruction.InSource.ToPioasmString()}, {(instruction.BitCount == 0 ? "32" : instruction.BitCount.ToString())}",
                PioInstructionTypes.OUT => $"out {instruction.OutDestination.ToPioasmString()}, {(instruction.BitCount == 0 ? "32" : instruction.BitCount.ToString())}",
                PioInstructionTypes.PUSH => $"push {(instruction.PushIfFull ? "iffull " : "")}{(instruction.Block ? "block" : "noblock")}",
                PioInstructionTypes.PULL => $"pull {(instruction.PullIfEmpty ? "ifempty " : "")}{(instruction.Block ? "block" : "noblock")}",
                PioInstructionTypes.MOV => $"mov {instruction.MovDestination.ToPioasmString()}, {(instruction.MovOperation != PioMovOperations.None ? $"{instruction.MovOperation.ToPioasmString()} " : "")}{instruction.MovSource.ToPioasmString()}",
                PioInstructionTypes.IRQ => $"irq {(instruction.IrqClear ? "clear" : instruction.IrqWait ? "wait" : "set")} {ResolveIrqNumberAndRel(instruction.Index)}", // TODO: test property
                PioInstructionTypes.SET => $"set {instruction.SetDestination.ToPioasmString()}, {instruction.SetData}",
                _ => throw new Exception($"Instruction {instruction.Type} not supported yet"),
            };
        }

        private static string ResolveJumpDestination(byte destination, OneOf<string, Func<int, string>>? destinationResolver)
        {
            if (destinationResolver == null)
            {
                return destination.ToString();
            }
            else if (destinationResolver.Value.IsT0)
            {
                return destinationResolver.Value.AsT0;
            }
            else
            {
                return destinationResolver.Value.AsT1(destination);
            }
        }

        private static string ResolveIrqNumberAndRel(byte index)
        {
            if ((index >> 4) == 1)
            {
                return $"{index & (1 << 0 | 1 << 1 | 1 << 2)} rel";
            }
            else
            {
                return (index & (1 << 0 | 1 << 1 | 1 << 2)).ToString();
            }
        }

        public static string ToPioasmString(this PioJumpConditions x)
        {
            return x switch
            {
                PioJumpConditions.Always => "",
                PioJumpConditions.ScratchXZero => "!X",
                PioJumpConditions.ScratchXNonZeroAndPostDecrement => "X--",
                PioJumpConditions.ScratchYZero => "!Y",
                PioJumpConditions.ScratchYNonZeroAndPostDecrement => "Y--",
                PioJumpConditions.ScratchXNotEqScratchY => "X!=Y",
                PioJumpConditions.BranchOnInputPin => "PIN",
                PioJumpConditions.OutShiftRegNotEmpty => "!OSRE",
                _ => throw new Exception($"Value {x} not supported"),
            };
        }

        public static string ToPioasmString(this PioWaitSources x)
        {
            return x switch
            {
                PioWaitSources.Gpio => "GPIO",
                PioWaitSources.Pin => "PIN",
                PioWaitSources.Irq => "IRQ",
                _ => throw new Exception($"Value {x} not supported"),
            };
        }

        public static string ToPioasmString(this PioWaitPolarities x)
        {
            return x switch
            {
                PioWaitPolarities.WaitForZero => "0",
                PioWaitPolarities.WaitForOne => "1",
                _ => throw new Exception($"Value {x} not supported"),
            };
        }

        public static string ToPioasmString(this PioInSources x)
        {
            return x switch
            {
                PioInSources.Pins => "PINS",
                PioInSources.X => "X",
                PioInSources.Y => "Y",
                PioInSources.Null => "NULL",
                PioInSources.Isr => "ISR",
                PioInSources.Osr => "OSR",
                _ => throw new Exception($"Value {x} not supported"),
            };
        }

        public static string ToPioasmString(this PioOutDestinations x)
        {
            return x switch
            {
                PioOutDestinations.Pins => "PINS",
                PioOutDestinations.X => "X",
                PioOutDestinations.Y => "Y",
                PioOutDestinations.Null => "NULL",
                PioOutDestinations.PinDirs => "PINDIRS",
                PioOutDestinations.PC => "PC",
                PioOutDestinations.ISR => "ISR",
                PioOutDestinations.Exec => "EXEC",
                _ => throw new Exception($"Value {x} not supported"),
            };
        }

        public static string ToPioasmString(this PioMovDestinations x)
        {
            return x switch
            {
                PioMovDestinations.Pins => "PINS",
                PioMovDestinations.X => "X",
                PioMovDestinations.Y => "Y",
                PioMovDestinations.Exec => "EXEC",
                PioMovDestinations.PC => "PC",
                PioMovDestinations.Isr => "ISR",
                PioMovDestinations.Osr => "OSR",
                _ => throw new Exception($"Value {x} not supported"),
            };
        }

        public static string ToPioasmString(this PioMovOperations x)
        {
            return x switch
            {
                PioMovOperations.BitReverse => "::",
                PioMovOperations.Invert => "!",// ~ is also allowed
                PioMovOperations.None => "",
                _ => throw new Exception($"Value {x} not supported"),
            };
        }

        public static string ToPioasmString(this PioMovSources x)
        {
            return x switch
            {
                PioMovSources.Pins => "PINS",
                PioMovSources.X => "X",
                PioMovSources.Y => "Y",
                PioMovSources.Null => "NULL",
                PioMovSources.Status => "STATUS",
                PioMovSources.Isr => "ISR",
                PioMovSources.Osr => "OSR",
                _ => throw new Exception($"Value {x} not supported"),
            };
        }

        public static string ToPioasmString(this PioSetDestinations x)
        {
            return x switch
            {
                PioSetDestinations.Pins => "PINS",
                PioSetDestinations.X => "X",
                PioSetDestinations.Y => "Y",
                PioSetDestinations.PinDirs => "PINDIRS",
                _ => throw new Exception($"Value {x} not supported"),
            };
        }
    }
}
