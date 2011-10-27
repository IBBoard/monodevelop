// VersionInformationTabPage.cs
//
// Author:
//   Viktoria Dudka (viktoriad@remobjects.com)
//   Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2009 RemObjects Software
// Copyright (c) 2011 Xamarin Inc. (http://xamarin.com)
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
//
//

using System;
using Gtk;
using MonoDevelop.Core;
using System.Reflection;
using System.Text;
using System.IO;
using MonoDevelop.Ide.Fonts;
using Mono.Addins;


namespace MonoDevelop.Ide.Gui.Dialogs
{
	internal class VersionInformationTabPage: VBox
	{
		//FIXME: move this somewhere it can be accessed by the error reporting code
		static string GetVersionInformation ()
		{
			var sb = new StringBuilder ();
			
			string mdversion = BuildVariables.PackageVersion == BuildVariables.PackageVersionLabel
				? BuildVariables.PackageVersionLabel
				: string.Format ("{0} ({1})", BuildVariables.PackageVersionLabel, BuildVariables.PackageVersion);
			sb.Append ("MonoDevelop ");
			sb.AppendLine (mdversion);
			
			sb.AppendFormat ("Installation UUID: {0}", PropertyService.Get<string> ("MonoDevelop.Core.InstallUuid", Guid.NewGuid ().ToString ()));
			sb.AppendLine ();
			
			var biFile = ((FilePath)typeof(VersionInformationTabPage).Assembly.Location).ParentDirectory.Combine ("buildinfo");
			if (File.Exists (biFile)) {
				sb.AppendLine ("Build information:");
				foreach (var line in File.ReadAllLines (biFile)) {
					if (!string.IsNullOrWhiteSpace (line)) {
						sb.Append ("\t");
						sb.AppendLine (line.Trim ());
					}
				}
			}
			
			sb.AppendLine ("Operating System:");
			if (Platform.IsMac) {
				sb.AppendFormat ("\tMac OS X {0}.{1}.{2}", Gestalt ("sys1"), Gestalt ("sys2"), Gestalt ("sys3"));
			} else if (Platform.IsWindows) {
				sb.Append ("\tWindows ");
				sb.Append (Environment.OSVersion.Version.ToString ());
				if (IntPtr.Size == 8 || Environment.GetEnvironmentVariable ("PROCESSOR_ARCHITEW6432") != null)
					sb.Append (" (64-bit)");
			} else {
				sb.Append ("\tLinux ");
			}
			sb.AppendLine ();
			
			if (!Platform.IsWindows) {
				var psi = new System.Diagnostics.ProcessStartInfo ("uname", "-a") {
					RedirectStandardOutput = true,
					UseShellExecute = false,
				};
				var process = System.Diagnostics.Process.Start (psi);
				process.WaitForExit (500);
				if (process.HasExited && process.ExitCode == 0)
					sb.Append ("\t");
					sb.AppendLine (process.StandardOutput.ReadLine ());
			}
			
			sb.AppendLine ("Runtime:");
			if (IsMono ()) {
				sb.Append ("\tMono " + GetMonoVersionNumber ());
			} else {
				sb.Append ("\tMicrosoft .NET " + Environment.Version);
			}
			
			if (IntPtr.Size == 8)
				sb.Append (" (64-bit)");
			sb.AppendLine ();
				
			sb.Append ("\tGTK " + GetGtkVersion ());
			sb.AppendLine (" (GTK# " + typeof(VBox).Assembly.GetName ().Version + ")");
			
			foreach (var info in AddinManager.GetExtensionObjects<ISystemInformationProvider> ("/MonoDevelop/Core/SystemInformation", false)) {
				try {
					sb.AppendLine (info.Description);
				} catch (Exception ex) {
					LoggingService.LogError ("Error getting about information: ", ex);
				}
			}
			
			sb.AppendLine ("Loaded assemblies:");
			
			int nameLength = 0;
			int versionLength = 0;
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies ()) {
				try {
					if (assembly.IsDynamic)
						continue;
					var assemblyName = assembly.GetName ();
					nameLength = Math.Max (nameLength, assemblyName.Name.Length);
					versionLength = Math.Max (versionLength, assemblyName.Version.ToString ().Length);
				} catch {
				}
			}
			nameLength++;
			versionLength++;
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies ()) {
				try {
					if (assembly.IsDynamic)
						continue;
					var assemblyName = assembly.GetName ();
					sb.AppendLine (assemblyName.Name.PadRight (nameLength) + assemblyName.Version.ToString ().PadRight (versionLength) + System.IO.Path.GetFullPath (assembly.Location));
				} catch {
				}
			}
			
			return sb.ToString ();
		}
		
		static bool IsMono ()
		{
			return Type.GetType ("Mono.Runtime") != null;
		}
		
		static string GetMonoVersionNumber ()
		{
			var t = Type.GetType ("Mono.Runtime"); 
			if (t == null)
				return "unknown";
			var mi = t.GetMethod ("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
			if (mi == null) {
				LoggingService.LogError ("No Mono.Runtime.GetDiplayName method found.");
				return "error";
			}
			return (string)mi.Invoke (null, null); 
		}
		
		static string GetGtkVersion ()
		{
			uint v1 = 2, v2 = 0, v3 = 0;
			
			while (v1 < 99 && Gtk.Global.CheckVersion (v1, v2, v3) == null)
				v1++;
			v1--;
			
			while (v2 < 99 && Gtk.Global.CheckVersion (v1, v2, v3) == null)
				v2++;
			v2--;
			
			v3 = 0;
			while (v3 < 99 && Gtk.Global.CheckVersion (v1, v2, v3) == null)
				v3++;
			v3--;
			
			if (v1 == 99 || v2 == 99 || v3 == 99)
				return "unknown";
			return v1 +"." + v2 + "."+ v3;
		}
		
		[System.Runtime.InteropServices.DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int Gestalt (int selector, out int result);
		
		//TODO: there are other gestalt selectors that return info we might want to display
		//mac API for obtaining info about the system
		static int Gestalt (string selector)
		{
			System.Diagnostics.Debug.Assert (selector != null && selector.Length == 4);
			int cc = selector[3] | (selector[2] << 8) | (selector[1] << 16) | (selector[0] << 24);
			int result;
			int ret = Gestalt (cc, out result);
			if (ret != 0)
				throw new Exception (string.Format ("Error reading gestalt for selector '{0}': {1}", selector, ret));
			return result;
		}
		
		public VersionInformationTabPage ()
		{
			var buf = new TextBuffer (null);
			buf.Text = GetVersionInformation ();
			
			var sw = new ScrolledWindow () {
				BorderWidth = 6,
				ShadowType = ShadowType.EtchedIn,
				Child = new TextView (buf) {
					Editable = false,
					LeftMargin = 4,
					RightMargin = 4,
					PixelsAboveLines = 4,
					PixelsBelowLines = 4
				}
			};
			
			sw.Child.ModifyFont (Pango.FontDescription.FromString (DesktopService.DefaultMonospaceFont));
			PackStart (sw, true, true, 0);
		}
	}
}
