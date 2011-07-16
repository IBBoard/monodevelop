//
// SystemAssemblyService.cs
//
// Author:
//   Todd Berman <tberman@sevenl.net>
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (C) 2004 Todd Berman
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Threading;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core.AddIns;
using MonoDevelop.Core.Serialization;
using Mono.Addins;
using Mono.Cecil;

namespace MonoDevelop.Core.Assemblies
{
	public class SystemAssemblyService
	{
		List<TargetFramework> frameworks;
		List<TargetRuntime> runtimes;
		TargetRuntime defaultRuntime;
		DirectoryAssemblyContext userAssemblyContext = new DirectoryAssemblyContext ();
		
		internal static string ReferenceFrameworksPath;
		internal static string GeneratedFrameworksFile;
		
		public TargetRuntime CurrentRuntime { get; private set; }
		
		public event EventHandler DefaultRuntimeChanged;
		public event EventHandler RuntimesChanged;
		
		internal SystemAssemblyService ()
		{
			ReferenceFrameworksPath = Environment.GetEnvironmentVariable ("MONODEVELOP_FRAMEWORKS_FILE");
			GeneratedFrameworksFile = Environment.GetEnvironmentVariable ("MONODEVELOP_FRAMEWORKS_OUTFILE");
		}
		
		internal void Initialize ()
		{
			CreateFrameworks ();
			runtimes = new List<TargetRuntime> ();
			foreach (ITargetRuntimeFactory factory in AddinManager.GetExtensionObjects ("/MonoDevelop/Core/Runtimes", typeof(ITargetRuntimeFactory))) {
				foreach (TargetRuntime runtime in factory.CreateRuntimes ()) {
					runtimes.Add (runtime);
					if (runtime.IsRunning)
						DefaultRuntime = CurrentRuntime = runtime;
				}
			}
			
			// Don't initialize until Current and Default Runtimes are set
			foreach (TargetRuntime runtime in runtimes)
				runtime.StartInitialization ();
			
			if (CurrentRuntime == null)
				LoggingService.LogFatalError ("Could not create runtime info for current runtime");
			
			LoadUserAssemblyContext ();
			userAssemblyContext.Changed += delegate {
				SaveUserAssemblyContext ();
			};
		}
		
		public TargetRuntime DefaultRuntime {
			get {
				return defaultRuntime;
			}
			set {
				defaultRuntime = value;
				if (DefaultRuntimeChanged != null)
					DefaultRuntimeChanged (this, EventArgs.Empty);
			}
		}
		
		public DirectoryAssemblyContext UserAssemblyContext {
			get { return userAssemblyContext; }
		}
		
		public IAssemblyContext DefaultAssemblyContext {
			get { return DefaultRuntime.AssemblyContext; }
		}
		
		public void RegisterRuntime (TargetRuntime runtime)
		{
			runtime.StartInitialization ();
			runtimes.Add (runtime);
			if (RuntimesChanged != null)
				RuntimesChanged (this, EventArgs.Empty);
		}
		
		public void UnregisterRuntime (TargetRuntime runtime)
		{
			if (runtime == CurrentRuntime)
				return;
			DefaultRuntime = CurrentRuntime;
			runtimes.Remove (runtime);
			if (RuntimesChanged != null)
				RuntimesChanged (this, EventArgs.Empty);
		}
		
		internal IEnumerable<TargetFramework> GetCoreFrameworks ()
		{
			return frameworks;
		}
		
		public IEnumerable<TargetFramework> GetTargetFrameworks ()
		{
			var keys = new HashSet<TargetFrameworkMoniker> ();
			foreach (var fx in frameworks)
				if (keys.Add (fx.Id))
					yield return fx;
			foreach (var rt in runtimes)
				foreach (var f in rt.CustomFrameworks)
					if (keys.Add (f.Id))
						yield return f;
		}
		
		public IEnumerable<TargetRuntime> GetTargetRuntimes ()
		{
			return runtimes;
		}
		
		public TargetRuntime GetTargetRuntime (string id)
		{
			foreach (TargetRuntime r in runtimes) {
				if (r.Id == id)
					return r;
			}
			return null;
		}

		public IEnumerable<TargetRuntime> GetTargetRuntimes (string runtimeId)
		{
			foreach (TargetRuntime r in runtimes) {
				if (r.RuntimeId == runtimeId)
					yield return r;
			}
		}

		public TargetFramework GetTargetFramework (TargetFrameworkMoniker id)
		{
			foreach (TargetFramework fx in frameworks)
				if (fx.Id == id)
					return fx;
			LoggingService.LogWarning ("Unregistered TargetFramework '{0}' is being requested from SystemAssemblyService", id);
			TargetFramework f = new TargetFramework (id);
			frameworks.Add (f);
			return f;
		}
		
		public SystemPackage GetPackageFromPath (string assemblyPath)
		{
			foreach (TargetRuntime r in runtimes) {
				SystemPackage p = r.AssemblyContext.GetPackageFromPath (assemblyPath);
				if (p != null)
					return p;
			}
			return null;
		}

		public static AssemblyName ParseAssemblyName (string fullname)
		{
			AssemblyName aname = new AssemblyName ();
			int i = fullname.IndexOf (',');
			if (i == -1) {
				aname.Name = fullname.Trim ();
				return aname;
			}
			
			aname.Name = fullname.Substring (0, i).Trim ();
			i = fullname.IndexOf ("Version", i+1);
			if (i == -1)
				return aname;
			i = fullname.IndexOf ('=', i);
			if (i == -1) 
				return aname;
			int j = fullname.IndexOf (',', i);
			if (j == -1)
				aname.Version = new Version (fullname.Substring (i+1).Trim ());
			else
				aname.Version = new Version (fullname.Substring (i+1, j - i - 1).Trim ());
			return aname;
		}
		
