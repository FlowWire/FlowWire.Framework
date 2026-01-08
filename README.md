# **FlowWire**
**High-Voltage Distributed Workflow Engine for .NET**
FlowWire is a source-generated, "near" zero-allocation workflow engine that reimagines distributed orchestration as Electronic Circuits. It allows you to build resilient, long-running processes using a high-performance DSL that feels like designing hardware, but runs as pure software.

### The Circuit Metaphor
FlowWire moves away from standard "workflow" terminology. Instead, it treats your business logic as a Circuit Board (`[Flow]`) that connects Components (`[Driver]`) via Traces (`[Wire]`).

| Concept | FlowWire Term | Description
| -------- | -------- | -------- |
| The Board | `[Flow]` | The "workflow" container for your logic and state. |
| The Logic | `[Wire]` | The main execution loop. Pure C# logic. |
| The Connector | `[Link]` | Dependency Injection for State and Drivers. |
| The Peripheral | `[Driver]` | External service interface (API, DB, Email). |
| The Input | `[Impulse]` | Active signal surge that triggers a state change. |
| The Sensor | `[Probe]` | Passive status check (read-only). |

### Quick Start
Define your hardware drivers and wire your circuit.

```csharp
[Flow]
public partial class OrderCircuit : IFlow
{
    // [Link]: Solder your components to the board
    [Link] private OrderState _state;
    [Link] private IPaymentDriver _payment;
    [Link] private IInventoryDriver _inventory;

    // [Impulse]: External trigger (e.g. User clicks "Cancel")
    [Impulse]
    public void Cancel() => _state.IsCancelled = true;

    // [Wire]: The main circuit path
    [Wire]
    public FlowCommand Execute()
    {
        if (_state.IsCancelled) 
            return Command.Finish();

        // Driving the hardware (Zero-Allocation Proxy)
        if (!_state.StockReserved)
        {
            return Command.Drive(_inventory.Reserve("SKU-123"));
        }

        return Command.Drive(_payment.Charge(99.95m));
    }
}
```

### Key Features

- **Near Zero-Allocation:** Uses advanced Source Generators to create [Driver] proxies. No Expression Trees. No Reflection at runtime.
- **Configurable Hybrid Storage:** Define your own "Storage Ladder" to automatically tier data based on size. The engine routes data to the optimal backend without code changes.
  - Example Configuration:
    - \< 4KB: Kept Inline (Hot RAM/Redis).
    - 4KB - 1MB: Moved to Fast Storage (NVMe/SQL).
    - \> 1MB: Offloaded to Cold Storage (Blob).
- **Decoupled Architecture:** Scale your Brain (Orchestrator) separately from your Muscle (Driver Workers).
- **Type-Safe Options:** Strongly typed configuration prevents runtime errors and enables robust validation.

### Configuration
```csharp
builder.Services.AddFlowWire(options =>
{
    // Configure the "Storage Ladder"
    options.Storage.ConfigureAutoStrategy(rules => rules
        .UseInlineBelow(4096)                 // Keep tiny data in Redis
        .UseBelow<FastNvmeStorage>(1_000_000) // 1MB to fast disk
        .DefaultTo<S3Storage>()               // Dump everything else to S3
    );
})
.AddClient() 
.AddWorker() 
.AddOrchestrator(); 
```
