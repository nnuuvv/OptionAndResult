# nuv.Option

basic option type implementation

### Declaring results

```csharp

    //               Option.Some<T>(TValue)
    var someOption = Option.Some("some value")
        
    // or           Option<TValue>.Some(TValue)
    var someOpion = new Option<string>.Some("some value")
    
    // or implicitly
    Option<string> some = "";
        
        
    //               Option.None<TValue>()
    var noneOption = Option.None<string>()
        
    //               Option.None<TValue>()
    var noneOption = new Option<string>.None()
```

### Applying functions to values

```csharp
    someOption.Map(x => x.Length)
    // -> Some(10)
    
    someOption
    .Map(x => Some(x.Length)) // -> Some(Some(10))
    .Flatten() // -> Some(10)
    
    someOption.Then(x => Some(x.Length))
    // -> Some(10)
    
    noneOption.Map(x => x.Length)
    // -> None()
```

### Checking for Some / None

```csharp
    someOption.IsSome()
    // -> true
    
    someOption.IsNone()
    // -> false
    
    noneOption.IsSome()
    // -> false
    
    noneOption.IsNone()
    // -> true
```

### Getting values

```csharp
    someOption.Unwrap("whoops");
    // -> "some value"
    
    noneOption.Unwrap("whoops");
    // -> "whoops"
```