		internal static System.Reflection.AssemblyName GetAssemblyNameObj (string file)
		{
			try {
				AssemblyDefinition asm = AssemblyDefinition.ReadAssembly (file);
				return new AssemblyName (asm.Name.FullName);
				
				// Don't use reflection to get the name since it is a common cause for deadlocks
				// in Mono < 2.6.
				// return System.Reflection.AssemblyName.GetAssemblyName (file);
				
			} catch (FileNotFoundException) {
				// GetAssemblyName is not case insensitive in mono/windows. This is a workaround
				foreach (string f in Directory.GetFiles (Path.GetDirectoryName (file), Path.GetFileName (file))) {
					if (f != file)
						return GetAssemblyNameObj (f);
				}
				throw;
			} catch (BadImageFormatException) {
				AssemblyDefinition asm = AssemblyDefinition.ReadAssembly (file);
				return new AssemblyName (asm.Name.FullName);
			}
		}
		
		public static string GetAssemblyName (string file)
		{
			return AssemblyContext.NormalizeAsmName (GetAssemblyNameObj (file).ToString ());
		}
		
		internal static bool UseExpandedFrameworksFile {
			get { return ReferenceFrameworksPath == null; }
		}
		
		internal static bool UpdateExpandedFrameworksFile {
			get { return GeneratedFrameworksFile != null; }
		}
		
		protected void CreateFrameworks ()
		{
			frameworks = new List<TargetFramework> ();
			foreach (TargetFrameworkNode node in AddinManager.GetExtensionNodes ("/MonoDevelop/Core/Frameworks")) {
				try {
					frameworks.Add (node.CreateFramework ());
				} catch (Exception ex) {
					LoggingService.LogError ("Could not load framework '" + node.Id + "'", ex);
				}
			}
			
			// Find framework realtions
			foreach (TargetFramework fx in frameworks)
				BuildFrameworkRelations (fx);

			if (!UseExpandedFrameworksFile)
				LoadKnownAssemblyVersions ();
		}
		
		void BuildFrameworkRelations (TargetFramework fx)
		{
			if (fx.RelationsBuilt)
				return;
			
			var includesFramework = fx.GetIncludesFramework ();
			if (includesFramework != null) {
				fx.IncludedFrameworks.Add (includesFramework);
				TargetFramework compatFx = GetTargetFramework (includesFramework);
				BuildFrameworkRelations (compatFx);
				fx.IncludedFrameworks.AddRange (compatFx.IncludedFrameworks);
			}
			
			fx.RelationsBuilt = true;
		}

		void LoadKnownAssemblyVersions ()
		{
			Stream s = AddinManager.CurrentAddin.GetResource ("frameworks.xml");
			XmlDocument doc = new XmlDocument ();
			doc.Load (s);
			s.Close ();

			foreach (TargetFramework fx in frameworks) {
				foreach (AssemblyInfo ai in fx.Assemblies) {
					XmlElement elem = (XmlElement) doc.DocumentElement.SelectSingleNode ("TargetFramework[@id='" + fx.Id + "']/Assemblies/Assembly[@name='" + ai.Name + "']");
					if (elem != null) {
						string v = elem.GetAttribute ("version");
						if (!string.IsNullOrEmpty (v))
							ai.Version = v;
						v = elem.GetAttribute ("publicKeyToken");
						if (!string.IsNullOrEmpty (v))
							ai.PublicKeyToken = v;
					}
				}
			}
		}
		
		internal void SaveGeneratedFrameworkInfo ()
		{
			if (GeneratedFrameworksFile != null) {
				Console.WriteLine ("Saving frameworks file: " + GeneratedFrameworksFile);
				using (StreamWriter sw = new StreamWriter (GeneratedFrameworksFile)) {
					XmlTextWriter tw = new XmlTextWriter (sw);
					tw.Formatting = Formatting.Indented;
					XmlDataSerializer ser = new XmlDataSerializer (new DataContext ());
					ser.Serialize (tw, frameworks);
				}
					
				XmlDocument doc = new XmlDocument ();
				doc.Load (GeneratedFrameworksFile);
				doc.DocumentElement.InsertBefore (doc.CreateComment ("This file has been autogenerated. DO NOT MODIFY!"), doc.DocumentElement.FirstChild);
				doc.Save (GeneratedFrameworksFile);
			}
		}
		
		public TargetFrameworkMoniker GetTargetFrameworkForAssembly (TargetRuntime tr, string file)
		{
			try {
				AssemblyDefinition asm = AssemblyDefinition.ReadAssembly (file);

				foreach (AssemblyNameReference aname in asm.MainModule.AssemblyReferences) {
					if (aname.Name == "mscorlib") {
						foreach (TargetFramework tf in GetTargetFrameworks ()) {
							if (tf.GetCorlibVersion () == aname.Version.ToString ())
								return tf.Id;
						}
						break;
					}
				}
			} catch {
				// Ignore
			}
			return TargetFrameworkMoniker.UNKNOWN;
		}
		
		void SaveUserAssemblyContext ()
		{
			List<string> list = new List<string> (userAssemblyContext.Directories);
			PropertyService.Set ("MonoDevelop.Core.Assemblies.UserAssemblyContext", list);
			PropertyService.SaveProperties ();
		}
		
		void LoadUserAssemblyContext ()
		{
			List<string> dirs = PropertyService.Get<List<string>> ("MonoDevelop.Core.Assemblies.UserAssemblyContext");
			if (dirs != null)
				userAssemblyContext.Directories = dirs;
		}
	}
}
