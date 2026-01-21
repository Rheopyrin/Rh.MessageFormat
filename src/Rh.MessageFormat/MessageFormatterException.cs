using System;

namespace Rh.MessageFormat;

/// <summary>
///     Thrown when an issue has occured in the message formatting process.
/// </summary>
public class MessageFormatterException : Exception
{
    #region Constructors and Destructors

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

    #endregion
}