//
// CodeRefactorer.cs
//
// Author:
//   Lluis Sanchez Gual
//
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
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using MonoDevelop.Core;
using MonoDevelop.Projects.Text;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Projects.Dom.Output;

namespace MonoDevelop.Projects.CodeGeneration
{
	public class CodeRefactorer
	{
		Solution solution;
		ITextFileProvider fileProvider;
		
		delegate void RefactorDelegate (IProgressMonitor monitor, RefactorerContext gctx, IRefactorer gen, string file);
		
		public CodeRefactorer (Solution solution)
		{
			this.solution = solution;
		}
		
		public CodeRefactorer ()
		{
		}
		
		public ITextFileProvider TextFileProvider {
			get { return fileProvider; }
			set { fileProvider = value; } 
		}
		
		public bool ClassSupportsOperation (IType cls, RefactorOperations operation)
		{
			IRefactorer r = GetGeneratorForClass (cls);
			if (r == null)
				return false;
			return (r.SupportedOperations & operation) == operation;
		}
		
		public bool FileSupportsOperation (string file, RefactorOperations operation)
		{
			IRefactorer r = LanguageBindingService.GetRefactorerForFile (file);
			if (r == null)
				return false;
			return (r.SupportedOperations & operation) == operation;
		}
		
		public bool LanguageSupportsOperation (string langName, RefactorOperations operation)
		{
			IRefactorer r = LanguageBindingService.GetRefactorerForLanguage (langName);
			if (r == null)
				return false;
			return (r.SupportedOperations & operation) == operation;
		}
		
