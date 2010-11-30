//
// FileService.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Diagnostics;
using System.IO;
using System.Text;

using Mono.Addins;
using MonoDevelop.Core.FileSystem;
using System.Collections.Generic;
using System.Threading;

namespace MonoDevelop.Core
{
	public static class FileService
	{
		const string addinFileSystemExtensionPath = "/MonoDevelop/Core/FileSystemExtensions";
		readonly static char[] separators = { Path.DirectorySeparatorChar, Path.VolumeSeparatorChar, Path.AltDirectorySeparatorChar };
		
		static FileServiceErrorHandler errorHandler;
		
		static FileSystemExtension fileSystemChain;
		static FileSystemExtension defaultExtension = new DefaultFileSystemExtension ();
		
		static EventQueue eventQueue = new EventQueue ();
		
		static string applicationRootPath = Path.Combine (PropertyService.EntryAssemblyPath, "..");
		public static string ApplicationRootPath {
			get {
				return applicationRootPath;
			}
		}
		
		static FileService()
		{
			AddinManager.ExtensionChanged += delegate (object sender, ExtensionEventArgs args) {
				if (args.PathChanged (addinFileSystemExtensionPath))
					UpdateExtensions ();
			};
		}
		
		static void UpdateExtensions ()
		{
			if (!Runtime.Initialized) {
				fileSystemChain = defaultExtension;
				return;
			}
			
			FileSystemExtension[] extensions = (FileSystemExtension[]) AddinManager.GetExtensionObjects (addinFileSystemExtensionPath, typeof(FileSystemExtension));
			for (int n=0; n<extensions.Length - 1; n++) {
				extensions [n].Next = extensions [n + 1];
			}

			if (extensions.Length > 0) {
				extensions [extensions.Length - 1].Next = defaultExtension;
				fileSystemChain = extensions [0];
			} else {
				fileSystemChain = defaultExtension;
			}
		}
		
		public static void DeleteFile (string fileName)
		{
			Debug.Assert (!String.IsNullOrEmpty (fileName));
			try {
				GetFileSystemForPath (fileName, false).DeleteFile (fileName);
			} catch (Exception e) {
				if (!HandleError (GettextCatalog.GetString ("Can't remove file {0}", fileName), e))
					throw;
				return;
			}
			OnFileRemoved (new FileEventInfo (fileName, false));
		}
		
		public static void DeleteDirectory (string path)
		{
			Debug.Assert (!String.IsNullOrEmpty (path));
			try {
				GetFileSystemForPath (path, true).DeleteDirectory (path);
			} catch (Exception e) {
				if (!HandleError (GettextCatalog.GetString ("Can't remove directory {0}", path), e))
					throw;
				return;
			}
			OnFileRemoved (new FileEventInfo (path, true));
		}
		
		public static void RenameFile (string oldName, string newName)
		{
			Debug.Assert (!String.IsNullOrEmpty (oldName));
			Debug.Assert (!String.IsNullOrEmpty (newName));
			if (Path.GetFileName (oldName) != newName) {
				string newPath = Path.Combine (Path.GetDirectoryName (oldName), newName);
				InternalMoveFile (oldName, newPath);
				OnFileRenamed (new FileCopyEventInfo (oldName, newPath, false));
				OnFileCreated (new FileEventInfo (newPath, false));
				OnFileRemoved (new FileEventInfo (oldName, false));
			}
		}
		
		public static void RenameDirectory (string oldName, string newName)
		{
			Debug.Assert (!String.IsNullOrEmpty (oldName));
			Debug.Assert (!String.IsNullOrEmpty (newName));
			if (Path.GetFileName (oldName) != newName) {
				string newPath = Path.Combine (Path.GetDirectoryName (oldName), newName);
				InternalMoveDirectory (oldName, newPath);
				OnFileRenamed (new FileCopyEventInfo (oldName, newPath, true));
				OnFileCreated (new FileEventInfo (newPath, false));
				OnFileRemoved (new FileEventInfo (oldName, false));
			}
		}
		
		public static void CopyFile (string srcFile, string dstFile)
		{
			Debug.Assert (!String.IsNullOrEmpty (srcFile));
			Debug.Assert (!String.IsNullOrEmpty (dstFile));
			GetFileSystemForPath (dstFile, false).CopyFile (srcFile, dstFile, true);
			OnFileCreated (new FileEventInfo (dstFile, false));
		}

