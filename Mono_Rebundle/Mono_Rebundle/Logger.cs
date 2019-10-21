using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mono_Xamarin_MkBundle
{
    public class TaskLoggingHelper
    {
        public bool HasLoggedErrors { get; internal set; } = false;

        internal void LogMessage(string message, params object[] messageArgs)
        {
            Console.WriteLine(message, messageArgs);
        }

        internal void LogMessage(MessageImportance importance, string message, params object[] messageArgs)
        {
            Console.Write($"{importance} ");
            Console.WriteLine(message, messageArgs);
        }

        internal void LogError(string empty1, string code, string empty2, string empty3, int v1, int v2, int v3, int v4, string message, object[] messageArgs)
        {
            HasLoggedErrors = true;
            Console.Write($"Error Code {code}");
            LogMessage(message, messageArgs);
        }

        internal void LogErrorFromException(Exception ex)
        {
            HasLoggedErrors = true;
            Console.WriteLine("Exception: {0}", ex);
        }

        internal void LogCodedWarning(string code, string message, params object[] messageArgs)
        {
            Console.Write($"Warning Code {code}");
            LogMessage(message, messageArgs);
        }
    }
}