		public void AddAttribute (IType cls, string name, params object[] parameters)
		{
			CodeAttributeArgument[] args = new CodeAttributeArgument[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
				args [i] = new CodeAttributeArgument (new CodePrimitiveExpression (parameters [i]));
			CodeAttributeDeclaration attr = new CodeAttributeDeclaration (name, args);
			AddAttribute (cls, attr);
		}

		public void AddAttribute (IType cls, CodeAttributeDeclaration attr)
		{
			RefactorerContext gctx = GetGeneratorContext (cls);
			IRefactorer gen = GetGeneratorForClass (cls);
			gen.AddAttribute (gctx, cls, attr);
			gctx.Save ();
		}
		
		public IType CreateClass (Project project, string language, string directory, string namspace, CodeTypeDeclaration type)
		{
			ProjectDom ctx = ProjectDomService.GetProjectDom (project);
			RefactorerContext gctx = new RefactorerContext (ctx, fileProvider, null);
			IRefactorer gen = LanguageBindingService.GetRefactorerForLanguage (language);
			IType c = gen.CreateClass (gctx, directory, namspace, type);
			gctx.Save ();
			return c;
		}
		
		public IType RenameClass (IProgressMonitor monitor, IType cls, string newName, RefactoryScope scope)
		{
			try {
				MemberReferenceCollection refs = new MemberReferenceCollection ();
				Refactor (monitor, cls, scope, new RefactorDelegate (new RefactorFindClassReferences (cls, refs, false).Refactor));
				refs.RenameAll (newName);
				
				RefactorerContext gctx = GetGeneratorContext (cls);
				IRefactorer r = GetGeneratorForClass (cls);
				
				foreach (IMethod method in cls.Methods) {
					if (method.IsConstructor)
						r.RenameMember (gctx, cls, (IMember) method, newName);
				}
				
				cls = r.RenameClass (gctx, cls, newName);
				
				gctx.Save ();
				
				return cls;
			} catch (Exception e) {
				LoggingService.LogError (GettextCatalog.GetString ("Error while renaming {0} to {1}: {2}",  cls, newName, e.ToString ()));
				return null;
			}
		}
		
		public MemberReferenceCollection FindClassReferences (IProgressMonitor monitor, IType cls, RefactoryScope scope, bool includeXmlComment)
		{
			MemberReferenceCollection refs = new MemberReferenceCollection ();
			Refactor (monitor, cls, scope, new RefactorDelegate (new RefactorFindClassReferences (cls, refs, includeXmlComment).Refactor));
			return refs;
		}
		
		public MemberReferenceCollection FindVariableReferences (IProgressMonitor monitor, LocalVariable var)
		{
			MemberReferenceCollection refs = new MemberReferenceCollection ();
			Refactor (monitor, var, new RefactorDelegate (new RefactorFindVariableReferences (var, refs).Refactor));
			return refs;
		}
		
		public MemberReferenceCollection FindParameterReferences (IProgressMonitor monitor, IParameter param, bool includeXmlComment)
		{
			MemberReferenceCollection refs = new MemberReferenceCollection ();
			Refactor (monitor, param, new RefactorDelegate (new RefactorFindParameterReferences (param, refs, includeXmlComment).Refactor));
			return refs;
		}
		
		public void FindOverridables (IType cls, List<IMember> classMembers, List<IMember> interfaceMembers,
		                              bool includeOverridenClassMembers, bool includeOverridenInterfaceMembers)
		{
			ProjectDom pctx = GetParserContext (cls);
			List<IType> visited = new List<IType> ();

			cls = pctx.ResolveType (cls);
			FindOverridables (pctx, cls, cls, classMembers, interfaceMembers, visited, includeOverridenClassMembers, includeOverridenInterfaceMembers);
		}

		void FindOverridables (ProjectDom pctx, IType motherClass, IType cls, List<IMember> classMembers, List<IMember> interfaceMembers, List<IType> visited, bool includeOverridenClassMembers, bool includeOverridenInterfaceMembers)
		{
			if (visited.Contains (cls))
				return;
			
			visited.Add (cls);
			
			foreach (IReturnType rt in cls.BaseTypes)
			{
				IType baseCls = pctx.GetType (rt);
				
				if (baseCls == null)
					continue;

				if (visited.Contains (baseCls))
					continue;

				bool isInterface = baseCls.ClassType == ClassType.Interface;
				if (isInterface && interfaceMembers == null)
					continue;
				List<IMember> list = isInterface ? interfaceMembers : classMembers;
				bool includeOverriden = isInterface ? includeOverridenInterfaceMembers : includeOverridenClassMembers;

				foreach (IMethod m in baseCls.Methods) {
					if (m.IsInternal && motherClass.SourceProject != null && motherClass.SourceProject != m.DeclaringType.SourceProject)
						continue;
					if ((isInterface || m.IsVirtual || m.IsAbstract) && !m.IsSpecialName && !m.IsSealed && (includeOverriden || !IsOverridenMethod (motherClass, m)))
						list.Add (m);
				}
				foreach (IProperty m in baseCls.Properties) {
					if (m.IsIndexer)
						continue;
					if (m.IsInternal && motherClass.SourceProject != null && motherClass.SourceProject != m.DeclaringType.SourceProject)
						continue;
					if ((isInterface || m.IsVirtual || m.IsAbstract) && !m.IsSpecialName && !m.IsSealed && (includeOverriden || !IsOverridenProperty (motherClass, m)))
						list.Add (m);
				}
				foreach (IProperty m in baseCls.Properties) {
					if (!m.IsIndexer)
						continue;
					if (m.IsInternal && motherClass.SourceProject != null && motherClass.SourceProject != m.DeclaringType.SourceProject)
						continue;
					if ((isInterface || m.IsVirtual || m.IsAbstract) && !m.IsSpecialName && !m.IsSealed && (includeOverriden || !IsOverridenIndexer (motherClass, m)))
						list.Add (m);
				}
				foreach (IEvent m in baseCls.Events) {
					if (m.IsInternal && motherClass.SourceProject != null && motherClass.SourceProject != m.DeclaringType.SourceProject)
						continue;
					if ((isInterface || m.IsVirtual || m.IsAbstract) && !m.IsSpecialName && !m.IsSealed)
						list.Add (m);
				}

				FindOverridables (pctx, motherClass, baseCls, classMembers, isInterface ? interfaceMembers : null, visited,
				                  includeOverridenClassMembers, includeOverridenInterfaceMembers);
			}
		}

		bool IsOverridenMethod (IType cls, IMethod method)
		{
			foreach (IMethod m in cls.Methods) {
				if (method.Name == m.Name && IsEqual (method.Parameters, m.Parameters))
					return true;
			}
			return false;
		}

		bool IsOverridenProperty (IType cls, IProperty prop)
		{
			foreach (IProperty p in cls.Properties) {
				if (p.IsIndexer)
					continue;
				if (prop.Name == p.Name)
					return true;
			}
			return false;
		}

		bool IsOverridenIndexer (IType cls, IProperty idx)
		{
			foreach (IProperty i in cls.Properties) {
				if (!i.IsIndexer)
					continue;
				if (idx.Name == i.Name && IsEqual (idx.Parameters, i.Parameters))
					return true;
			}
			return false;
		}

		bool IsEqual (IList<IParameter> c1, IList<IParameter> c2)
		{
			if (c1.Count != c2.Count)
				return false;
			for (int i = 0; i < c1.Count; i++) {
				if (c1[i].ReturnType.FullName != c2[i].ReturnType.FullName)
					return false;
			}
			return true;
		}

//		public IMember AddMember (IType cls, CodeTypeMember member)
//		{
//			RefactorerContext gctx = GetGeneratorContext (cls);
//			IRefactorer gen = GetGeneratorForClass (cls);
//			IMember m = gen.AddMember (gctx, cls, member);
//			gctx.Save ();
//			return m;
//		}
//		
//		public IType AddMembers (IType cls, IEnumerable<CodeTypeMember> members, string foldingRegionName)
//		{
//			RefactorerContext gctx = GetGeneratorContext (cls);
//			IRefactorer gen = GetGeneratorForClass (cls);
//			gen.AddMembers (gctx, cls, members, foldingRegionName);
//			gctx.Save ();
//			return GetUpdatedClass (gctx, cls);
//		}
		
//		public IMember ImplementMember (IType cls, IMember member, IReturnType privateReturnType)
//		{
//			RefactorerContext gctx = GetGeneratorContext (cls);
//			IRefactorer gen = GetGeneratorForClass (cls);
//			IMember m = gen.ImplementMember (gctx, cls, member, privateReturnType);
//			gctx.Save ();
//			return m;
//		}
		
		public void AddGlobalNamespaceImport (ProjectDom dom, string fileName, string nsName)
		{
			IRefactorer refactorer = LanguageBindingService.GetRefactorerForFile (fileName);
			refactorer.AddGlobalNamespaceImport (new RefactorerContext (dom, fileProvider, null), fileName, nsName);
		}

		public void AddLocalNamespaceImport (ProjectDom dom, string fileName, string nsName, DomLocation caretLocation)
		{
			IRefactorer refactorer = LanguageBindingService.GetRefactorerForFile (fileName);
			refactorer.AddLocalNamespaceImport (new RefactorerContext (dom, fileProvider, null), fileName, nsName, caretLocation);
		}

	
		public DomLocation CompleteStatement (ProjectDom dom, string fileName, DomLocation caretLocation)
		{
			IRefactorer refactorer = LanguageBindingService.GetRefactorerForFile (fileName);
			return refactorer.CompleteStatement (new RefactorerContext (dom, fileProvider, null), fileName, caretLocation);
		}
		
//		public IType ImplementMembers (IType cls, IEnumerable<KeyValuePair<IMember,IReturnType>> members,
//		                                              string foldingRegionName)
//		{
//			RefactorerContext gctx = GetGeneratorContext (cls);
//			cls = GetUpdatedClass (gctx, cls);
//			IRefactorer gen = GetGeneratorForClass (cls);
//			gen.ImplementMembers (gctx, cls, members, foldingRegionName);
//			gctx.Save ();
//			return GetUpdatedClass (gctx, cls);
//		}
		
		string GenerateGenerics (IRefactorer gen, IType iface, IReturnType hintReturnType)
		{
			StringBuilder result = new StringBuilder ();
			if (hintReturnType != null && hintReturnType.GenericArguments != null) {
				result.Append ("<");
				for (int i = 0; i < hintReturnType.GenericArguments.Count; i++)  {
					result.Append (gen.ConvertToLanguageTypeName (RemoveGenericParamSuffix (hintReturnType.GenericArguments[i].FullName)));
					result.Append (GenerateGenerics (gen, iface, hintReturnType.GenericArguments[i]));
					if (i + 1 < hintReturnType.GenericArguments.Count)
						result.Append (", ");
				}
				result.Append (">");
			}
			return result.ToString ();
		}
		
		public static string RemoveGenericParamSuffix (string name)
		{
			int idx = name.IndexOf('`');
			if (idx > 0)
				return name.Substring (0, idx);
			return name;
		}
		
		static bool Equals (IList<IParameter> col1, IList<IParameter> col2)
		{
			if (col1 == null && col2 == null)
				return true;
			if (col1 == null  || col2 == null)
				return false;
			if (col1.Count != col2.Count)
				return false;
			for (int i = 0; i < col1.Count; i++) {
				if (!col1[i].ReturnType.Equals (col2[i].ReturnType))
					return false;
			}
			return true;
		}
		string GetExplicitPrefix (IEnumerable<IReturnType> explicitInterfaces)
		{	
			if (explicitInterfaces != null) {
				foreach (IReturnType retType in explicitInterfaces) {
					return retType.FullName;
				}
			}
			return null;
		}
		
//		public IType ImplementInterface (ICompilationUnit pinfo, IType klass, IType iface, bool explicitly, IType declaringClass, IReturnType hintReturnType)
//		{
//			if (klass == null)
//				throw new ArgumentNullException ("klass");
//			if (iface == null)
//				throw new ArgumentNullException ("iface");
//			RefactorerContext gctx = GetGeneratorContext (klass);
//			klass = GetUpdatedClass (gctx, klass);
//			
//			bool alreadyImplemented;
//			IReturnType prefix = null;
//			
//			List<KeyValuePair<IMember,IReturnType>> toImplement = new List<KeyValuePair<IMember,IReturnType>> ();
//			
//			prefix = new DomReturnType (iface);
//			
//			// Stub out non-implemented events defined by @iface
//			foreach (IEvent ev in iface.Events) {
//				if (ev.IsSpecialName)
//					continue;
//				bool needsExplicitly = explicitly;
//				
//				alreadyImplemented = gctx.ParserContext.GetInheritanceTree (klass).Any (x => x.ClassType != ClassType.Interface && x.Events.Any (y => y.Name == ev.Name));
//				
//				if (!alreadyImplemented)
//					toImplement.Add (new KeyValuePair<IMember,IReturnType> (ev, needsExplicitly ? prefix : null));
//			}
//			
//			// Stub out non-implemented methods defined by @iface
//			foreach (IMethod method in iface.Methods) {
//				if (method.IsSpecialName)
//					continue;
//				bool needsExplicitly = explicitly;
//				alreadyImplemented = false;
//				foreach (IType t in gctx.ParserContext.GetInheritanceTree (klass)) {
//					if (t.ClassType == ClassType.Interface)
//						continue;
//					foreach (IMethod cmet in t.Methods) {
//						if (cmet.Name == method.Name && Equals (cmet.Parameters, method.Parameters)) {
//							if (!needsExplicitly && !cmet.ReturnType.Equals (method.ReturnType))
//								needsExplicitly = true;
//							else
//								alreadyImplemented |= !needsExplicitly || (iface.FullName == GetExplicitPrefix (cmet.ExplicitInterfaces));
//						}
//					}
//				}
//				
//				if (!alreadyImplemented) 
//					toImplement.Add (new KeyValuePair<IMember,IReturnType> (method, needsExplicitly ? prefix : null));
//			}
//			
//			// Stub out non-implemented properties defined by @iface
//			foreach (IProperty prop in iface.Properties) {
//				if (prop.IsSpecialName)
//					continue;
//				bool needsExplicitly = explicitly;
//				alreadyImplemented = false;
//				foreach (IType t in gctx.ParserContext.GetInheritanceTree (klass)) {
//					if (t.ClassType == ClassType.Interface)
//						continue;
//					foreach (IProperty cprop in t.Properties) {
//						if (cprop.Name == prop.Name) {
//							if (!needsExplicitly && !cprop.ReturnType.Equals (prop.ReturnType))
//								needsExplicitly = true;
//							else
//								alreadyImplemented |= !needsExplicitly || (iface.FullName == GetExplicitPrefix (cprop.ExplicitInterfaces));
//						}
//					}
//				}
//				if (!alreadyImplemented)
//					toImplement.Add (new KeyValuePair<IMember,IReturnType> (prop, needsExplicitly ? prefix : null)); 				}
//			
//			Ambience ambience = AmbienceService.GetAmbienceForFile (klass.CompilationUnit.FileName);
//			//implement members
//			ImplementMembers (klass, toImplement, ambience.GetString (iface, OutputFlags.ClassBrowserEntries | OutputFlags.IncludeGenerics | OutputFlags.IncludeParameters) +  " implementation");
//			gctx.Save ();
//			
//			klass = GetUpdatedClass (gctx, klass);
//			foreach (IType baseClass in iface.SourceProjectDom.GetInheritanceTree (iface)) {
//				if (baseClass.Equals (iface) || baseClass.FullName == "System.Object")
//					continue;
//				klass = ImplementInterface (pinfo, klass, baseClass, explicitly, declaringClass, hintReturnType);
//			}
//			
//			
//			return klass;
//		}
		
		IType GetUpdatedClass (RefactorerContext gctx, IType klass)
		{
			IEditableTextFile file = gctx.GetFile (klass.CompilationUnit.FileName);
			ParsedDocument doc = ProjectDomService.Parse (gctx.ParserContext.Project, file.Name, delegate () { return file.Text; });
			IType result = gctx.ParserContext.GetType (klass.FullName, klass.TypeParameters.Count, true);
			if (result is CompoundType) {
				IType hintType = doc.CompilationUnit.GetType (klass.FullName, klass.TypeParameters.Count);
				if (hintType != null) 
					((CompoundType)result).SetMainPart (file.Name, hintType.Location);
			}
			return result;
		}
		
		public void RemoveMember (IType cls, IMember member)
		{
			try {
				RefactorerContext gctx = GetGeneratorContext (cls);
				IRefactorer gen = GetGeneratorForClass (cls);
				gen.RemoveMember (gctx, cls, member);
				gctx.Save ();
			} catch (Exception e) {
				LoggingService.LogError (GettextCatalog.GetString ("Error while removing {0}:{1}", member, e.ToString ()));
			}
		}
		
		IMember InnerRenameMember (IProgressMonitor monitor, IType cls, IMember member, string newName, RefactoryScope scope)
		{
			try {
				MemberReferenceCollection refs = new MemberReferenceCollection ();
				Refactor (monitor, cls, scope, new RefactorDelegate (new RefactorFindMemberReferences (cls, member, refs, false).Refactor));
				refs.RenameAll (newName);
				
				RefactorerContext gctx = GetGeneratorContext (cls);
				
				IRefactorer gen = GetGeneratorForClass (cls);
				IMember m = gen.RenameMember (gctx, cls, member, newName);
				gctx.Save ();
				return m;
			} catch (Exception e) {
				LoggingService.LogError (GettextCatalog.GetString ("Error while renaming {0} to {1}: {2}",  member, newName, e.ToString ()));
				return null;
			}
		}
		
		public IMember RenameMember (IProgressMonitor monitor, IType cls, IMember member, string newName, RefactoryScope scope)
		{
			// rename overriden members
			if (cls.ClassType == ClassType.Interface | member.IsAbstract | member.IsVirtual) {
				foreach (IType t in cls.SourceProjectDom.GetSubclasses (cls)) {
					foreach (IMember m in t.SearchMember (member.Name, true)) {
						InnerRenameMember (monitor, t, m, newName, scope);
					}
				}
			}
			
			// rename base members that are overridden as well.
			if (member.IsOverride) {
				foreach (IType t in cls.SourceProjectDom.GetInheritanceTree (cls)) {
					if (t == cls)
						continue;
					foreach (IMember m in t.SearchMember (member.Name, true)) {
						InnerRenameMember (monitor, t, m, newName, scope);
					}
				}
			}
			
			return InnerRenameMember (monitor, cls, member, newName, scope);
		}
		
		static RefactoryScope GetScope (IMember member)
		{
			if (member.DeclaringType != null && member.DeclaringType.ClassType == ClassType.Interface)
				return GetScope (member.DeclaringType);
			
			if (member.IsPublic)
				return RefactoryScope.Solution;
			
			if (member.IsProtected || member.IsInternal || member.DeclaringType == null)
				return RefactoryScope.Project;
			return RefactoryScope.DeclaringType;
		}
		
		public MemberReferenceCollection FindMemberReferences (IProgressMonitor monitor, IType cls, IMember member, bool includeXmlComment)
		{
			return FindMemberReferences (monitor, cls, member, GetScope (member), includeXmlComment);
		}
		
		public MemberReferenceCollection FindMemberReferences (IProgressMonitor monitor, IType cls, IMember member, RefactoryScope scope, bool includeXmlComment)
		{
			MemberReferenceCollection refs = new MemberReferenceCollection ();
			Refactor (monitor, cls, scope, new RefactorDelegate (new RefactorFindMemberReferences (cls, member, refs, includeXmlComment).Refactor));
			return refs;
		}
		
		public IMember ReplaceMember (IType cls, IMember oldMember, CodeTypeMember member)
		{
			try {
				RefactorerContext gctx = GetGeneratorContext (cls);
				IRefactorer gen = GetGeneratorForClass (cls);
				IMember m = gen.ReplaceMember (gctx, cls, oldMember, member);
				gctx.Save ();
				return m;
			} catch (Exception e) {
				LoggingService.LogError (GettextCatalog.GetString ("Error while replacing {0}: {1}",  member, e.ToString ()));
				return null;
			}
		}
		
//		public bool RenameVariable (IProgressMonitor monitor, LocalVariable var, string newName)
//		{
//			try {
//				MemberReferenceCollection refs = new MemberReferenceCollection ();
//				Refactor (monitor, var, new RefactorDelegate (new RefactorFindVariableReferences (var, refs).Refactor));
//				refs.RenameAll (newName);
//				
//				RefactorerContext gctx = GetGeneratorContext (var);
//				IRefactorer r = GetGeneratorForVariable (var);
//				bool rv = r.RenameVariable (gctx, var, newName);
//				gctx.Save ();
//				
//				return rv;
//			} catch (Exception e) {
//				LoggingService.LogError (GettextCatalog.GetString ("Error while renaming {0} to {1}: {2}",  var, newName, e.ToString ()));
//				return false;
//			}
//		}
//		
//		public bool RenameParameter (IProgressMonitor monitor, IParameter param, string newName)
//		{
//			try {
//				MemberReferenceCollection refs = new MemberReferenceCollection ();
//				Refactor (monitor, param, new RefactorDelegate (new RefactorFindParameterReferences (param, refs, false).Refactor));
//				refs.RenameAll (newName);
//				
//				IMember member = param.DeclaringMember;
//				RefactorerContext gctx = GetGeneratorContext (member.DeclaringType);
//				IRefactorer r = GetGeneratorForClass (member.DeclaringType);
//				bool rv = r.RenameParameter (gctx, param, newName);
//				gctx.Save ();
//				
//				return rv;
//			} catch (Exception e) {
//				LoggingService.LogError (GettextCatalog.GetString ("Error while renaming {0} to {1}: {2}",  param, newName, e.ToString ()));
//				return false;
//			}
//		}
		
		public IType[] FindDerivedClasses (IType baseClass)
		{
			ArrayList list = new ArrayList ();
			
			if (solution != null) {
				foreach (Project p in solution.GetAllProjects ()) {
					ProjectDom ctx = ProjectDomService.GetProjectDom (p);
					if (ctx == null)
						continue;
					foreach (IType cls in ctx.Types) {
						if (IsSubclass (ctx, baseClass, cls))
							list.Add (cls);
					}
				}
			} else {
				ProjectDom ctx = GetParserContext (baseClass);
				if (ctx != null) {
					foreach (IType cls in ctx.Types) {
						if (IsSubclass (ctx, baseClass, cls))
							list.Add (cls);
					}
				}
			}
			return (IType[]) list.ToArray (typeof(IType));
		}
		
		bool IsSubclass (ProjectDom ctx, IType baseClass, IType subclass)
		{
			foreach (IReturnType clsName in subclass.BaseTypes)
				if (clsName.FullName == baseClass.FullName)
					return true;

			foreach (IReturnType clsName in subclass.BaseTypes) {
				IType cls = ctx.GetType (clsName);
				if (cls != null && IsSubclass (ctx, baseClass, cls))
					return true;
			}
			return false;
		}
		
		void Refactor (IProgressMonitor monitor, IType cls, RefactoryScope scope, RefactorDelegate refactorDelegate)
		{
			switch (scope) {
			case RefactoryScope.DeclaringType:
				ProjectDom ctx = GetParserContext (cls);
				if (cls is InstantiatedType)
					cls = ((InstantiatedType)cls).UninstantiatedType;
				IType resolvedType = ctx.GetType (cls.FullName, cls.TypeParameters.Count, true, true);
				if (resolvedType == null) 
					goto case RefactoryScope.Solution;
				foreach (IType part in resolvedType.Parts) {
					string file = part.CompilationUnit.FileName;
					RefactorerContext gctx = GetGeneratorContext (part);
					IRefactorer gen = LanguageBindingService.GetRefactorerForFile (file);
					if (gen == null)
						return;
					refactorDelegate (monitor, gctx, gen, file);
					gctx.Save ();
				}
				break;
			case RefactoryScope.File: {
				string file = cls.CompilationUnit.FileName;
				RefactorerContext gctx = GetGeneratorContext (cls);
				IRefactorer gen = LanguageBindingService.GetRefactorerForFile (file);
				if (gen == null)
					return;
				refactorDelegate (monitor, gctx, gen, file);
				gctx.Save ();
				break;
				}
			case RefactoryScope.Project:
				Project prj = GetProjectForFile (cls.CompilationUnit.FileName);
				if (prj == null)
					return;
				RefactorProject (monitor, prj, refactorDelegate);
				break;
			case RefactoryScope.Solution:
				if (solution == null)
					goto case RefactoryScope.File;
				foreach (Project project in solution.GetAllProjects ())
					RefactorProject (monitor, project, refactorDelegate);
				break;
			}
				
		}
		
		void Refactor (IProgressMonitor monitor, LocalVariable var, RefactorDelegate refactorDelegate)
		{
			RefactorerContext gctx = GetGeneratorContext (var);
			string file = var.FileName;
			
			IRefactorer gen = LanguageBindingService.GetRefactorerForFile (file);
			if (gen == null)
				return;
			
			refactorDelegate (monitor, gctx, gen, file);
			gctx.Save ();
		}
		
		void Refactor (IProgressMonitor monitor, IParameter param, RefactorDelegate refactorDelegate)
		{
			IMember member = param.DeclaringMember;
			RefactorerContext gctx = GetGeneratorContext (member.DeclaringType);
			IType cls = member.DeclaringType;
			IRefactorer gen;
			string file;
			
			foreach (IType part in cls.Parts) {
				file = part.CompilationUnit.FileName;
				
				if ((gen = LanguageBindingService.GetRefactorerForFile (file)) == null)
					continue;
				
				refactorDelegate (monitor, gctx, gen, file);
				gctx.Save ();
			}
		}
		
		void RefactorProject (IProgressMonitor monitor, Project p, RefactorDelegate refactorDelegate)
		{
			RefactorerContext gctx = GetGeneratorContext (p);
			monitor.Log.WriteLine (GettextCatalog.GetString ("Refactoring project {0}", p.Name));
			foreach (ProjectFile file in p.Files) {
				if (file.BuildAction != BuildAction.Compile || !System.IO.File.Exists (file.FilePath))
					continue;
				IRefactorer gen = LanguageBindingService.GetRefactorerForFile (file.Name);
				if (gen == null)
					continue;
				refactorDelegate (monitor, gctx, gen, file.Name);
				gctx.Save ();
			}
		}
		
		RefactorerContext GetGeneratorContext (Project p)
		{
			ProjectDom ctx = ProjectDomService.GetProjectDom (p);
			return new RefactorerContext (ctx, fileProvider, null);
		}
		
		RefactorerContext GetGeneratorContext (IType cls)
		{
			return new RefactorerContext (GetParserContext (cls), fileProvider, cls);
		}
		
		RefactorerContext GetGeneratorContext (LocalVariable var)
		{
			return new RefactorerContext (GetParserContext (var), fileProvider, null);
		}
		
		ProjectDom GetParserContext (IType cls)
		{
			if (cls != null && cls.CompilationUnit != null) {
				Project p = GetProjectForFile (cls.CompilationUnit.FileName);
				if (p != null)
					return ProjectDomService.GetProjectDom (p);
			}
			return ProjectDom.Empty;
		}
		
		ProjectDom GetParserContext (LocalVariable var)
		{
			Project p = GetProjectForFile (var.FileName);
			if (p != null)
				return ProjectDomService.GetProjectDom (p);
			return ProjectDom.Empty;
		}
		
		Project GetProjectForFile (string file)
		{
			if (solution == null)
				return null;

			foreach (Project p in solution.GetAllProjects ())
				if (p.IsFileInProject (file))
					return p;
			return null;
		}
		
		IRefactorer GetGeneratorForClass (IType cls)
		{
			return LanguageBindingService.GetRefactorerForFile (cls.CompilationUnit.FileName);
		}
		
		IRefactorer GetGeneratorForVariable (LocalVariable var)
		{
			return LanguageBindingService.GetRefactorerForFile (var.FileName);
		}
	}
	
