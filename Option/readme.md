# nuv.Option

basic option type implementation

### Declaring results

```csharp
    //               Some(TValue)
    var someOption = Some("some value")
        
    // or           Option<TValue>.Some(TValue)
    var someOpion = Option<string>.Some("some value")
        
        
    //               None<TValue>()
    var noneOption = None<string>()
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
