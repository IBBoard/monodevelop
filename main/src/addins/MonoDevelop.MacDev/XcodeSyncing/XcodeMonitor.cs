// 
// XcodeMonitor.cs
//  
// Author:
//       Michael Hutchinson <mhutch@xamarin.com>
// 
// Copyright (c) Xamarin, Inc. (http://xamarin.com)
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
using System.Linq;
using System.IO;
using System.Collections.Generic;

using MonoDevelop.Core;
using MonoDevelop.Projects;
using MonoDevelop.MacDev.ObjCIntegration;
using System.Threading.Tasks;
using MonoDevelop.MacDev.XcodeIntegration;

using MonoDevelop.MacInterop;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace MonoDevelop.MacDev.XcodeSyncing
{
	class XcodeMonitor
	{
		FilePath originalProjectDir;
		int nextHackDir = 0;
		string name;
		
		FilePath xcproj, projectDir;
		List<XcodeSyncedItem> items;
		Dictionary<string,XcodeSyncedItem> itemMap = new Dictionary<string, XcodeSyncedItem> ();
		Dictionary<string,DateTime> syncTimeCache = new Dictionary<string, DateTime> ();
		
		XcodeProject pendingProjectWrite;
		
		public XcodeMonitor (FilePath projectDir, string name)
		{
			this.originalProjectDir = projectDir;
			this.name = name;
			HackGetNextProjectDir ();
		}
		
		public bool ProjectExists ()
		{
			return Directory.Exists (xcproj);
		}
		
		public void UpdateProject (List<XcodeSyncedItem> allItems, XcodeProject emptyProject)
		{
			items = allItems;
			
			XC4Debug.Log ("Updating synced project with {0} items", items.Count);
			
			var ctx = new XcodeSyncContext (projectDir, syncTimeCache);
			
			var toRemove = new HashSet<string> (itemMap.Keys);
			var toClose = new HashSet<string> ();
			var syncList = new List<XcodeSyncedItem> ();
			bool updateProject = false;
			
			foreach (var item in items) {
				bool needsSync = item.NeedsSyncOut (ctx);
				if (needsSync)
					syncList.Add (item);
				
				var files = item.GetTargetRelativeFileNames ();
				foreach (var f in files) {
					toRemove.Remove (f);
					if (!itemMap.ContainsKey (f)) {
						updateProject = true;
					} else if (needsSync) {
						toClose.Add (f);
					}
					itemMap [f] = item;
				}
			}
			updateProject = updateProject || toRemove.Any ();
			
			bool removedOldProject = false;
			if (updateProject) {
				if (pendingProjectWrite == null && ProjectExists ()) {
					XC4Debug.Log ("Project file needs to be updated, closing and removing old project");
					CloseProject ();
					DeleteProjectArtifacts ();
					removedOldProject = true;
				}
			} else {
				foreach (var f in toClose)
					CloseFile (projectDir.Combine (f));
			}
			
			foreach (var f in toRemove) {
				itemMap.Remove (f);
				syncTimeCache.Remove (f);
				var path = projectDir.Combine (f);
				if (File.Exists (path))
					File.Delete (path);
			}
			
			if (removedOldProject) {
				HackRelocateProject ();
			}
			
			foreach (var item in items) {
				if (updateProject)
					item.AddToProject (emptyProject, projectDir);
			}
			
			foreach (var item in syncList) {
				XC4Debug.Log ("Syncing item {0}", item.GetTargetRelativeFileNames ()[0]);
				item.SyncOut (ctx);
			}
			
			if (updateProject) {
				XC4Debug.Log ("Queuing Xcode project {0} to write when opened", projectDir);
				pendingProjectWrite = emptyProject;
			}
		}
		
		// Xcode keeps some kind of internal lock on project files while it's running and
		// gets extremely unhappy if a new or changed project is in the same location as
		// a previously opened one.
		//
		// To work around this we increment a subdirectory name and use that, and do some
		// careful bookkeeping to reduce the unnecessary I/O overhead that this adds.
		//
		void HackRelocateProject ()
		{
			var oldProjectDir = projectDir;
			HackGetNextProjectDir ();
			XC4Debug.Log ("Relocating {0} to {1}", oldProjectDir, projectDir);
			foreach (var f in syncTimeCache) {
				var target = projectDir.Combine (f.Key);
				var src = oldProjectDir.Combine (f.Key);
				var parent = target.ParentDirectory;
				if (!Directory.Exists (parent))
					Directory.CreateDirectory (parent);
				File.Move (src, target);
			}
		}
		
		void HackGetNextProjectDir ()
		{
			do {
				this.projectDir = originalProjectDir.Combine (nextHackDir.ToString ());
				nextHackDir++;
			} while (Directory.Exists (this.projectDir));
			this.xcproj = projectDir.Combine (name + ".xcodeproj");
		}

		public XcodeSyncBackContext GetChanges (NSObjectInfoService infoService, DotNetProject project)
		{
			var ctx = new XcodeSyncBackContext (projectDir, syncTimeCache, infoService, project);
			foreach (var item in items) {
				if (item.NeedsSyncBack (ctx)) {
					item.SyncBack (ctx);
				}
			}
			return ctx;
		}
		
		public bool CheckRunning ()
		{
			var appPathKey = new NSString ("NSApplicationPath");
			var appPathVal = new NSString (AppleSdkSettings.XcodePath);
			return NSWorkspace.SharedWorkspace.LaunchedApplications.Any (app => appPathVal.Equals (app[appPathKey]));
		}
		
		public void SaveProject ()
		{
			XC4Debug.Log ("Saving Xcode project");
			AppleScript.Run (XCODE_SAVE_IN_PATH, AppleSdkSettings.XcodePath, projectDir);
		}
		
		public void OpenProject ()
		{
			if (pendingProjectWrite != null) {
				pendingProjectWrite.Generate (projectDir);
				pendingProjectWrite = null;
			}
			if (!NSWorkspace.SharedWorkspace.OpenFile (xcproj, AppleSdkSettings.XcodePath))
				throw new Exception ("Failed to open Xcode project");
		}
		
		public void OpenFile (string relativeName)
		{
			XC4Debug.Log ("Opening file in Xcode: {0}", relativeName);
			OpenProject ();
			NSWorkspace.SharedWorkspace.OpenFile (projectDir.Combine (relativeName), AppleSdkSettings.XcodePath);
		}
		
		public void DeleteProjectDirectory ()
		{
			XC4Debug.Log ("Deleting project directory");
			//if (Directory.Exists (projectDir))
			//	Directory.Delete (projectDir, true);
			HackFlushDirs ();
		}
		
		void HackFlushDirs ()
		{
			if (CheckRunning ())
				return;
			if (Directory.Exists (this.originalProjectDir))
				Directory.Delete (this.originalProjectDir, true);
		}
		
		void DeleteProjectArtifacts ()
		{
			XC4Debug.Log ("Deleting project artifacts");
			if (Directory.Exists (xcproj))
				Directory.Delete (xcproj, true);
		}
		
		public bool IsProjectOpen ()
		{
			if (!CheckRunning ())
				return false;
			return AppleScript.Run (XCODE_CHECK_PROJECT_OPEN, AppleSdkSettings.XcodePath, xcproj) == "true";
		}
		
		public bool CloseProject ()
		{
			var success = AppleScript.Run (XCODE_CLOSE_IN_PATH, AppleSdkSettings.XcodePath, projectDir) == "true";
			XC4Debug.Log ("Closing project: {0}", success);
			return success;
		}
		
		public bool CloseFile (string fileName)
		{
			var success = AppleScript.Run (XCODE_CLOSE_IN_PATH, AppleSdkSettings.XcodePath, fileName) == "true";
			XC4Debug.Log ("Closing file {0}: {1}", fileName, success);
			return success;
		}
		
		const string XCODE_SAVE_IN_PATH =
@"tell application ""{0}""
set pp to ""{1}""
	set ext to {{ "".xib"", "".h"", "".m"" }}
	repeat with d in documents
		if d is modified then
			set f to path of d
			if f starts with pp then
				repeat with e in ext
					if f ends with e then
						save d
						exit repeat
					end if
				end repeat
			end if
		end if
	end repeat
end tell";
		
		const string XCODE_CLOSE_IN_PATH =
@"tell application ""{0}""
	set pp to ""{1}""
	repeat with d in documents
		set f to path of d
		if f starts with pp then
			close d
			return true
		end if
	end repeat
	return false
end tell";
		
		const string XCODE_CHECK_PROJECT_OPEN =
@"tell application ""{0}""
	set pp to ""{1}""
	repeat with p in projects
		if real path of p is pp then
			return true
			exit repeat
		end if
	end repeat
	return false
end tell";
	}
}