	class RefactorFindClassReferences
	{
		MemberReferenceCollection references;
		IType cls;
		bool includeXmlComment;
		
		public RefactorFindClassReferences (IType cls, MemberReferenceCollection references, bool includeXmlComment)
		{
			this.cls = cls;
			this.references = references;
			this.includeXmlComment = includeXmlComment;
		}
		
		public void Refactor (IProgressMonitor monitor, RefactorerContext rctx, IRefactorer r, string fileName)
		{
			try {
				IEnumerable<MemberReference> refs = r.FindClassReferences (rctx, fileName, cls, includeXmlComment);
				if (refs != null)
					references.AddRange (refs);
			} catch (Exception ex) {
				monitor.ReportError (GettextCatalog.GetString ("Could not look for references in file '{0}': {1}", fileName, ex.Message), ex);
			}
		}
	}
	
	class RefactorFindMemberReferences
	{
		IType cls;
		MemberReferenceCollection references;
		IMember member;
		bool includeXmlComment;
		
		public RefactorFindMemberReferences (IType cls, IMember member, MemberReferenceCollection references, bool includeXmlComment)
		{
			this.cls = cls;
			this.references = references;
			this.member = member;
			this.includeXmlComment = includeXmlComment;
		}
		
		public void Refactor (IProgressMonitor monitor, RefactorerContext rctx, IRefactorer r, string fileName)
		{
			try {
				IEnumerable<MemberReference> refs = r.FindMemberReferences (rctx, fileName, cls, member, includeXmlComment);
				if (refs != null)
					references.AddRange (refs);
			} catch (Exception ex) {
				monitor.ReportError (GettextCatalog.GetString ("Could not look for references in file '{0}': {1}", fileName, ex.Message), ex);
			}
		}
	}
	
