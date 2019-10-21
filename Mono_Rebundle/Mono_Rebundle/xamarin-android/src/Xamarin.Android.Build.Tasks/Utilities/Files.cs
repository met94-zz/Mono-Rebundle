using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

#if MSBUILD
using Microsoft.Build.Utilities;
using Xamarin.Android.Tasks;
#endif

namespace Xamarin.Android.Tools
{

	static class Files {

		/// <summary>
		/// Windows has a MAX_PATH limit of 260 characters
		/// See: https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file#maximum-path-length-limitation
		/// </summary>
		public const int MaxPath = 260;

		/// <summary>
		/// On Windows, we can opt into a long path with this prefix
		/// </summary>
		public const string LongPathPrefix = @"\\?\";

		/// <summary>
		/// Converts a full path to a \\?\ prefixed path that works on all Windows machines when over 260 characters
		/// NOTE: requires a *full path*, use sparingly
		/// </summary>
		public static string ToLongPath (string fullPath)
		{
			// On non-Windows platforms, return the path unchanged
			if (Path.DirectorySeparatorChar != '\\') {
				return fullPath;
			}
			return LongPathPrefix + fullPath;
		}

		public static bool ArchiveZipUpdate(string target, Action<string> archiver)
		{
			var lastWrite = File.Exists (target) ? File.GetLastWriteTimeUtc (target) : DateTime.MinValue;
			archiver (target);
			return lastWrite < File.GetLastWriteTimeUtc (target);
		}

		public static string HashFile (string filename, HashAlgorithm hashAlg)
		{
			using (Stream file = new FileStream (filename, FileMode.Open, FileAccess.Read)) {
				byte[] hash = hashAlg.ComputeHash (file);
				return ToHexString (hash);
			}
		}

		public static string ToHexString (byte[] hash)
		{
			char [] array = new char [hash.Length * 2];
			for (int i = 0, j = 0; i < hash.Length; i += 1, j += 2) {
				byte b = hash [i];
				array [j] = GetHexValue (b / 16);
				array [j + 1] = GetHexValue (b % 16);
			}
			return new string (array);
		}

		static char GetHexValue (int i) => (char) (i < 10 ? i + 48 : i - 10 + 65);

		public static void DeleteFile (string filename, object log)
		{
			try {
				File.Delete (filename);
			} catch (Exception ex) {
#if MSBUILD
				var helper = log as TaskLoggingHelper;
				helper.LogErrorFromException (ex);
#else
				Console.Error.WriteLine (ex.ToString ());
#endif
			}
		}

		const uint ppdb_signature = 0x424a5342;

		public static bool IsPortablePdb (string filename)
		{
			try {
				using (var fs = new FileStream (filename, FileMode.Open, FileAccess.Read)) {
					using (var br = new BinaryReader (fs)) {
						return br.ReadUInt32 () == ppdb_signature;
					}
				}
			}
			catch {
				return false;
			}
		}
	}
}