		public static void MoveFile (string srcFile, string dstFile)
		{
			Debug.Assert (!String.IsNullOrEmpty (srcFile));
			Debug.Assert (!String.IsNullOrEmpty (dstFile));
			InternalMoveFile (srcFile, dstFile);
			OnFileCreated (new FileEventInfo (dstFile, false));
			OnFileRemoved (new FileEventInfo (srcFile, false));
		}
		
		static void InternalMoveFile (string srcFile, string dstFile)
		{
			Debug.Assert (!String.IsNullOrEmpty (srcFile));
			Debug.Assert (!String.IsNullOrEmpty (dstFile));
			FileSystemExtension srcExt = GetFileSystemForPath (srcFile, false);
			FileSystemExtension dstExt = GetFileSystemForPath (dstFile, false);
			
			if (srcExt == dstExt) {
				// Everything can be handled by the same file system
				srcExt.MoveFile (srcFile, dstFile);
			} else {
				// If the file system of the source and dest files are
				// different, decompose the Move operation into a Copy
				// and Delete, so every file system can handle its part
				dstExt.CopyFile (srcFile, dstFile, true);
				srcExt.DeleteFile (srcFile);
			}
		}
		
		public static void CreateDirectory (string path)
		{
			Debug.Assert (!String.IsNullOrEmpty (path));
			GetFileSystemForPath (path, true).CreateDirectory (path);
			OnFileCreated (new FileEventInfo (path, true));
		}
		
		public static void CopyDirectory (string srcPath, string dstPath)
		{
			Debug.Assert (!String.IsNullOrEmpty (srcPath));
			Debug.Assert (!String.IsNullOrEmpty (dstPath));
			GetFileSystemForPath (dstPath, true).CopyDirectory (srcPath, dstPath);
		}
		
		public static void MoveDirectory (string srcPath, string dstPath)
		{
			Debug.Assert (!String.IsNullOrEmpty (srcPath));
			Debug.Assert (!String.IsNullOrEmpty (dstPath));
			InternalMoveDirectory (srcPath, dstPath);
			OnFileCreated (new FileEventInfo (dstPath, true));
			OnFileRemoved (new FileEventInfo (srcPath, true));
		}
		
		static void InternalMoveDirectory (string srcPath, string dstPath)
		{
			Debug.Assert (!String.IsNullOrEmpty (srcPath));
			Debug.Assert (!String.IsNullOrEmpty (dstPath));
			FileSystemExtension srcExt = GetFileSystemForPath (srcPath, true);
			FileSystemExtension dstExt = GetFileSystemForPath (dstPath, true);
			
			if (srcExt == dstExt) {
				// Everything can be handled by the same file system
				srcExt.MoveDirectory (srcPath, dstPath);
			} else {
				// If the file system of the source and dest files are
				// different, decompose the Move operation into a Copy
				// and Delete, so every file system can handle its part
				dstExt.CopyDirectory (srcPath, dstPath);
				srcExt.DeleteDirectory (srcPath);
			}
		}
		
		public static bool RequestFileEdit (string fileName)
		{
			Debug.Assert (!String.IsNullOrEmpty (fileName));
			return GetFileSystemForPath (fileName, false).RequestFileEdit (fileName);
		}
		
		public static void NotifyFileChanged (string fileName)
		{
			Debug.Assert (!String.IsNullOrEmpty (fileName));
			try {
				GetFileSystemForPath (fileName, false).NotifyFileChanged (fileName);
				OnFileChanged (new FileEventInfo (fileName, false));
			} catch (Exception ex) {
				LoggingService.LogError ("File change notification failed", ex);
			}
		}
		
		public static void NotifyFileRemoved (string fileName)
		{
			Debug.Assert (!String.IsNullOrEmpty (fileName));
			try {
				OnFileRemoved (new FileEventInfo (fileName, false));
			} catch (Exception ex) {
				LoggingService.LogError ("File remove notification failed", ex);
			}
		}
		
		internal static FileSystemExtension GetFileSystemForPath (string path, bool isDirectory)
		{
			Debug.Assert (!String.IsNullOrEmpty (path));
			if (fileSystemChain == null)
				UpdateExtensions ();
			FileSystemExtension nx = fileSystemChain;
			while (nx != null && !nx.CanHandlePath (path, isDirectory))
				nx = nx.Next;
			return nx;
		}
		
