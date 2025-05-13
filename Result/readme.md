# nuv.Result

basic Result type implementation

### Declaring results

```csharp
    // declare "Ok" variant explicitly: 
    //             Result<TValue, TError>.Ok(TValue)
    var okResult = new Result<string, int>.Ok("everything went ok");
    //             Result.Ok<TValue, TError>(TValue)
    var okResult = Result.Ok<string, int>("everything went ok");
    // or implicitly: 
    Result<string, int> implicitOkResult = "everything went ok";
    
    
    // declare "Error" variant explicitly
    //                Result<TValue, TError>.Error(TError)
    var errorResult = new Result<string, int>.Error(1);
    //                Result.Error<TValue, TError>(TError)
    var errorResult = Result.Error<string, int>(1);
    // or implicitly: 
    Result<string, int> implicitErrorResult = 1;
     
```

### Returning results

```csharp
public Result<string, int> SomeFallibleFunction() // success example
{
    // explicitly: 
    return new Result<string, int>.Ok("everything went ok");
    // or implicitly: 
    return "everything went ok";
}

public Result<string, int> SomeFallibleFunction() // fail example
{
    // explicitly: 
    return new Result<string, int>.Error(1);
    // or implicitly: 
    return 1;
}
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
