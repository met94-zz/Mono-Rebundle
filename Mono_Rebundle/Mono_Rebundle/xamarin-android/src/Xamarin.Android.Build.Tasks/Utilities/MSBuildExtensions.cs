using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

using Microsoft.Build.Framework;
using Mono_Xamarin_MkBundle;

namespace Xamarin.Android.Tasks
{
	public static class MSBuildExtensions
	{
		private static bool IsRunningInsideVS {
			get { 
				var vside = false;
				return bool.TryParse(Environment.GetEnvironmentVariable("VSIDE"), out vside) && vside; 
			}
		}

		public static void LogDebugMessage (this TaskLoggingHelper log, string message, params object[] messageArgs)
		{
			log.LogMessage (MessageImportance.Low, message, messageArgs);
		}

		public static void LogTaskItems (this TaskLoggingHelper log, string message, ITaskItem[] items)
		{
			log.LogMessage (message);

			if (items == null)
				return;

			foreach (var item in items)
				log.LogMessage ("    {0}", item.ItemSpec);
		}

		public static void LogTaskItems (this TaskLoggingHelper log, string message, params string[] items)
		{
			log.LogMessage (message);

			if (items == null)
				return;

			foreach (var item in items)
				log.LogMessage ("    {0}", item);
		}

		public static void LogDebugTaskItems (this TaskLoggingHelper log, string message, ITaskItem[] items, bool logMetadata = false)
		{
			log.LogMessage (MessageImportance.Low, message);

			if (items == null)
				return;

			foreach (var item in items) {
				log.LogMessage (MessageImportance.Low, "    {0}", item.ItemSpec);
				if (!logMetadata || item.MetadataCount <= 0)
					continue;
				foreach (string name in item.MetadataNames)
					log.LogMessage (MessageImportance.Low, $"       {name} = {item.GetMetadata (name)}");
			}
		}

		public static void LogDebugTaskItems (this TaskLoggingHelper log, string message, params string[] items)
		{
			log.LogMessage (MessageImportance.Low, message);

			if (items == null)
				return;

			foreach (var item in items)
				log.LogMessage (MessageImportance.Low, "    {0}", item);
		}

		// looking for: mandroid: warning XA9000: message...
		static readonly Regex Message = new Regex (
				@"^(?<source>[^: ]+)\s*:\s*(?<type>warning|error) (?<code>[^:]+): (?<message>.*)");

		
		public static void LogCodedError (this TaskLoggingHelper log, string code, string message, params object[] messageArgs)
		{
			log.LogError (string.Empty, code, string.Empty, string.Empty, 0, 0, 0, 0, message, messageArgs);
		}
	}
}
