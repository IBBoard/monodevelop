// 
// MsNetFrameworkBackend.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;

namespace MonoDevelop.Core.Assemblies
{
	public class MsNetFrameworkBackend: TargetFrameworkBackend<MsNetTargetRuntime>
	{
		public override bool IsInstalled {
			get {
				//TODO: it should be possible to support any framework by reading the MS framework definition files
				return base.IsInstalled;
			}
		}
		
		public override IEnumerable<string> GetFrameworkFolders ()
		{
			if (framework.Id.Identifier != TargetFrameworkMoniker.ID_NET_FRAMEWORK)
				yield break;
			
			switch (framework.Id.Version) {
			case "1.1":
			case "2.0":
			case "4.0":
				yield return targetRuntime.RootDirectory.Combine (GetClrVersion (framework.ClrVersion));
				break;
			case "3.0":
			case "3.5":
				RegistryKey fxFolderKey = Registry.LocalMachine.OpenSubKey (@"SOFTWARE\Microsoft\.NETFramework\AssemblyFolders\v" + framework.Id.Version, false);
				if (fxFolderKey != null) {
					string folder = fxFolderKey.GetValue ("All Assemblies In") as string;
					fxFolderKey.Close ();
					yield return folder;
				}
				break;
			}
		}
		
		public override Dictionary<string, string> GetToolsEnvironmentVariables ()
		{
			Dictionary<string, string> vars = new Dictionary<string, string> ();
			string path = Environment.GetEnvironmentVariable ("PATH");
			vars["PATH"] = GetFrameworkToolsPath () + Path.PathSeparator + path;
			return vars;
		}

		public override IEnumerable<string> GetToolsPaths ()
		{
			string path = GetFrameworkToolsPath ();
			if (path != null)
				yield return path;

			string sdkPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ProgramFiles), "Microsoft SDKs");
			sdkPath = Path.Combine (sdkPath, "Windows");
			if (framework.Id.Version == "4.0")
				yield return Path.Combine (sdkPath, "v7.0A\\bin\\NETFX 4.0 Tools");
			else if (framework.Id.Version == "3.5") {
				yield return Path.Combine (sdkPath, "v7.0A\\bin");
				yield return targetRuntime.RootDirectory.Combine (GetClrVersion (ClrVersion.Net_2_0));
			} else
				yield return Path.Combine (sdkPath, "v6.0A\\bin");

			foreach (string s in BaseGetToolsPaths ())
				yield return s;
			yield return PropertyService.EntryAssemblyPath;
		}

		//base isn't verifiably accessible from the enumerator so use this private helper
		IEnumerable<string> BaseGetToolsPaths ()
		{
			return base.GetToolsPaths ();
		}

		string GetFrameworkToolsPath ()
		{
			var version = framework.Id.Version;
			if (version == "1.1" || version == "2.0" || version == "4.0")
				return targetRuntime.RootDirectory.Combine (GetClrVersion (framework.ClrVersion));

			if (version == "3.0")
				return targetRuntime.RootDirectory.Combine (GetClrVersion (ClrVersion.Net_2_0));
 
			return targetRuntime.RootDirectory.Combine ("v" + version);
		}
		
		internal static string GetClrVersion (ClrVersion v)
		{
			switch (v) {
				case ClrVersion.Net_1_1: return "v1.1.4322";
				case ClrVersion.Net_2_0: return "v2.0.50727";
				case ClrVersion.Clr_2_1: return "v2.1";
				case ClrVersion.Net_4_0: return "v4.0.30319";
			}
			return "?";
		}
	}
}
