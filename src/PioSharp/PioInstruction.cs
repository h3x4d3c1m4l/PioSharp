namespace PioSharp
{
    // TODO: Side Set
    public readonly record struct PioInstruction
    {
        public PioInstructionTypes Type { get; init; }

        public byte BitCount { get; init; }

        public byte Index { get; init; }

        #region JMP

        public static PioInstruction CreateJmp(PioJumpConditions condition, byte address)
        {
            return new PioInstruction
            {
                Type = PioInstructionTypes.JMP,
                JumpConditions = condition,
                JumpAddress = address
            };
        }

        public PioJumpConditions JumpConditions { get; init; }

        public byte JumpAddress { get; init; }

        #endregion

        #region WAIT

        public static PioInstruction CreateWait(PioWaitPolarities polarity, PioWaitSources source, byte index)
        {
            return new PioInstruction
            {
                Type = PioInstructionTypes.WAIT,
                WaitPolarity = polarity,
                WaitSource = source,
                Index = index
            };
        }

        public PioWaitPolarities WaitPolarity { get; init; }

        public PioWaitSources WaitSource { get; init; }

        #endregion

        #region IN

        public static PioInstruction CreateIn(PioInSources source, byte bitCount)
        {
            if (bitCount > 32)
            {
                throw new ArgumentException("Bit count > 32 unsupported by PIO", nameof(bitCount));
            }

            return new PioInstruction
            {
                Type = PioInstructionTypes.IN,
                InSource = source,
                BitCount = bitCount == 0 ? (byte)32 : bitCount
            };
        }

        public PioInSources InSource { get; init; }

        #endregion

        #region OUT

        public static PioInstruction CreateOut(PioOutDestinations destination, byte bitCount)
        {
            if (bitCount > 32)
            {
                throw new ArgumentException("Bit count > 32 unsupported by PIO", nameof(bitCount));
            }

            return new PioInstruction
            {
                Type = PioInstructionTypes.OUT,
                OutDestination = destination,
                BitCount = bitCount == 0 ? (byte)32 : bitCount
            };
        }

        public PioOutDestinations OutDestination { get; init; }

        #endregion

        #region PUSH

        public static PioInstruction CreatePush(PioPushOperations operations)
        {
            return new PioInstruction
            {
                Type = PioInstructionTypes.PUSH,
                PushOperations = operations
            };
        }

        public PioPushOperations PushOperations { get; init; }

        #endregion

        #region PULL

        public static PioInstruction CreatePull(PioPullOperations operations)
        {
            return new PioInstruction
            {
                Type = PioInstructionTypes.PULL,
                PullOperations = operations
            };
        }

        public PioPullOperations PullOperations { get; init; }

        #endregion

        #region MOV

        public static PioInstruction CreateMov(PioMovDestinations destination, PioMovOperations operation, PioMovSources source)
        {
            return new PioInstruction
            {
                Type = PioInstructionTypes.MOV,
                MovDestination = destination,
                MovOperation = operation,
                MovSource = source
            };
        }

        public PioMovDestinations MovDestination { get; init; }

        public PioMovOperations MovOperation { get; init; }

        public PioMovSources MovSource { get; init; }

        #endregion

        #region IRQ

        public static PioInstruction CreateIrq(PioIrqOperations operation, byte index)
        {
            return new PioInstruction
            {
                Type = PioInstructionTypes.IRQ,
                IrqOperation = operation,
                Index = index
            };
        }

        public PioIrqOperations IrqOperation { get; init; }

        #endregion

        #region SET

        public static PioInstruction CreateSet(PioSetDestinations destination, byte data)
        {
            return new PioInstruction
            {
                Type = PioInstructionTypes.SET,
                SetDestination = destination,
                SetData = data
            };
        }

        public PioSetDestinations SetDestination { get; init; }

        public byte SetData { get; init; }

        #endregion

        #region Encoding

        public ushort EncodeToUShort()
        {
            return Type switch
            {
                PioInstructionTypes.JMP => (ushort)((ushort)Type | (ushort)JumpConditions | JumpAddress),
                PioInstructionTypes.WAIT => (ushort)((ushort)Type | (ushort)WaitPolarity | (ushort)WaitSource | Index),
                PioInstructionTypes.IN => (ushort)((ushort)Type | (ushort)InSource | (BitCount == 32 ? 0 : BitCount)),
                PioInstructionTypes.OUT => (ushort)((ushort)Type | (ushort)OutDestination | (BitCount == 32 ? 0 : BitCount)),
                PioInstructionTypes.PUSH => (ushort)((ushort)Type | (ushort)((PushOperations & PioPushOperations.IfFull) != 0 ? 1 << 6 : 0) | (ushort)((PushOperations & PioPushOperations.Block) != 0 ? 1 << 5 : 0)),
                PioInstructionTypes.PULL => (ushort)((ushort)Type | (ushort)((PullOperations & PioPullOperations.IfEmpty) != 0 ? 1 << 6 : 0) | (ushort)((PullOperations & PioPullOperations.Block) != 0 ? 1 << 5 : 0)),
                PioInstructionTypes.MOV => (ushort)((ushort)Type | (ushort)MovDestination | (ushort)MovOperation | (ushort)MovSource),
                PioInstructionTypes.IRQ => (ushort)((ushort)Type | (ushort)((IrqOperation & PioIrqOperations.Clear) != 0 ? 1 << 6 : 0) | (ushort)((IrqOperation & PioIrqOperations.Wait) != 0 ? 1 << 5 : 0) | Index),
                PioInstructionTypes.SET => (ushort)((ushort)Type | (ushort)SetDestination | SetData),
                _ => throw new Exception($"Instruction type {Type} not supported"),
            };
        }

        public byte[] Encode()
        {
            var data = new byte[2];
            Encode(data);
            return data;
        }

        public void Encode(Span<byte> byteSpan)
        {
            var ins = EncodeToUShort();
            if (!BitConverter.TryWriteBytes(byteSpan, ins))
            {
                throw new Exception("Conversion of ushort to bytes failed");
            }
        }

        #endregion

        #region Decoding

        public static PioInstruction DecodeFromUShort(ushort data)
        {
            // instruction type
            var insType = (PioInstructionTypes)(data & (1 << 13 | 1 << 14 | 1 << 15));
            if (insType == PioInstructionTypes.PUSH && (data & (1 << 7)) == (1 << 7))
            {
                insType = PioInstructionTypes.PULL;
            }

            return insType switch
            {
                PioInstructionTypes.JMP => CreateJmp(
                    condition: (PioJumpConditions)(data & (1 << 5 | 1 << 6 | 1 << 7)),
                    address: (byte)(data & (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4))
                ),
                PioInstructionTypes.WAIT => CreateWait(
                    polarity: (data & (1 << 7)) == (1 << 7) ? PioWaitPolarities.WaitForOne : PioWaitPolarities.WaitForZero,
                    source: (PioWaitSources)(data & (1 << 5 | 1 << 6)),
                    index: (byte)(data & (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4))
                ),
                PioInstructionTypes.IN => CreateIn(
                    source: (PioInSources)(data & (1 << 5 | 1 << 6 | 1 << 7)),
                    bitCount: (byte)(data & (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4))
                ),
                PioInstructionTypes.OUT => CreateOut(
                    destination: (PioOutDestinations)(data & (1 << 5 | 1 << 6 | 1 << 7)),
                    bitCount: (byte)(data & (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4))
                ),
                PioInstructionTypes.PUSH => CreatePush(
                    operations: (PioPushOperations)(data & (1 << 5 | 1 << 6))
                ),
                PioInstructionTypes.PULL => CreatePull(
                    operations: (PioPullOperations)(data & (1 << 5 | 1 << 6))
                ),
                PioInstructionTypes.MOV => CreateMov(
                    destination: (PioMovDestinations)(data & (1 << 5 | 1 << 6 | 1 << 7)),
                    operation: (PioMovOperations)(data & (1 << 3 | 1 << 4)),
                    source: (PioMovSources)(data & (1 << 0 | 1 << 1 | 1 << 2))
                ),
                PioInstructionTypes.IRQ => CreateIrq(
                    operation: (PioIrqOperations)(data & (1 << 5 | 1 << 6)),
                    index: (byte)(data & (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4))
                ),
                PioInstructionTypes.SET => CreateSet(
                    destination: (PioSetDestinations)(data & (1 << 5 | 1 << 6 | 1 << 7)),
                    data: (byte)(data & (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4))
                ),
                _ => throw new Exception($"Instruction {insType} not supported"),
            };
        }

        public static PioInstruction Decode(ReadOnlySpan<byte> instructionData)
        {
            return DecodeFromUShort(BitConverter.ToUInt16(instructionData));
        }

        #endregion
    }
}