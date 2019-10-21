﻿using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Xamarin.Android.Tools
{
	abstract class AndroidSdkBase
	{
		string[] allAndroidSdks = null;
		string[] allAndroidNdks = null;

		public string[] AllAndroidSdks {
			get {
				if (allAndroidSdks == null)
					allAndroidSdks = GetAllAvailableAndroidSdks ().Distinct ().ToArray ();
				return allAndroidSdks;
			}
		}
		public string[] AllAndroidNdks {
			get {
				if (allAndroidNdks == null)
					allAndroidNdks = GetAllAvailableAndroidNdks ().Distinct ().ToArray ();
				return allAndroidNdks;
			}
		}

		public readonly Action<TraceLevel, string> Logger;

		public AndroidSdkBase (Action<TraceLevel, string> logger)
		{
			Logger  = logger;
		}

		public string AndroidSdkPath { get; private set; }
		public string AndroidNdkPath { get; private set; }
		public string JavaSdkPath { get; private set; }
		public string JavaBinPath { get; private set; }
		public string AndroidToolsPath { get; private set; }
		public string AndroidPlatformToolsPath { get; private set; }
		public string AndroidToolsPathShort { get; private set; }
		public string AndroidPlatformToolsPathShort { get; private set; }

		public virtual string Adb { get; protected set; } = "adb";
		public virtual string Android { get; protected set; } = "android";
		public virtual string Emulator { get; protected set; } = "emulator";
		public virtual string Monitor { get; protected set; } = "monitor";
		public virtual string ZipAlign { get; protected set; } = "zipalign";
		public virtual string JarSigner { get; protected set; } = "jarsigner";
		public virtual string KeyTool { get; protected set; } = "keytool";

		public virtual string NdkStack { get; protected set; } = "ndk-stack";
		public abstract string NdkHostPlatform32Bit { get; }
		public abstract string NdkHostPlatform64Bit { get; }
		public virtual string Javac { get; protected set; } = "javac";

		public abstract string PreferedAndroidSdkPath { get; }
		public abstract string PreferedAndroidNdkPath { get; }
		public abstract string PreferedJavaSdkPath { get; }

		public virtual void Initialize (string androidSdkPath = null, string androidNdkPath = null, string javaSdkPath = null)
		{
			androidSdkPath  = androidSdkPath ?? PreferedAndroidSdkPath;
			androidNdkPath  = androidNdkPath ?? PreferedAndroidNdkPath;
			javaSdkPath     = javaSdkPath ?? PreferedJavaSdkPath;

			AndroidSdkPath  = ValidateAndroidSdkLocation (androidSdkPath) ? androidSdkPath : AllAndroidSdks.FirstOrDefault ();
			AndroidNdkPath  = ValidateAndroidNdkLocation (androidNdkPath) ? androidNdkPath : AllAndroidNdks.FirstOrDefault ();
			JavaSdkPath     = ValidateJavaSdkLocation (javaSdkPath) ? javaSdkPath : GetJavaSdkPath ();

			if (!string.IsNullOrEmpty (JavaSdkPath)) {
				JavaBinPath = Path.Combine (JavaSdkPath, "bin");
			} else {
				JavaBinPath = null;
			}

			if (!string.IsNullOrEmpty (AndroidSdkPath)) {
				AndroidToolsPath = Path.Combine (AndroidSdkPath, "tools");
				AndroidToolsPathShort = GetShortFormPath (AndroidToolsPath);
				AndroidPlatformToolsPath = Path.Combine (AndroidSdkPath, "platform-tools");
				AndroidPlatformToolsPathShort = GetShortFormPath (AndroidPlatformToolsPath);
			} else {
				AndroidToolsPath = null;
				AndroidToolsPathShort = null;
				AndroidPlatformToolsPath = null;
				AndroidPlatformToolsPathShort = null;
			}

			if (!string.IsNullOrEmpty (AndroidNdkPath)) {
				// It would be nice if .NET had real globbing support in System.IO...
				string toolchainsDir = Path.Combine (AndroidNdkPath, "toolchains");
				if (Directory.Exists (toolchainsDir)) {
					IsNdk64Bit = Directory.EnumerateDirectories (toolchainsDir, "arm-linux-androideabi-*")
						.Any (dir => Directory.Exists (Path.Combine (dir, "prebuilt", NdkHostPlatform64Bit)));
				}
			}
			// we need to look for extensions other than the default .exe|.bat
			// google have a habbit of changing them.
			Adb = GetExecutablePath (AndroidPlatformToolsPath, Adb);
			Android = GetExecutablePath (AndroidToolsPath, Android);
			Emulator = GetExecutablePath (AndroidToolsPath, Emulator);
			Monitor = GetExecutablePath (AndroidToolsPath, Monitor);
			NdkStack = GetExecutablePath (AndroidNdkPath, NdkStack);
		}

		protected abstract IEnumerable<string> GetAllAvailableAndroidSdks ();
		protected abstract IEnumerable<string> GetAllAvailableAndroidNdks ();
		protected abstract string GetJavaSdkPath ();
		protected abstract string GetShortFormPath (string path);

		public abstract void SetPreferredAndroidSdkPath (string path);
		public abstract void SetPreferredJavaSdkPath (string path);
		public abstract void SetPreferredAndroidNdkPath (string path);

		public bool IsNdk64Bit { get; private set; }

		public string NdkHostPlatform {
			get { return IsNdk64Bit ? NdkHostPlatform64Bit : NdkHostPlatform32Bit; }
		}

		/// <summary>
		/// Checks that a value is the location of an Android SDK.
		/// </summary>
		public bool ValidateAndroidSdkLocation (string loc)
		{
			bool result = !string.IsNullOrEmpty (loc) && ProcessUtils.FindExecutablesInDirectory (Path.Combine (loc, "platform-tools"), Adb).Any ();
			Logger (TraceLevel.Verbose, $"{nameof (ValidateAndroidSdkLocation)}: `{loc}`, result={result}");
			return result;
		}

		/// <summary>
		/// Checks that a value is the location of a Java SDK.
		/// </summary>
		public virtual bool ValidateJavaSdkLocation (string loc)
		{
			bool result = !string.IsNullOrEmpty (loc) && ProcessUtils.FindExecutablesInDirectory (Path.Combine (loc, "bin"), JarSigner).Any ();
			Logger (TraceLevel.Verbose, $"{nameof (ValidateJavaSdkLocation)}: `{loc}`, result={result}");
			return result;
		}

		/// <summary>
		/// Checks that a value is the location of an Android SDK.
		/// </summary>
		public bool ValidateAndroidNdkLocation (string loc)
		{
			bool result = !string.IsNullOrEmpty (loc) && ProcessUtils.FindExecutablesInDirectory (loc, NdkStack).Any ();
			Logger (TraceLevel.Verbose, $"{nameof (ValidateAndroidNdkLocation)}: `{loc}`, result={result}");
			return result;
		}

		protected static string NullIfEmpty (string s)
		{
			if (s == null || s.Length != 0)
				return s;

			return null;
		}

		static string GetExecutablePath (string dir, string exe)
		{
			if (string.IsNullOrEmpty (dir))
				return exe;

			foreach (var e in ProcessUtils.ExecutableFiles (exe))
				if (File.Exists (Path.Combine (dir, e)))
					return e;
			return exe;
		}
	}
}

