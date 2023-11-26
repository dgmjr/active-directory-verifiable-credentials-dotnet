using Microsoft.Extensions.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace AspNetCoreVerifiableCredentials;

public static partial class LoggingExtensions
{
    [LoggerMessage(Trace, "IssuanceRequest file: {payloadPath}")]
    public static partial void LogIssuanceRequest(this ILogger logger, string payloadPath);

    [LoggerMessage(Error, "Error reading file: {payloadPath}")]
    public static partial void LogErrorReadingFile(this ILogger logger, string payloadPath);

    [LoggerMessage(Error, "File not found: {payloadPath}")]
    public static partial void LogFileNotFoundError(this ILogger logger, string payloadPath);

    [LoggerMessage(
        Trace,
        "pin element found in JSON payload, but on mobile so remove pin since we will be using deeplinking"
    )]
    public static partial void LogPinElementFoundInJsonPayload_RemovingPin(this ILogger logger);

    [LoggerMessage(
        Trace,
        "pin element found in JSON payload, modifying to a random number of the specific length"
    )]
    public static partial void LogPinElementFoundInJsonPayload_UsingRandomNumber(
        this ILogger logger
    );

    [LoggerMessage(
        Trace,
        "VCCallbackHostURL is not set in the AppSettings section of appsettings.json file. Please refer to README section on Running the sample for instructions on how to set this value."
    )]
    public static partial void LogVCCallbackHostUrlNotSetError(this ILogger logger);
}
