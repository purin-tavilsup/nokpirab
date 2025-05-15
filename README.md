# Nokpirab 🕊️

![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)
[![Build](https://github.com/purin-tavilsup/nokpirab/actions/workflows/ci.yml/badge.svg)](https://github.com/purin-tavilsup/nokpirab/actions/workflows/ci.yml)

**Nokpirab** is a lightweight and test-friendly mediator library designed to simplify command and query handling in .NET applications.  
Nokpirab helps you implement the **CQRS pattern** cleanly — with minimal ceremony and maximum flexibility.

> Nokpirab (นกพิราบ) means "pigeon" in Thai 🕊️ — a message carrier, like your reliable mediator.

## Register with DI

```
services.AddNokpirabFromAssembly(typeof(Startup).Assembly);
```
This registers `INokpirab` as transient and then scans handlers from an assembly and registers them as transient

## Example

### Define a Command and Handler

```
public record CreateOrderCommand(Guid OrderId) : ICommand;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    public Task HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Creating order: {command.OrderId}");
        return Task.CompletedTask;
    }
}
```

### Send a Command

```
await _nokpirab.SendAsync(new CreateOrderCommand(Guid.NewGuid()));
```

### Define a Query and Handler

```
public record GetOrderByIdQuery(Guid OrderId) : IQuery<string>;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, string>
{
    public Task<string> HandleAsync(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var order = $"Order ID: {query.OrderId}";
        return Task.FromResult(order);
    }
}
```

### Send a Query

```
var result = await _nokpirab.SendAsync(new GetOrderByIdQuery(Guid.NewGuid()));
```