		public static string AbsoluteToRelativePath (string baseDirectoryPath, string absPath)
		{
			if (!Path.IsPathRooted (absPath))
				return absPath;
			
			absPath           = Path.GetFullPath (absPath);
			baseDirectoryPath = Path.GetFullPath (baseDirectoryPath.TrimEnd (Path.DirectorySeparatorChar));
			
			string[] bPath = baseDirectoryPath.Split (separators);
			string[] aPath = absPath.Split (separators);
			int indx = 0;
			
			for (; indx < Math.Min (bPath.Length, aPath.Length); indx++) {
				if (!bPath[indx].Equals(aPath[indx]))
					break;
			}
			
			if (indx == 0) 
				return absPath;
			
			StringBuilder result = new StringBuilder ();
			
			for (int i = indx; i < bPath.Length; i++) {
				result.Append ("..");
				if (i + 1 < bPath.Length || aPath.Length - indx > 0)
					result.Append (Path.DirectorySeparatorChar);
			}
			
			
			result.Append (String.Join(Path.DirectorySeparatorChar.ToString(), aPath, indx, aPath.Length - indx));
			if (result.Length == 0)
				return ".";
			return result.ToString ();
		}
		
		public static string RelativeToAbsolutePath (string baseDirectoryPath, string relPath)
		{
			return Path.GetFullPath (Path.Combine (baseDirectoryPath, relPath));
		}
		
		public static bool IsValidPath (string fileName)
		{
			if (String.IsNullOrEmpty (fileName) || fileName.Trim() == string.Empty) 
				return false;
			if (fileName.IndexOfAny (Path.GetInvalidPathChars ()) >= 0)
				return false;
			return true;
		}

		public static bool IsValidFileName (string fileName)
		{
			if (String.IsNullOrEmpty (fileName) || fileName.Trim() == string.Empty) 
				return false;
			if (fileName.IndexOfAny (Path.GetInvalidFileNameChars ()) >= 0)
				return false;
			return true;
		}
		
		public static bool IsDirectory (string filename)
		{
			return Directory.Exists (filename) && (File.GetAttributes (filename) & FileAttributes.Directory) != 0;
		}
		
		public static string GetFullPath (string path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			// Note: It's not required for Path.GetFullPath (path) that path exists.
			return Path.GetFullPath (path); 
		}
		
		public static string CreateTempDirectory ()
		{
			Random rnd = new Random ();
			string result;
			while (true) {
				result = Path.Combine (Path.GetTempPath (), "mdTmpDir" + rnd.Next ());
				if (!Directory.Exists (result))
					break;
			} 
			Directory.CreateDirectory (result);
			return result;
		}

		public static string NormalizeRelativePath (string path)
		{
			string result = path.Trim (Path.DirectorySeparatorChar, ' ');
			while (result.StartsWith ("." + Path.DirectorySeparatorChar)) {
				result = result.Substring (2);
				result = result.Trim (Path.DirectorySeparatorChar);
			}
			return result == "." ? "" : result;
		}

		// Atomic rename of a file. It does not fire events.
		public static void SystemRename (string sourceFile, string destFile)
		{
			if (PropertyService.IsWindows) {
				string wtmp = null;
				if (File.Exists (destFile)) {
					do {
						wtmp = Path.Combine (Path.GetTempPath (), Guid.NewGuid ().ToString ());
					} while (File.Exists (wtmp));

					File.Move (destFile, wtmp);
				}
				try {
					File.Move (sourceFile, destFile);
				}
				catch {
					try {
						if (wtmp != null)
							File.Move (wtmp, destFile);
					}
					catch {
						wtmp = null;
					}
					throw;
				}
				finally {
					if (wtmp != null) {
						try {
							File.Delete (wtmp);
						}
						catch { }
					}
				}
			}
			else {
				Mono.Unix.Native.Syscall.rename (sourceFile, destFile);
			}
		}

		static bool HandleError (string message, Exception ex)
		{
			return errorHandler != null ? errorHandler (message, ex) : false;
		}
		
		public static FileServiceErrorHandler ErrorHandler {
			get { return errorHandler; }
			set { errorHandler = value; }
		}
		
		public static void FreezeEvents ()
		{
			eventQueue.Freeze ();
		}
		
		public static void ThawEvents ()
		{
			eventQueue.Thaw ();
		}

		public static event EventHandler<FileEventArgs> FileCreated;
		static void OnFileCreated (FileEventInfo args)
		{
			if (args.IsDirectory)
				Counters.DirectoriesCreated++;
			else
				Counters.FilesCreated++;

			eventQueue.Add<FileEventArgs> (FileCreated, null, args);
		}
		
