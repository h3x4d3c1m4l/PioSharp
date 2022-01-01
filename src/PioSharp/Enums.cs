namespace PioSharp
{
    public enum PioInstructionTypes
    {
        JMP = 0,
        WAIT = 1 << 13,
        IN = 1 << 14,
        OUT = 1 << 13 | 1 << 14,
        PUSH = 1 << 15,
        PULL = 1 << 7 | 1 << 15,
        MOV = 1 << 13 | 1 << 15,
        IRQ = 1 << 14 | 1 << 15,
        SET = 1 << 13 | 1 << 14 | 1 << 15
    }

    public enum PioJumpConditions
    {
        Always = 0,
        ScratchXZero = 1 << 5,
        ScratchXNonZeroAndPostDecrement = 1 << 6,
        ScratchYZero = 1 << 5 | 1 << 6,
        ScratchYNonZeroAndPostDecrement = 1 << 7,
        ScratchXNotEqScratchY = 1 << 5 | 1 << 7,
        BranchOnInputPin = 1 << 6 | 1 << 7,
        OutShiftRegNotEmpty = 1 << 5 | 1 << 6 | 1 << 7
    }

    public enum PioWaitPolarities
    {
        WaitForZero = 0,
        WaitForOne = 1
    }

    public enum PioWaitSources
    {
        Gpio = 0,
        Pin = 1 << 5,
        Irq = 1 << 6
    }

    public enum PioInSources
    {
        Pins = 0,
        X = 1 << 5,
        Y = 1 << 6,
        Null = 1 << 5 | 1 << 6,
        Isr = 1 << 6 | 1 << 7,
        Osr = 1 << 5 | 1 << 6 | 1 << 7
    }

    public enum PioOutDestinations
    {
        Pins = 0,
        X = 1 << 5,
        Y = 1 << 6,
        Null = 1 << 5 | 1 << 6,
        PinDirs = 1 << 7,
        PC = 1 << 5 | 1 << 7,
        ISR = 1 << 6 | 1 << 7,
        Exec = 1 << 5 | 1 << 6 | 1 << 7
    }

    public enum PioMovDestinations
    {
        Pins = 0,
        X = 1 << 5,
        Y = 1 << 6,
        Exec = 1 << 7,
        PC = 1 << 5 | 1 << 7,
        Isr = 1 << 6 | 1 << 7,
        Osr = 1 << 5 | 1 << 6 | 1 << 7
    }

    public enum PioMovOperations
    {
        None = 0,
        Invert = 1 << 3,
        BitReverse = 1 << 4
    }

    public enum PioMovSources
    {
        Pins = 0,
        X = 1 << 0,
        Y = 1 << 1,
        Null = 1 << 0 | 1 << 1,
        Status = 1 << 0 | 1 << 2,
        Isr = 1 << 1 | 1 << 2,
        Osr = 1 << 0 | 1 << 1 | 1 << 2
    }

    public enum PioSetDestinations
    {
        Pins = 0,
        X = 1 << 5,
        Y = 1 << 6,
        PinDirs = 1 << 7
    }
}
