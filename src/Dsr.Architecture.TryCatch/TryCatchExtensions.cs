using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Dsr.Architecture.Utilities.TryCatch;

/// <summary>
/// Extension methods for creating and chaining TryCatch and TryCatch<T> objects.
/// </summary>
public static class TryCatchExtensions
{
    /// <summary>
    /// Initializes a TryCatch object with the specified try task.
    /// </summary>
    /// <param name="obj">The object from which the method is called.</param>
    /// <param name="task">The task to be executed in the try block.</param>
    /// <returns>A TryCatch object with the specified try task.</returns>
    public static TryCatch Try(this object obj, Task task)
    {
        var tryCatch = new TryCatch
        {
            Try = task
        };
        return tryCatch;
    }

    /// <summary>
    /// Initializes a TryCatch<T> object with the specified try task.
    /// </summary>
    /// <typeparam name="T">The type of the return value of the try task.</typeparam>
    /// <param name="obj">The object from which the method is called.</param>
    /// <param name="task">The task to be executed in the try block.</param>
    /// <returns>A TryCatch<T> object with the specified try task.</returns>
    public static TryCatch<T> Try<T>(this object obj, Task<T?> task)
    {
        var tryCatch = new TryCatch<T>
        {
            Try = task
        };
        return tryCatch;
    }

    /// <summary>
    /// Initializes a TryCatch object with the specified try function.
    /// </summary>
    /// <param name="obj">The object from which the method is called.</param>
    /// <param name="func">The function to be executed in the try block.</param>
    /// <returns>A TryCatch object with the specified try task.</returns>
    public static TryCatch Try(this object obj, Action func)
    {
        var tryCatch = new TryCatch
        {
            Try = Task.Run(func)
        };
        return tryCatch;
    }

    /// <summary>
    /// Initializes a TryCatch<T> object with the specified try function.
    /// </summary>
    /// <typeparam name="T">The type of the return value of the try task.</typeparam>
    /// <param name="obj">The object from which the method is called.</param>
    /// <param name="func">The function to be executed in the try block.</param>
    /// <returns>A TryCatch<T> object with the specified try task.</returns>
    public static TryCatch<T> Try<T>(this object obj, Func<T?> func)
    {
        var tryCatch = new TryCatch<T>
        {
            Try = Task.Run(func)
        };
        return tryCatch;
    }

    /// <summary>
    /// Initializes a TryCatch<T> object with the specified try function.
    /// </summary>
    /// <typeparam name="T">The type of the return value of the try task.</typeparam>
    /// <param name="obj">The object from which the method is called.</param>
    /// <param name="func">The function to be executed in the try block.</param>
    /// <returns>A TryCatch<T> object with the specified try task.</returns>
    public static TryCatch<T> Try<T>(this object obj, Func<Task<T?>> func)
    {
        var tryCatch = new TryCatch<T>
        {
            Try = func()
        };
        return tryCatch;
    }

    /// <summary>
    /// Adds a catch task to the specified TryCatch object.
    /// </summary>
    /// <param name="obj">The TryCatch object to which the catch task will be added.</param>
    /// <param name="task">The task to be executed in the catch block.</param>
    /// <returns>The updated TryCatch object with the catch task added.</returns>
    public static TryCatch Catch(this TryCatch obj, Task task)
    {
        obj.Catch = task;
        return obj;
    }

    /// <summary>
    /// Adds a catch task to the specified TryCatch object.
    /// </summary>
    /// <param name="obj">The TryCatch object to which the catch task will be added.</param>
    /// <param name="func">The function to be executed in the catch block.</param>
    /// <returns>The updated TryCatch object with the catch task added.</returns>
    public static TryCatch Catch(this TryCatch obj, Func<Exception, Task> func)
    {
        obj.CatchFunc = func;
        return obj;
    }

    /// <summary>
    /// Adds a catch task to the specified TryCatch<T> object.
    /// </summary>
    /// <typeparam name="T">The type of the return value of the catch task.</typeparam>
    /// <param name="obj">The TryCatch<T> object to which the catch task will be added.</param>
    /// <param name="task">The task to be executed in the catch block.</param>
    /// <returns>The updated TryCatch<T> object with the catch task added.</returns>
    public static TryCatch<T> Catch<T>(this TryCatch<T> obj, Task<T?> task)
    {
        obj.Catch = task;
        return obj;
    }

    /// <summary>
    /// Adds a catch function to the specified TryCatch<T> object.
    /// </summary>
    /// <typeparam name="T">The type of the return value of the catch task.</typeparam>
    /// <param name="obj">The TryCatch<T> object to which the catch task will be added.</param>
    /// <param name="func">The function to be executed in the catch block.</param>
    /// <returns>The updated TryCatch<T> object with the catch task added.</returns>
    public static TryCatch<T> Catch<T>(this TryCatch<T> obj, Func<Exception, Task<T?>> func)
    {
        obj.CatchFunc = func;
        return obj;
    }

    /// <summary>
    /// Adds a finally task to the specified TryCatch object.
    /// </summary>
    /// <param name="obj">The TryCatch object to which the finally task will be added.</param>
    /// <param name="task">The task to be executed in the finally block.</param>
    /// <returns>The updated TryCatch object with the finally task added.</returns>
    public static TryCatch Finally(this TryCatch obj, Task task)
    {
        obj.Finally = task;
        return obj;
    }

    /// <summary>
    /// Adds a finally task to the specified TryCatch<T> object.
    /// </summary>
    /// <typeparam name="T">The type of the return value of the try and catch tasks.</typeparam>
    /// <param name="obj">The TryCatch<T> object to which the finally task will be added.</param>
    /// <param name="task">The task to be executed in the finally block.</param>
    /// <returns>The updated TryCatch<T> object with the finally task added.</returns>
    public static TryCatch<T> Finally<T>(this TryCatch<T> obj, Task task)
    {
        obj.Finally = task;
        return obj;
    }
}
