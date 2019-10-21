using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Android.Tasks;

namespace Mono_Xamarin_MkBundle
{
    [Verb("produce-stub", HelpText = "Produce stub only, do not compile")]
    class ProduceStabOptions
    {
        [Option('p', "path", Required = true, HelpText = "Path to assemblies to bundle")]
        public string AssembliesPath { get; set; }
    }

    [Verb("link", HelpText = "Link into libmonodroid_bundle_app")]
    class BundleOptions
    {
    }

    public class Program
    {
        static void Main(string[] args)
        {
            bool error = false;
            string androidNdkDirectory = string.Empty;
            string bundleApiPath = string.Empty;
            string supportedAbis = string.Empty;
            string toolPath = string.Empty;
            string assembliesPath = string.Empty;
            string configPath = $".{Path.DirectorySeparatorChar}config.json";
            bool produceStab = false;
            bool link = false;
            try
            {
                if (!File.Exists(configPath))
                {
                    Console.WriteLine("Missing config file!");
                    Console.WriteLine("Creating config.json, do not forget to setup it before running!");
                    JObject configProperties = new JObject(
                        new JProperty("AndroidNdkDirectory", @"C:\Users\AB\AppData\Local\Android\Sdk\ndk\20.0.5594570\"),
                        new JProperty("BundleApiPath", @"E:\xamarin-android\src\monodroid\jni\mkbundle-api.h"),
                        new JProperty("SupportedAbis", "armeabi-v7a"),
                        new JProperty("ToolPath", @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Xamarin\Android"));
                    using (StreamWriter file = File.CreateText(configPath))
                    using (JsonTextWriter writer = new JsonTextWriter(file))
                    {
                        configProperties.WriteTo(writer);
                    }
                    return;
                }
                Console.WriteLine("Reading config file.");
                JObject o1 = JObject.Parse(File.ReadAllText(configPath));
                androidNdkDirectory = o1["AndroidNdkDirectory"].Value<string>();
                Console.WriteLine($"AndroidNdkDirectory: {androidNdkDirectory}");
                if (!Directory.Exists(androidNdkDirectory))
                {
                    Console.WriteLine($"There is no such directory: {androidNdkDirectory}");
                    return;
                }
                bundleApiPath = o1["BundleApiPath"].Value<string>();
                Console.WriteLine($"BundleApiPath: {bundleApiPath}");
                if (!File.Exists(bundleApiPath))
                {
                    Console.WriteLine($"There is no such file: {bundleApiPath}");
                    return;
                }
                toolPath = o1["ToolPath"].Value<string>();
                Console.WriteLine($"ToolPath: {toolPath}");
                if (!Directory.Exists(toolPath))
                {
                    Console.WriteLine($"There is no such directory: {toolPath}");
                    return;
                }
                supportedAbis = o1["SupportedAbis"].Value<string>();
                Console.WriteLine($"SupportedAbis: {supportedAbis}");
                if (string.IsNullOrWhiteSpace(supportedAbis))
                {
                    Console.WriteLine($"There is no such directory: {toolPath}");
                    return;
                }
                Parser.Default.ParseArguments<ProduceStabOptions, BundleOptions>(args)
                  .WithParsed<ProduceStabOptions>(opts =>
                  {
                      assembliesPath = opts.AssembliesPath;
                      produceStab = true;
                  })
                  .WithParsed<BundleOptions>(opts =>
                  {
                      link = true;
                  })
                  .WithNotParsed((errs) =>
                  {
                      error = true;
                      errs.ToList().ForEach(e => Console.WriteLine(e.ToString()));
                  });
            }
            catch (Exception e)
            {
                error = true;
                Console.WriteLine(e);
            }
            if (error)
            {
                Console.WriteLine("Exiting due to a fatal error!");
                return;
            }
            var mkBundle = new MakeBundleNativeCodeExternal();
            mkBundle.KeepTemp = false;
            mkBundle.AndroidNdkDirectory = androidNdkDirectory;
            string[] assemblies = produceStab ? Directory.GetFiles(assembliesPath, "*.*", SearchOption.AllDirectories) : new string[0];
            mkBundle.Assemblies = assemblies;
            mkBundle.IncludePath = "";
            mkBundle.SupportedAbis = supportedAbis.Split(';');
            mkBundle.TempOutputPath = ".\\temp\\";
            mkBundle.ToolPath = toolPath;
            mkBundle.BundleApiPath = bundleApiPath;
            mkBundle.ProduceStub = produceStab;
            mkBundle.Link = link;
            if (!produceStab && !link)
            {
                return;
            }
            mkBundle.RunTask();
        }
    }
}
