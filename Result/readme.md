# nuv.Result

basic Result type implementation

### Declaring results

```csharp
    //             Result<TValue, TError>.Ok(TValue)
    var okResult = Result<string, int>.Ok("everything went ok")
        
    //                Result<TValue, TError>.Error(TError)
    var errorResult = Result<string, int>.Error(1)
```

### Applying functions to values

```csharp
    okResult.Map(x => x.Length)
    // -> Ok(19)
    
    okResult
    .Map(x => Ok(x.Length)) // -> Ok(Ok(19))
    .Flatten() // -> Ok(19)
    
    okResult.Try(x => Ok(x.Length))
    // -> Ok(19)
    
    errorResult.Map(x => x.Length)
    // -> Error(1)
    
    okResult.MapError(x => x + 1)
    // -> Ok(19)
    
    errorResult.MapError(x => x + 1)
    // -> Error(2)
```

### Checking for Ok / Error

```csharp
    okResult.IsOk()
    // -> true
    
    okResult.IsError()
    // -> false
    
    errorResult.IsOk()
    // -> false
    
    errorResult.IsError()
    // -> true
```

### Getting error / ok values

```csharp
    errorResult.UnwrapError(0);
    // -> 1
    
    okResult.UnwrapError(0);
    // -> 0
    
    errorResult.Unwrap("whoops");
    // -> "whoops"
    
    okResult.Unwrap("whoops");
    // -> "everything went ok"
```
