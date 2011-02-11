// 
// ReferenceFinder.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
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
using MonoDevelop.Projects.Dom;
using System.Collections.Generic;
using System.Linq;
using Mono.Addins;
using MonoDevelop.Core;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Projects.CodeGeneration;
using MonoDevelop.Projects;

namespace MonoDevelop.Ide.FindInFiles
{
	public abstract class ReferenceFinder
	{
		public bool IncludeDocumentation {
			get;
			set;
		}
		
		static List<ReferenceFinderCodon> referenceFinderCodons = new List<ReferenceFinderCodon> ();
		
		static ReferenceFinder ()
		{
			AddinManager.AddExtensionNodeHandler ("/MonoDevelop/Ide/ReferenceFinder", delegate(object sender, ExtensionNodeEventArgs args) {
				var codon = (ReferenceFinderCodon)args.ExtensionNode;
				switch (args.Change) {
				case ExtensionChange.Add:
					referenceFinderCodons.Add (codon);
					break;
				case ExtensionChange.Remove:
					referenceFinderCodons.Remove (codon);
					break;
				}
			});
		}
		
		public static ReferenceFinder GetReferenceFinder (string mimeType)
		{
			var codon = referenceFinderCodons.FirstOrDefault (c => c.SupportedMimeTypes.Any (mt => mt == mimeType));
			return codon != null ? codon.ReferenceFinder : null;
		}
		
		public abstract IEnumerable<MemberReference> FindReferences (ProjectDom dom, FilePath fileName, INode member);
		
		
		public static IEnumerable<MemberReference> FindReferences (INode member)
		{
			return FindReferences (IdeApp.ProjectOperations.CurrentSelectedSolution, member);
		}
			
		public static IEnumerable<MemberReference> FindReferences (Solution solution, INode member)
		{
			var scope = GetScope (member);
			ReferenceFinder finder;
			ProjectDom dom = null;
			ICompilationUnit unit = null;
			IEnumerable<INode> searchNodes = new INode[] { member };
			 if (member is LocalVariable) {
				dom = ((LocalVariable)member).DeclaringMember.DeclaringType.SourceProjectDom;
				unit = ((LocalVariable)member).CompilationUnit;
			} else if (member is IParameter) {
				dom = ((IParameter)member).DeclaringMember.DeclaringType.SourceProjectDom;
				unit = ((IParameter)member).DeclaringMember.DeclaringType.CompilationUnit;
			} else if (member is IType) {
				dom = ((IType)member).SourceProjectDom;
				unit = ((IType)member).CompilationUnit;
			} else if (member is IMember) {
				dom = ((IMember)member).DeclaringType.SourceProjectDom;
				unit = ((IMember)member).DeclaringType.CompilationUnit;
				searchNodes = CollectMembers (dom, (IMember)member);
			}
			
			switch (scope) {
			case RefactoryScope.File:
			case RefactoryScope.DeclaringType:
				if (dom == null || unit == null)
					yield break;
				finder = GetReferenceFinder (DesktopService.GetMimeTypeForUri (unit.FileName));
				if (finder == null)
					yield break;
				foreach (var searchNode in searchNodes) {
					foreach (var foundReference in finder.FindReferences (dom, unit.FileName, searchNode)) {
						yield return foundReference;
					}
				}
				break;
			case RefactoryScope.Project:
				if (dom == null)
					yield break;
				foreach (var file in dom.Project.Files) {
					finder = GetReferenceFinder (DesktopService.GetMimeTypeForUri (file.FilePath));
					if (finder == null)
						continue;
					foreach (var searchNode in searchNodes) {
						foreach (var foundReference in finder.FindReferences (dom, file.FilePath, searchNode)) {
							yield return foundReference;
						}
					}
				}
				break;
				
			case RefactoryScope.Solution:
				foreach (var project in solution.GetAllProjects ()) {
					var currentDom = ProjectDomService.GetProjectDom (project);
					foreach (var file in project.Files) {
						finder = GetReferenceFinder (DesktopService.GetMimeTypeForUri (file.FilePath));
						if (finder == null)
							continue;
						foreach (var searchNode in searchNodes) {
							foreach (var foundReference in finder.FindReferences (currentDom, file.FilePath, searchNode)) {
								yield return foundReference;
							}
						}
					}
				}
				break;
			}
		}
		
		internal static IEnumerable<INode> CollectMembers (ProjectDom dom, IMember member)
		{
			if (member is IMethod && ((IMethod)member).IsConstructor) {
				yield return member;
			} else {
				bool isOverrideable = member.DeclaringType.ClassType == ClassType.Interface || member.IsOverride || member.IsVirtual || member.IsAbstract;
				bool isLastMember = false;
				// for members we need to collect the whole 'class' of members (overloads & implementing types)
				HashSet<string> alreadyVisitedTypes = new HashSet<string> ();
				foreach (IType type in dom.GetInheritanceTree (member.DeclaringType)) {
					if (type.ClassType == ClassType.Interface || isOverrideable || type.DecoratedFullName == member.DeclaringType.DecoratedFullName) {
						// search in the class for the member
						foreach (IMember interfaceMember in type.SearchMember (member.Name, true)) {
							if (interfaceMember.MemberType == member.MemberType)
								yield return interfaceMember;
						}
						
						// now search in all subclasses of this class for the member
						isLastMember = !member.IsOverride;
						foreach (IType implementingType in dom.GetSubclasses (type)) {
							string name = implementingType.DecoratedFullName;
							if (alreadyVisitedTypes.Contains (name))
								continue;
							alreadyVisitedTypes.Add (name);
							foreach (IMember typeMember in implementingType.SearchMember (member.Name, true)) {
								if (typeMember.MemberType == member.MemberType) {
									isLastMember = type.ClassType != ClassType.Interface && (typeMember.IsVirtual || typeMember.IsAbstract || !typeMember.IsOverride);
									yield return typeMember;
								}
							}
							if (!isOverrideable)
								break;
						}
						if (isLastMember)
							break;
					}
				}
			}
		}
		
		static RefactoryScope GetScope (INode node)
		{
			IMember member = node as IMember;
			if (member == null)
				return RefactoryScope.DeclaringType;
			
			if (member.DeclaringType != null && member.DeclaringType.ClassType == ClassType.Interface)
				return GetScope (member.DeclaringType);
			
			if (member.IsPublic)
				return RefactoryScope.Solution;
			
			if (member.IsProtected || member.IsInternal || member.DeclaringType == null)
				return RefactoryScope.Project;
			return RefactoryScope.DeclaringType;
		}
	}
	
	[ExtensionNode (Description="A reference finder. The specified class needs to inherit from MonoDevelop.Projects.CodeGeneration.ReferenceFinder")]
	internal class ReferenceFinderCodon : TypeExtensionNode
	{
		[NodeAttribute("supportedmimetypes", "Mime types supported by this binding (to be shown in the Open File dialog)")]
		string[] supportedMimetypes;
		
		public string[] SupportedMimeTypes {
			get {
				return supportedMimetypes;
			}
			set {
				supportedMimetypes = value;
			}
		}
		
		public ReferenceFinder ReferenceFinder {
			get {
				return (ReferenceFinder)GetInstance ();
			}
		}
		
		public override string ToString ()
		{
			return string.Format ("[ReferenceFinderCodon: SupportedMimeTypes={0}, ReferenceFinder={1}]", SupportedMimeTypes.Count () + ":" + String.Join (";", SupportedMimeTypes), ReferenceFinder);
		}
	}
}
