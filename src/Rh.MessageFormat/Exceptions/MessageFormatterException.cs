using System;

namespace Rh.MessageFormat.Exceptions;

/// <summary>
///     Thrown when an issue has occurred in the message formatting process.
/// </summary>
public class MessageFormatterException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageFormatterException" /> class.
    /// </summary>
    /// <param name="message">
    ///     The message that describes the error.
    /// </param>
    public MessageFormatterException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MessageFormatterException" /> class.
    /// </summary>
    /// <param name="message">
    ///     The message that describes the error.
    /// </param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception.
    /// </param>
    public MessageFormatterException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}