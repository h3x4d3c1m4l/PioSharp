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
                PioInstructionTypes.WAIT => $"wait {instruction.WaitPolarity.ToPioasmString()} {instruction.WaitSource.ToPioasmString()}",//TODO: IRQ no rel
                PioInstructionTypes.IN => $"in {instruction.InSource.ToPioasmString()}, {instruction.BitCount}",
                PioInstructionTypes.OUT => $"out {instruction.OutDestination.ToPioasmString()}, {instruction.BitCount}",
                PioInstructionTypes.PUSH => $"push {(instruction.PushIfFull ? "iffull " : "")}{(instruction.Block ? "block" : "noblock")}",
                PioInstructionTypes.PULL => $"pull {(instruction.PullIfEmpty ? "ifempty " : "")}{(instruction.Block ? "block" : "noblock")}",
                PioInstructionTypes.MOV => $"mov {instruction.MovDestination.ToPioasmString()}, {(instruction.MovOperation != PioMovOperations.None ? $"{instruction.MovOperation.ToPioasmString()} " : "")}{instruction.MovSource.ToPioasmString()}",
                PioInstructionTypes.IRQ => throw new Exception("IRQ instruction not supported yet"),// TODO
                PioInstructionTypes.SET => $"set {instruction.SetDestination}, {instruction.SetData}",
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
                PioWaitSources.Irq => "PIN",
                PioWaitSources.Pin => "IRQ",
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
