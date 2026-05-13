using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using MechanicShop.Domain.Common.Results.Abstractions;
namespace MechanicShop.Domain.Common.Results;

public static class Result
{
  public static Success Success => default;
  public static Created Created => default;
  public static Deleted Deleted => default;
  public static Updated Updated => default;
}

public sealed class Result<TValue> : IResult<TValue>
{
    private readonly TValue? _value = default;
    private readonly List<Error>? _errors = null;
    public bool IsSuccess {get;}
    public bool IsError => !IsSuccess;
    public List<Error> Errors => IsError ? _errors! : [];
    public TValue Value => IsSuccess ? _value! : default!;
    public Error TopError => (_errors?.Count > 0) ? _errors[0] : default;

    private Result(Error error)
    {
        _errors = [error];
    }

    private Result(List<Error> errors)
    {
        if(errors is null || errors.Count == 0)
        {
            throw new ArgumentException("Cannot Create An Error <TValue> From An Empty Collection Of Errors",nameof(errors));
        }
        _errors = errors;
        IsSuccess = false;
    }

    private Result(TValue value)
    {
        if(value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        _value = value;
        IsSuccess = true;
    }
    [JsonConstructor]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("For Serialization Only", true)]
    public Result(TValue? value, List<Error>? errors, bool isSuccess)
    {
        if (isSuccess)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
            _errors = [];
            IsSuccess = true;
        }
        else
        {
            if(errors is null || errors.Count == 0)
            {
                throw new ArgumentException("Provide At least One Error.", nameof(errors));
            }
            _errors = errors;
            _value = default!;
            IsSuccess = false;  
        }

    }
    
    public TNextValue Match<TNextValue>(Func<TValue,TNextValue> onValue, Func<List<Error>, TNextValue> onError) 
        => IsSuccess ? onValue(Value!) : onError(Errors);

    public static implicit operator Result<TValue>(TValue value) 
    => new(value);

    public static implicit operator Result<TValue>(Error error) 
    => new(error);
    
    public static implicit operator Result<TValue>(List<Error> errors)
     => new(errors);
}

public readonly record struct Success;
public readonly record struct Created;
public readonly record struct Deleted;
public readonly record struct Updated;