	class RefactorFindVariableReferences
	{
		MemberReferenceCollection references;
		LocalVariable var;
		
		public RefactorFindVariableReferences (LocalVariable var, MemberReferenceCollection references)
		{
			this.references = references;
			this.var = var;
		}
		
		public void Refactor (IProgressMonitor monitor, RefactorerContext rctx, IRefactorer r, string fileName)
		{
			try {
				IEnumerable<MemberReference> refs = r.FindVariableReferences (rctx, fileName, var);
				if (refs != null)
					references.AddRange (refs);
			} catch (Exception ex) {
				monitor.ReportError (GettextCatalog.GetString ("Could not look for references in file '{0}': {1}", fileName, ex.Message), ex);
			}
		}
	}
	
	class RefactorFindParameterReferences
	{
		MemberReferenceCollection references;
		IParameter param;
		bool includeXmlComment;
		
		public RefactorFindParameterReferences (IParameter param, MemberReferenceCollection references, bool includeXmlComment)
		{
			this.references = references;
			this.param = param;
			this.includeXmlComment = includeXmlComment;
		}
		
		public void Refactor (IProgressMonitor monitor, RefactorerContext rctx, IRefactorer r, string fileName)
		{
			try {
				IEnumerable<MemberReference> refs = r.FindParameterReferences (rctx, fileName, param, includeXmlComment);
				if (refs != null)
					references.AddRange (refs);
			} catch (Exception ex) {
				monitor.ReportError (GettextCatalog.GetString ("Could not look for references in file '{0}': {1}", fileName, ex.Message), ex);
			}
		}
	}
	
	public enum RefactoryScope
	{
		File,
		Project,
		Solution,
		DeclaringType
	}
}
