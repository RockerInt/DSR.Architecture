using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dsr.Architecture.Utilities.TryCatch;

/// <summary>
/// Provides a structured way to handle try, catch, and finally blocks asynchronously.
/// </summary>
public class TryCatch
{
    /// <summary>
    /// Task to execute in the try block.
    /// </summary>
    public Task? Try { get; set; }

    /// <summary>
    /// Task to execute in the catch block if an exception occurs.
    /// </summary>
    public Task? Catch { get; set; }

    /// <summary>
    /// Function to execute in the catch block if an exception occurs.
    /// </summary>
    public Func<Exception, Task>? CatchFunc { get; set; }

    /// <summary>
    /// Task to execute in the finally block after try and catch blocks.
    /// </summary>
    public Task? Finally { get; set; }

    /// <summary>
    /// Executes the try, catch, and finally blocks asynchronously.
    /// </summary>
    public async Task Apply()
    {
        try
        {
            // Attempt to execute the try block if it is not null.
            if (Try is not null)
                await Try;
        }
        catch (Exception ex)
        {
            // Log the exception to the console.
            Console.WriteLine(ex.ToString());

            // Execute the catch function block if it is not null.
            if (CatchFunc is not null)
                await CatchFunc(ex);
            // Execute the catch block if it is not null.
            else if (Catch is not null)
                await Catch;
        }
        finally
        {
            // Execute the finally block if it is not null.
            if (Finally is not null)
                await Finally;
        }
    }
}

/// <summary>
/// Provides a structured way to handle try, catch, and finally blocks asynchronously with a return value.
/// </summary>
/// <typeparam name="T">The type of the return value.</typeparam>
public class TryCatch<T>
{
    /// <summary>
    /// Task to execute in the try block.
    /// </summary>
    public Task<T?>? Try { get; set; }

    /// <summary>
    /// Task to execute in the catch block if an exception occurs.
    /// </summary>
    public Task<T?>? Catch { get; set; }

    /// <summary>
    /// Function to execute in the catch block if an exception occurs.
    /// </summary>
    public Func<Exception, Task<T?>>? CatchFunc { get; set; }

    /// <summary>
    /// Task to execute in the finally block after try and catch blocks.
    /// </summary>
    public Task? Finally { get; set; }

    /// <summary>
    /// Executes the try, catch, and finally blocks asynchronously and returns a result of type T.
    /// </summary>
    /// <returns>The result of the try or catch block.</returns>
    public async Task<T?> Apply()
    {
        T? result = default;
        try
        {
            // Attempt to execute the try block if it is not null and capture the result.
            if (Try is not null)
                result = await Try;
        }
        catch (Exception ex)
        {
            // Log the exception to the console.
            Console.WriteLine(ex.ToString());

            // Execute the catch function block if it is not null and capture the result.
            if (CatchFunc is not null)
                result = await CatchFunc(ex);
            // Execute the catch block if it is not null and capture the result.
            else if (Catch is not null)
                result = await Catch;
        }
        finally
        {
            // Execute the finally block if it is not null.
            if (Finally is not null)
                await Finally;
        }

        // Return the result of the try or catch block.
        return result;
    }
}


