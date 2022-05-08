using OneOf;

namespace PioSharp.Pioasm
{
    public record PioasmLabel(string Name);
    public record PioasmJmp(PioJumpConditions Condition, OneOf<string, byte> Target);
    public record PioasmWait(PioWaitPolarities Polarity, PioWaitSources Source, byte Number, bool Rel);
    public record PioasmIn(PioInSources Source, byte BitCount);
    public record PioasmOut(PioOutDestinations Destination, byte BitCount);
    public record PioasmPush(PioPushOperations Operations);
    public record PioasmPull(PioPullOperations Operations);
    public record PioasmMov(PioMovDestinations Destination, PioMovOperations Operation, PioMovSources Source);
    public record PioasmIrq(PioIrqOperations Operation, byte Number, bool Rel);
    public record PioasmSet(PioSetDestinations Destination, byte Value);
}