		public static event EventHandler<FileCopyEventArgs> FileRenamed;
		static void OnFileRenamed (FileCopyEventInfo args)
		{
			if (args.IsDirectory)
				Counters.DirectoriesRenamed++;
			else
				Counters.FilesRenamed++;
			
			eventQueue.Add<FileCopyEventArgs> (FileRenamed, null, args);
		}
		
		public static event EventHandler<FileEventArgs> FileRemoved;
		static void OnFileRemoved (FileEventInfo args)
		{
			if (args.IsDirectory)
				Counters.DirectoriesRemoved++;
			else
				Counters.FilesRemoved++;
			
			eventQueue.Add<FileEventArgs> (FileRemoved, null, args);
		}
		
		public static event EventHandler<FileEventArgs> FileChanged;
		static void OnFileChanged (FileEventInfo args)
		{
			Counters.FileChangeNotifications++;
			eventQueue.Add<FileEventArgs> (FileChanged, null, args);
		}
	}
	
	class EventQueue
	{
		class EventData
		{
			public Delegate Delegate;
			public object ThisObject;
			public IEventArgsChain Args;
		}
		
		List<EventData> events = new List<EventData> ();
		
		int frozen;
		
		public void Freeze ()
		{
			lock (events) {
				frozen++;
			}
		}
		
		public void Thaw ()
		{
			List<EventData> pendingEvents = null;
			lock (events) {
				if (--frozen == 0) {
					pendingEvents = events;
					events = new List<EventData> ();
				}
			}
			if (pendingEvents != null) {
				foreach (EventData ed in pendingEvents) {
					ed.Delegate.DynamicInvoke (ed.ThisObject, ed.Args);
				}
			}
		}
		
		public void Add (Delegate d, object thisObj, object args)
		{
			lock (events) {
				if (frozen > 0) {
					EventData lastEvent = events.Count > 0 ? events [events.Count - 1] : null;
					if (lastEvent != null && lastEvent.Delegate == d && lastEvent.ThisObject == thisObj)
						lastEvent.Args.Add (args);
					else {
						EventData ed = new EventData ();
						ed.Delegate = d;
						ed.ThisObject = thisObj;
						Type t = typeof(EventArgsChain<>).MakeGenericType (args.GetType ());
						ed.Args = (IEventArgsChain) Activator.CreateInstance (t);
						ed.Args.Add (args);
						events.Add (ed);
					}
					return;
				}
			}
			Type ct = typeof(EventArgsChain<>).MakeGenericType (args.GetType ());
			IEventArgsChain argsChain = (IEventArgsChain) Activator.CreateInstance (ct);
			d.DynamicInvoke (thisObj, argsChain);
		}

		public void Add<TC> (Delegate d, object thisObj, object args) where TC:IEventArgsChain, new()
		{
			lock (events) {
				if (frozen > 0) {
					EventData lastEvent = events.Count > 0 ? events [events.Count - 1] : null;
					if (lastEvent != null && lastEvent.Delegate == d && lastEvent.ThisObject == thisObj)
						lastEvent.Args.Add (args);
					else {
						EventData ed = new EventData ();
						ed.Delegate = d;
						ed.ThisObject = thisObj;
						ed.Args = new TC ();
						ed.Args.Add (args);
						events.Add (ed);
					}
					return;
				}
			}
			IEventArgsChain argsChain = new TC ();
			argsChain.Add (args);
			d.DynamicInvoke (thisObj, argsChain);
		}
	}
	
	[Serializable]
	public struct FilePath: IComparable<FilePath>, IComparable, IEquatable<FilePath>
	{
		string fileName;

		public static readonly FilePath Null = new FilePath (null);
		public static readonly FilePath Empty = new FilePath (string.Empty);

		public FilePath (string name)
		{
			fileName = name;
		}

		public bool IsNull {
			get { return fileName == null; }
		}

		public bool IsNullOrEmpty {
			get { return string.IsNullOrEmpty (fileName); }
		}

		public bool IsEmpty {
			get { return fileName != null && fileName.Length == 0; }
		}

		public FilePath FullPath {
			get {
				return new FilePath (!string.IsNullOrEmpty (fileName) ? Path.GetFullPath (fileName) : "");
			}
		}
		
