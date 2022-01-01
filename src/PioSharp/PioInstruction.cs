namespace PioSharp
{
    // TODO IRQ and SideSet
    public readonly record struct PioInstruction
    {
        public PioInstructionTypes Type { get; init; }

        public byte BitCount { get; init; }

        public bool Block { get; init; }

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
                WaitIndex = index
            };
        }

        public PioWaitPolarities WaitPolarity { get; init; }

        public PioWaitSources WaitSource { get; init; }

        public byte WaitIndex { get; init; }

        #endregion

        #region IN

        public static PioInstruction CreateIn(PioInSources source, byte bitCount)
        {
            if (bitCount > 32)
            {
                throw new ArgumentException("Bit count > 32 unsupported by PIO", nameof(bitCount));
            }
            else if (bitCount == 32)
            {
                // this is just encoding stuff, see manual
                bitCount = 0;
            }

            return new PioInstruction
            {
                Type = PioInstructionTypes.IN,
                InSource = source,
                BitCount = bitCount
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
            else if (bitCount == 32)
            {
                // this is just encoding stuff, see manual
                bitCount = 0;
            }

            return new PioInstruction
            {
                Type = PioInstructionTypes.OUT,
                OutDestination = destination,
                BitCount = bitCount
            };
        }

        public PioOutDestinations OutDestination { get; init; }

        #endregion

        #region PUSH

        public static PioInstruction CreatePush(bool ifFull, bool block)
        {
            return new PioInstruction
            {
                Type = PioInstructionTypes.PUSH,
                PushIfFull = ifFull,
                Block = block
            };
        }

        public bool PushIfFull { get; init; }

        #endregion

        #region PULL

        public static PioInstruction CreatePull(bool ifEmpty, bool block)
        {
            return new PioInstruction
            {
                Type = PioInstructionTypes.PULL,
                PullIfEmpty = ifEmpty,
                Block = block
            };
        }

        public bool PullIfEmpty { get; init; }

        #endregion

        #region MOV

        public static PioInstruction CreateMov(PioMovDestinations destination, PioMovOperations operation, PioMovSources sources)
        {
            return new PioInstruction
            {
                Type = PioInstructionTypes.MOV,
                MovDestination = destination,
                MovOperation = operation,
                MovSource = sources
            };
        }

        public PioMovDestinations MovDestination { get; init; }

        public PioMovOperations MovOperation { get; init; }

        public PioMovSources MovSource { get; init; }

        #endregion

        #region IRQ

        // TODO

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
                PioInstructionTypes.WAIT => (ushort)((ushort)Type | (ushort)WaitPolarity | (ushort)WaitSource | WaitIndex),
                PioInstructionTypes.IN => (ushort)((ushort)Type | (ushort)InSource | BitCount),
                PioInstructionTypes.OUT => (ushort)((ushort)Type | (ushort)OutDestination | BitCount),
                PioInstructionTypes.PUSH => (ushort)((ushort)Type | (ushort)(PushIfFull ? 1 << 6 : 0) | (ushort)(Block ? 1 << 5 : 0)),
                PioInstructionTypes.PULL => (ushort)((ushort)Type | (ushort)(PullIfEmpty ? 1 << 6 : 0) | (ushort)(Block ? 1 << 5 : 0)),
                PioInstructionTypes.MOV => (ushort)((ushort)Type | (ushort)MovDestination | (ushort)MovOperation | (ushort)MovSource),
                PioInstructionTypes.IRQ => throw new Exception("IRQ instruction not supported yet"),// TODO
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
                PioInstructionTypes.JMP => new PioInstruction
                {
                    Type = PioInstructionTypes.JMP,
                    JumpConditions = (PioJumpConditions)(data & (1 << 5 | 1 << 6 | 1 << 7)),
                    JumpAddress = (byte)(data & (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4))
                },
                PioInstructionTypes.WAIT => new PioInstruction
                {
                    Type = PioInstructionTypes.WAIT,
                    WaitPolarity = (data & (1 << 7)) == (1 << 7) ? PioWaitPolarities.WaitForOne : PioWaitPolarities.WaitForZero,
                    WaitSource = (PioWaitSources)(data & (1 << 5 | 1 << 6)),
                    WaitIndex = (byte)(data & (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4))
                },
                PioInstructionTypes.IN => new PioInstruction
                {
                    Type = PioInstructionTypes.IN,
                    InSource = (PioInSources)(data & (1 << 5 | 1 << 6 | 1 << 7)),
                    BitCount = (byte)(data & (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4))
                },
                PioInstructionTypes.OUT => new PioInstruction
                {
                    Type = PioInstructionTypes.OUT,
                    OutDestination = (PioOutDestinations)(data & (1 << 5 | 1 << 6 | 1 << 7)),
                    BitCount = (byte)(data & (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4))
                },
                PioInstructionTypes.PUSH => new PioInstruction
                {
                    Type = PioInstructionTypes.PUSH,
                    PushIfFull = (data & (1 << 6)) == (1 << 6),
                    Block = (data & (1 << 5)) == (1 << 5),
                },
                PioInstructionTypes.PULL => new PioInstruction
                {
                    Type = PioInstructionTypes.PULL,
                    PullIfEmpty = (data & (1 << 6)) == (1 << 6),
                    Block = (data & (1 << 5)) == (1 << 5),
                },
                PioInstructionTypes.MOV => new PioInstruction
                {
                    Type = PioInstructionTypes.MOV,
                    MovDestination = (PioMovDestinations)(data & (1 << 5 | 1 << 6 | 1 << 7)),
                    MovOperation = (PioMovOperations)(data & (1 << 3 | 1 << 4)),
                    MovSource = (PioMovSources)(data & (1 << 0 | 1 << 1 | 1 << 2)),
                },
                PioInstructionTypes.IRQ => throw new Exception("IRQ instruction not supported yet"),// TODO
                PioInstructionTypes.SET => new PioInstruction
                {
                    Type = PioInstructionTypes.SET,
                    SetDestination = (PioSetDestinations)(data & (1 << 5 | 1 << 6 | 1 << 7)),
                    SetData = (byte)(data & (1 << 0 | 1 << 1 | 1 << 2 | 1 << 3 | 1 << 4))
                },
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