		/// <summary>
		/// Returns a path in standard form, which can be used to be compared
		/// for equality with other canonical paths. It is similar to FullPath,
		/// but unlike FullPath, the directory "/a/b" is considered equal to "/a/b/"
		/// </summary>
		public FilePath CanonicalPath {
			get {
				string fp = Path.GetFullPath (fileName);
				if (fp.Length > 0 && fp[fp.Length - 1] == Path.DirectorySeparatorChar)
					return fp.TrimEnd (Path.DirectorySeparatorChar);
				else
					return fp;
			}
		}

		public string FileName {
			get {
				return Path.GetFileName (fileName);
			}
		}

		public string Extension {
			get {
				return Path.GetExtension (fileName);
			}
		}

		public string FileNameWithoutExtension {
			get {
				return Path.GetFileNameWithoutExtension (fileName);
			}
		}

		public FilePath ParentDirectory {
			get {
				return new FilePath (Path.GetDirectoryName (fileName));
			}
		}

		public bool IsAbsolute {
			get { return Path.IsPathRooted (fileName); }
		}

		public bool IsChildPathOf (FilePath basePath)
		{
			StringComparison sc = PropertyService.IsWindows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			if (basePath.fileName [basePath.fileName.Length - 1] != Path.DirectorySeparatorChar)
				return fileName.StartsWith (basePath.fileName + Path.DirectorySeparatorChar, sc);
			else
				return fileName.StartsWith (basePath.fileName, sc);
		}

		public FilePath ChangeExtension (string ext)
		{
			return Path.ChangeExtension (fileName, ext);
		}

		public FilePath Combine (params FilePath[] paths)
		{
			string path = fileName;
			foreach (FilePath p in paths)
				path = Path.Combine (path, p.fileName);
			return new FilePath (path);
		}

		public FilePath Combine (params string[] paths)
		{
			string path = fileName;
			foreach (string p in paths)
				path = Path.Combine (path, p);
			return new FilePath (path);
		}
		
		/// <summary>
		/// Builds a path by combining all provided path sections
		/// </summary>
		public static FilePath Build (params string[] paths)
		{
			return Empty.Combine (paths);
		}

		public FilePath ToAbsolute (FilePath basePath)
		{
			if (IsAbsolute)
				return FullPath;
			else
				return Combine (basePath, this).FullPath;
		}

		public FilePath ToRelative (FilePath basePath)
		{
			return FileService.AbsoluteToRelativePath (basePath, fileName);
		}

		public static implicit operator FilePath (string name)
		{
			return new FilePath (name);
		}

		public static implicit operator string (FilePath filePath)
		{
			return filePath.fileName;
		}

		public static bool operator == (FilePath name1, FilePath name2)
		{
			if (PropertyService.IsWindows)
				return string.Equals (name1.fileName, name2.fileName, StringComparison.OrdinalIgnoreCase);
			else
				return string.Equals (name1.fileName, name2.fileName, StringComparison.Ordinal);
		}

		public static bool operator != (FilePath name1, FilePath name2)
		{
			return !(name1 == name2);
		}

		public override bool Equals (object obj)
		{
			if (obj == null && !(obj is FilePath))
				return false;

			FilePath fn = (FilePath) obj;
			return this == fn;
		}

		public override int GetHashCode ( )
		{
			if (fileName == null)
				return 0;
			if (PropertyService.IsWindows)
				return fileName.ToLower ().GetHashCode ();
			else
				return fileName.GetHashCode ();
		}

		public override string ToString ( )
		{
			return fileName;
		}

		public int CompareTo (FilePath filePath)
		{
			return string.Compare (fileName, filePath.fileName, PropertyService.IsWindows);
		}

		int IComparable.CompareTo (object obj)
		{
			if (!(obj is FilePath))
				return -1;
			return CompareTo ((FilePath) obj);
		}

		#region IEquatable<FilePath> Members

		bool IEquatable<FilePath>.Equals (FilePath other)
		{
			return this == other;
		}

		#endregion
	}

	public static class FilePathUtil
	{
		public static string[] ToStringArray (this FilePath[] paths)
		{
			string[] array = new string[paths.Length];
			for (int n = 0; n < paths.Length; n++)
				array[n] = paths[n].ToString ();
			return array;
		}
		
		public static FilePath[] ToFilePathArray (this string[] paths)
		{
			var array = new FilePath[paths.Length];
			for (int n = 0; n < paths.Length; n++)
				array[n] = paths[n];
			return array;
		}
	}
	
	public delegate bool FileServiceErrorHandler (string message, Exception ex);
}
