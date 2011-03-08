//
// BaseRefactorer.cs
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
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;

using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Parser;
using MonoDevelop.Projects.Text;
using MonoDevelop.Projects.CodeGeneration;
using System.Text;

namespace MonoDevelop.Projects.CodeGeneration
{
	public abstract class BaseRefactorer: IRefactorer, INameValidator
	{
		public virtual RefactorOperations SupportedOperations {
			get { return RefactorOperations.All ^ RefactorOperations.AddFoldingRegion; }
		}
		
		protected abstract CodeDomProvider GetCodeDomProvider ();
		
		public virtual void AddAttribute (RefactorerContext ctx, IType cls, CodeAttributeDeclaration attr)
		{
			IEditableTextFile buffer = ctx.GetFile (cls.CompilationUnit.FileName);

			CodeTypeDeclaration type = new CodeTypeDeclaration ("temp");
			type.CustomAttributes.Add (attr);
			CodeDomProvider provider = GetCodeDomProvider ();
			StringWriter sw = new StringWriter ();
			provider.GenerateCodeFromType (type, sw, GetOptions (false));
			string code = sw.ToString ();
			int start = code.IndexOf ('[');
			int end = code.LastIndexOf (']');
			code = code.Substring (start, end-start+1) + Environment.NewLine;

			int line = cls.Location.Line;
			int col = cls.Location.Column;
			int pos = buffer.GetPositionFromLineColumn (line, col);

			code = Indent (code, GetLineIndent (buffer, line), false);
			buffer.InsertText (pos, code);
		}

		public IType CreateClass (RefactorerContext ctx, string directory, string namspace, CodeTypeDeclaration type)
		{
			CodeCompileUnit unit = new CodeCompileUnit ();
			CodeNamespace ns = new CodeNamespace (namspace);
			ns.Types.Add (type);
			unit.Namespaces.Add (ns);
			
			string file = Path.Combine (directory, type.Name + ".cs");
			StreamWriter sw = new StreamWriter (file);
			
			CodeDomProvider provider = GetCodeDomProvider ();
			provider.GenerateCodeFromCompileUnit (unit, sw, GetOptions (false));
			
			sw.Close ();
			
			
			ICompilationUnit pi = ProjectDomService.Parse (ctx.ParserContext.Project, file).CompilationUnit;
			IList<IType> clss = pi.Types;
			if (clss.Count > 0)
				return clss [0];
			else
				throw new Exception ("Class creation failed. The parser did not find the created class.");
		}
		
		public virtual IType RenameClass (RefactorerContext ctx, IType cls, string newName)
		{
			return null;
		}
		
		public virtual IEnumerable<MemberReference> FindClassReferences (RefactorerContext ctx, string file, IType cls, bool includeXmlComment)
		{
			return null;
		}
		
		public virtual IMember AddMember (RefactorerContext ctx, IType cls, CodeTypeMember member)
		{
			IEditableTextFile buffer = ctx.GetFile (cls.CompilationUnit.FileName);
			
			int pos = GetNewMemberPosition (buffer, cls, member);
			string code = GenerateCodeFromMember (member).Trim ();
			
			int line, col;
			buffer.GetLineColumnFromPosition (pos, out line, out col);
			
			string indent = GetLineIndent (buffer, cls.Location.Line) + "\t";
			code = Indent (code, indent, false);
			buffer.InsertText (pos, code);
			
			return FindGeneratedMember (ctx, buffer, cls, member, line);
		}
		
		public virtual void AddMembers (RefactorerContext ctx, IType cls, IEnumerable<CodeTypeMember> members)
		{
			foreach (CodeTypeMember member in members) {
				cls = GetMainPart (cls);
				AddMember (ctx, cls, member);
				IEditableTextFile buffer = ctx.GetFile (cls.CompilationUnit.FileName);
				cls = GetGeneratedClass (ctx, buffer, cls);
			}
		}
		
		public virtual void AddMembers (RefactorerContext ctx, IType cls, IEnumerable<CodeTypeMember> members, string foldingRegionName)
		{
			//no region name, so distribute them with like members
			if (string.IsNullOrEmpty (foldingRegionName)) {
				AddMembers (ctx, cls, members);
				return;
			}
			
			if (!members.Any ())
				return;
			
			IEditableTextFile buffer = ctx.GetFile (cls.CompilationUnit.FileName);
			int pos;
			
			// create/find the folding region, or if creation of regions isn't supported, put all the added
			// members in one place anyway
			if ((SupportedOperations & RefactorOperations.AddFoldingRegion) == 0) {
				pos = GetNewMethodPosition (buffer, cls);
			} else {
				pos = AddFoldingRegion (ctx, cls, foldingRegionName);
			}
			AddMembersAtPosition (ctx, cls, members, buffer, pos);
		}
		
		protected void AddMembersAtPosition (RefactorerContext ctx, IType cls, IEnumerable<CodeTypeMember> members, 
		                                     IEditableTextFile buffer, int pos)
		{
			int line, col;
			buffer.GetLineColumnFromPosition (pos, out line, out col);
			
			string indent = GetLineIndent (buffer, line);
			
			StringBuilder generatedString = new StringBuilder ();
			bool isFirst = true;
			foreach (CodeTypeMember member in members) {
				if (generatedString.Length > 0) {
					generatedString.AppendLine ();
				}
				generatedString.Append (Indent (GenerateCodeFromMember (member), indent, isFirst));
				isFirst = false;
			}
			
			// remove last new line + indent
			generatedString.Length -= indent.Length + Environment.NewLine.Length;
			// remove indent from last generated code member
			generatedString.Length -= indent.Length;
			if (buffer.GetCharAt (pos) == '\n')
				pos++;
			buffer.InsertText (pos, generatedString.ToString ());
		}
		
		public virtual int AddFoldingRegion (RefactorerContext ctx, IType cls, string regionName)
		{
			IEditableTextFile buffer = ctx.GetFile (cls.CompilationUnit.FileName);
			return GetNewMethodPosition (buffer, cls);
		}
		
		/*
		IReturnType GetGenericArgument (IType type, IReturnType rtype, IReturnType hintType)
		{
			if (hintType != null && type != null && rtype != null && type.GenericParameters != null)  {
				for (int i = 0; i < type.GenericParameters.Count; i++) {
					if (type.GenericParameters[i].Name == rtype.FullName) {
						return hintType.GenericArguments[i];
					}
				}
			}
			return null;
		}*/
		
		static string[] baseTypes = new string[] {"System.Void", "System.Object", "System.Boolean", 
			                         "System.Byte", "System.SByte", "System.Char", 
			                         "System.Enum", "System.Int16", "System.Int32", 
			                         "System.Int64", "System.UInt16", "System.UInt32",
			                         "System.UInt64", "System.Single", "System.Double",
			                         "System.Decimal", "System.String"};

		bool IsBaseType (string name)
		{
			foreach (string baseType in baseTypes) {
				if (name == baseType)
					return true;
			}
			return false;
		}
		
		protected CodeTypeReference ReturnTypeToDom (RefactorerContext ctx, ICompilationUnit unit, IReturnType declaredType)
		{
			CodeTypeReference [] argTypes = null;
			IReturnType rtype = declaredType;
			if (rtype == null)
				return null;
			IList<IReturnType> genericArgs = rtype.GenericArguments;
			if (genericArgs != null && genericArgs.Count > 0) {
				argTypes = new CodeTypeReference [genericArgs.Count];
				for (int i = 0; i < genericArgs.Count; i++) {
					argTypes[i] = ReturnTypeToDom (ctx, unit, genericArgs[i]);
				}
			}
			string name = IsBaseType (rtype.FullName) ? rtype.FullName : ctx.TypeNameResolver.ResolveName (rtype.FullName);
			CodeTypeReference typeRef = argTypes != null ? new CodeTypeReference (name, argTypes) : new CodeTypeReference (name);
			for (int i = 0; i < rtype.ArrayDimensions; i++) {
				typeRef = new CodeTypeReference (typeRef, rtype.GetDimension (i) + 1);
			}
			return typeRef;
		}
		
		protected CodeTypeReference TypeToDom (RefactorerContext ctx, Type type)
		{
			if (IsBaseType (type.FullName))
				return new CodeTypeReference (type);
// TODO:
//			return new CodeTypeReference (ctx.TypeNameResolver.ResolveName (type.FullName));
			return new CodeTypeReference (type.FullName);
		}
		
		public virtual IMember ImplementMember (RefactorerContext ctx, IType cls, IMember member, IReturnType privateImplementationType)
		{
			CodeTypeMember m = CreateImplementation (ctx, cls, member, privateImplementationType);
			return AddMember (ctx, cls, m);
		}
		
		public virtual void ImplementMembers (RefactorerContext ctx, IType cls, 
		                                                      IEnumerable<KeyValuePair<IMember,IReturnType>> members,
		                                                      string foldingRegionName)
		{
			AddMembers (ctx, cls, YieldImpls (ctx, cls, members), foldingRegionName);
		}
		
		//FIXME: this is a workaround for not being able to use LINQ, i.e.
		// from mem in members select CreateImplementation (ctx, cls, mem.Key, mem.Value)
		IEnumerable<CodeTypeMember> YieldImpls (RefactorerContext ctx, IType cls, 
		                                        IEnumerable<KeyValuePair<IMember,IReturnType>> members)
		{
			foreach (KeyValuePair<IMember,IReturnType> mem in members)
				yield return CreateImplementation (ctx, cls, mem.Key, mem.Value);
		}
		static CodeCommentStatement monoTouchModelStatement = new CodeCommentStatement ("TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute");

		protected CodeTypeMember CreateImplementation (RefactorerContext ctx, IType cls, IMember member, 
		                                               IReturnType privateImplementationType)
		{
			CodeTypeMember m;
			
			bool is_interface_method = member.DeclaringType.ClassType == ClassType.Interface;
			bool isIndexer = false;
			if (member is IEvent) {
				CodeMemberEvent mEvent = new CodeMemberEvent ();
				m = mEvent;
				mEvent.Type = ReturnTypeToDom (ctx, cls.CompilationUnit, member.ReturnType);
				if (!is_interface_method)
					mEvent.Attributes = MemberAttributes.Override;

				if (privateImplementationType != null)
					mEvent.PrivateImplementationType = ReturnTypeToDom (ctx, cls.CompilationUnit, privateImplementationType);
			} else if (member is IMethod) {
				CodeMemberMethod mMethod = new CodeMemberMethod ();
				IMethod method = (IMethod) member;
				m = mMethod;
				
				foreach (ITypeParameter param in method.TypeParameters)
					mMethod.TypeParameters.Add (param.Name);

				if (!is_interface_method)
					mMethod.Attributes = MemberAttributes.Override;
				
				mMethod.ReturnType = ReturnTypeToDom (ctx, cls.CompilationUnit, member.ReturnType);
				if (IsMonoTouchModelMember (method)) {
					mMethod.Statements.Add (monoTouchModelStatement);
				} else if (member.IsAbstract || member.DeclaringType.ClassType == ClassType.Interface) {
					CodeExpression nieReference = new CodeObjectCreateExpression (TypeToDom (ctx, typeof (NotImplementedException)));
					CodeStatement throwExpression = new CodeThrowExceptionStatement (nieReference);
					mMethod.Statements.Add (throwExpression);
				} else {
					List<CodeExpression> parameters = new List<CodeExpression> ();
					foreach (IParameter parameter in method.Parameters) {
						parameters.Add (new CodeVariableReferenceExpression (parameter.Name));
					}
					mMethod.Statements.Add (new CodeMethodInvokeExpression (new CodeBaseReferenceExpression (), member.Name, parameters.ToArray ()));
				}
				
				foreach (IParameter param in method.Parameters) {
					CodeParameterDeclarationExpression par;
					par = new CodeParameterDeclarationExpression (ReturnTypeToDom (ctx, cls.CompilationUnit, param.ReturnType), param.Name);
					if (param.IsOut)
						par.Direction = FieldDirection.Out;
					else if (param.IsRef)
						par.Direction = FieldDirection.Ref;
					if (param.IsParams)
						par.CustomAttributes.Add (new CodeAttributeDeclaration ("System.ParamArrayAttribute"));
					mMethod.Parameters.Add (par);
				}
				if (privateImplementationType != null)
					mMethod.PrivateImplementationType = ReturnTypeToDom (ctx, cls.CompilationUnit, privateImplementationType);
			} else if (member is IProperty) {
				IProperty property = (IProperty) member;
				
				if (!property.IsIndexer) {
					CodeMemberProperty mProperty = new CodeMemberProperty ();
					m = mProperty;
					if (!is_interface_method)
						mProperty.Attributes = MemberAttributes.Override;
				
					CodeExpression nieReference = new CodeObjectCreateExpression (TypeToDom (ctx, typeof (NotImplementedException)));
					
					CodeStatement throwExpression = new CodeThrowExceptionStatement (nieReference);
					mProperty.HasGet = property.HasGet;
					mProperty.HasSet = property.HasSet;
					if (property.HasGet) {
						if (IsMonoTouchModelMember (property)) {
							mProperty.SetStatements.Add (monoTouchModelStatement);
						} else if (member.IsAbstract || member.DeclaringType.ClassType == ClassType.Interface) {
							mProperty.GetStatements.Add (throwExpression);
						} else {
							mProperty.GetStatements.Add (new CodeMethodReturnStatement (new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), property.Name)));
						}
					}
					if (property.HasSet) {
						if (IsMonoTouchModelMember (property)) {
							mProperty.SetStatements.Add (monoTouchModelStatement);
						} else if (member.IsAbstract || member.DeclaringType.ClassType == ClassType.Interface) {
							mProperty.SetStatements.Add (throwExpression);
						} else {
							mProperty.SetStatements.Add (new CodeAssignStatement (new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), property.Name), new CodePropertySetValueReferenceExpression ()));
						}
					}
				
					mProperty.Type = ReturnTypeToDom (ctx, cls.CompilationUnit, member.ReturnType);
					if (privateImplementationType != null)
						mProperty.PrivateImplementationType = ReturnTypeToDom (ctx, cls.CompilationUnit, privateImplementationType);
				} else {
					isIndexer = true;
					CodeMemberProperty mProperty = new CodeMemberProperty ();
					m = mProperty;
					if (!is_interface_method)
						mProperty.Attributes = MemberAttributes.Override;
				
					CodeExpression nieReference = new CodeObjectCreateExpression (TypeToDom (ctx, typeof (NotImplementedException)));
					CodeStatement throwExpression = new CodeThrowExceptionStatement (nieReference);
					mProperty.HasGet = property.HasGet;
					mProperty.HasSet = property.HasSet;

					List<CodeExpression> parameters = new List<CodeExpression> ();
					foreach (IParameter parameter in property.Parameters) {
						parameters.Add (new CodeVariableReferenceExpression (parameter.Name));
					}

					if (mProperty.HasGet) {
						if (member.IsAbstract || member.DeclaringType.ClassType == ClassType.Interface) {
							mProperty.GetStatements.Add (throwExpression);
						} else {
							mProperty.GetStatements.Add (new CodeMethodReturnStatement (new CodeIndexerExpression (new CodeBaseReferenceExpression(), parameters.ToArray ())));
						}
					}
					if (mProperty.HasSet) {
						if (member.IsAbstract || member.DeclaringType.ClassType == ClassType.Interface) {
							mProperty.SetStatements.Add (throwExpression);
						} else {
							mProperty.SetStatements.Add (new CodeAssignStatement (new CodeIndexerExpression (new CodeBaseReferenceExpression(), parameters.ToArray ()), new CodePropertySetValueReferenceExpression ()));
						}
					}
				
					foreach (IParameter param in property.Parameters) {
						CodeParameterDeclarationExpression par;
						par = new CodeParameterDeclarationExpression (ReturnTypeToDom (ctx, cls.CompilationUnit, param.ReturnType), param.Name);
						mProperty.Parameters.Add (par);
					}
				
					mProperty.Type = ReturnTypeToDom (ctx, cls.CompilationUnit, member.ReturnType);
					if (privateImplementationType != null)
						mProperty.PrivateImplementationType = ReturnTypeToDom (ctx, cls.CompilationUnit, privateImplementationType);
				} 
			} else {
				return null;
			}
			m.Name = isIndexer ? "Item" : member.Name;
			if ((m.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Override)
				// Mark final if not overriding
				m.Attributes = (m.Attributes & ~MemberAttributes.ScopeMask) | MemberAttributes.Final;
			
			if (member.DeclaringType != null && member.DeclaringType.ClassType == ClassType.Interface) {
				m.Attributes = (m.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
			} else {
				if (member.IsPublic) {
					m.Attributes = (m.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Public;
				} else if (member.IsPrivate) {
					m.Attributes = (m.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Private;
				} else if (member.IsProtectedOrInternal) {
					m.Attributes = (m.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.FamilyOrAssembly;
				} else if (member.IsProtectedAndInternal) {
					m.Attributes = (m.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.FamilyOrAssembly;
				} else if (member.IsInternal) {
					m.Attributes = (m.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Assembly;
				} else if (member.IsProtected) {
					m.Attributes = (m.Attributes & ~MemberAttributes.AccessMask) | MemberAttributes.Family;
				}
			}
			return m;
		}
		
		public static bool IsMonoTouchModelMember (IMember member)
		{
			if (member == null || member.DeclaringType == null)
				return false;
			return member.DeclaringType.Attributes.Any (attr => attr.AttributeType != null && attr.AttributeType.FullName == "MonoTouch.Foundation.ModelAttribute");
		}
		
		public virtual void RemoveMember (RefactorerContext ctx, IType cls, IMember member)
		{
			IEditableTextFile buffer = null;
			int pos = -1;
			foreach (IType part in cls.Parts) {
				if ((buffer = ctx.GetFile (part.CompilationUnit.FileName)) == null)
					continue;
				
				if ((pos = GetMemberNamePosition (buffer, member)) != -1)
					break;
			}
			
			if (pos == -1)
				return;
			
			DomRegion reg = GetMemberBounds (buffer, member);
			int sp = buffer.GetPositionFromLineColumn (reg.Start.Line, reg.Start.Column);
			int ep = buffer.GetPositionFromLineColumn (reg.End.Line, reg.End.Column);
			buffer.DeleteText (sp, ep - sp);
		}
		
		public virtual IMember ReplaceMember (RefactorerContext ctx, IType cls, IMember oldMember, CodeTypeMember memberInfo)
		{
			IEditableTextFile buffer = null;
			int pos = -1;
			
			foreach (IType part in cls.Parts) {
				if ((buffer = ctx.GetFile (part.CompilationUnit.FileName)) == null)
					continue;
				
				if ((pos = GetMemberNamePosition (buffer, oldMember)) != -1)
					break;
			}
			
			if (pos == -1)
				return null;
			
			DomRegion reg = GetMemberBounds (buffer, oldMember);
			int sp = buffer.GetPositionFromLineColumn (reg.Start.Line, reg.Start.Column);
			int ep = buffer.GetPositionFromLineColumn (reg.End.Line, reg.End.Column);
			buffer.DeleteText (sp, ep - sp);
			
			string code = GenerateCodeFromMember (memberInfo);
			string indent = GetLineIndent (buffer, reg.Start.Line);
			code = Indent (code, indent, false);
			
			buffer.InsertText (sp, code);
			
			return FindGeneratedMember (ctx, buffer, cls, memberInfo, reg.Start.Line);
		}

		public virtual string ConvertToLanguageTypeName (string netTypeName)
		{
			return netTypeName;
		}
		
		public virtual IMember RenameMember (RefactorerContext ctx, IType cls, IMember member, string newName)
		{
			IEditableTextFile file = null;
			int pos = -1;
			
			foreach (IType part in cls.Parts) {
				if ((file = ctx.GetFile (part.CompilationUnit.FileName)) == null)
					continue;
				
				if ((pos = GetMemberNamePosition (file, member)) != -1)
					break;
			}
			
			if (pos == -1)
				return null;
			
			string name;
			if (member is IMethod && ((IMethod) member).IsConstructor)
				name = cls.Name;
			else
				name = member.Name;
			
			string txt = file.GetText (pos, pos + name.Length);
			if (txt != name)
				return null;
// Rename is done in CodeRefactorer.RenameMember. A simple text search & replace is NOT the way to rename things
// in source code.
//			file.DeleteText (pos, txt.Length);
//			file.InsertText (pos, newName);
			
			CodeTypeMember memberInfo;
			if (member is IField)
				memberInfo = new CodeMemberField ();
			else if (member is IMethod)
				memberInfo = new CodeMemberMethod ();
			else if (member is IProperty)
				memberInfo = new CodeMemberProperty ();
			else if (member is IEvent)
				memberInfo = new CodeMemberEvent ();
			else
				return null;
			
			memberInfo.Name = newName;
			return FindGeneratedMember (ctx, file, cls, memberInfo, member.Location.Line);
		}
		
		public virtual IEnumerable<MemberReference> FindMemberReferences (RefactorerContext ctx, string fileName, IType cls, IMember member, bool includeXmlComment)
		{
			if (member is IField)
				return FindFieldReferences (ctx, fileName, cls, (IField) member, includeXmlComment);
			else if (member is IMethod)
				return FindMethodReferences (ctx, fileName, cls, (IMethod) member, includeXmlComment);
			else if (member is IProperty)
				return FindPropertyReferences (ctx, fileName, cls, (IProperty) member, includeXmlComment);
			else if (member is IEvent)
				return FindEventReferences (ctx, fileName, cls, (IEvent) member, includeXmlComment);
			else
				return null;
		}
		
		///
		/// EncapsulateFieldImpGetSet:
		///
		/// Override this method for each language to fill-in the Get/SetStatements
		///
		protected virtual void EncapsulateFieldImpGetSet (RefactorerContext ctx, IType cls, IField field, CodeMemberProperty  prop)
		{
			
		}
		
		public virtual IMember EncapsulateField (RefactorerContext ctx, IType cls, IField field, string propName, MemberAttributes attr, bool generateSetter)
		{
			// If the field isn't already private/protected/internal, we'll need to fix it to be
			if (field.IsPublic || (!field.IsPrivate && !field.IsProtectedOrInternal) || true) {
				IEditableTextFile file = null;
				int pos = -1;
				
				// Find the file the field is contained in
				foreach (IType part in cls.Parts) {
					if ((file = ctx.GetFile (part.CompilationUnit.FileName)) == null)
						continue;
					
					if ((pos = GetMemberNamePosition (file, field)) != -1)
						break;
				}
				
				if (pos != -1) {
					// FIXME: need a way to get the CodeMemberField fieldInfo as a parsed object
					// (so we don't lose initialization state nor custom attributes, etc).
//					CodeMemberField fieldInfo = new CodeMemberField ();
//					
//					fieldInfo.Attributes = fieldInfo.Attributes & ~MemberAttributes.Public;
//					fieldInfo.Attributes |= MemberAttributes.Private;
//					
//					RemoveMember (ctx, cls, field);
//					AddMember (ctx, cls, fieldInfo);
					
					//int begin = file.GetPositionFromLineColumn (field.Region.Start.Line, field.Region.Start.Column);
					//int end = file.GetPositionFromLineColumn (field.Region.End.Line, field.Region.End.Column);
					//
					//string snippet = file.GetText (begin, end);
					//
					//Console.WriteLine ("field declaration: {0}", snippet);
					//
					//DomRegion region = GetMemberBounds (file, field);
					//
					//begin = file.GetPositionFromLineColumn (region.Start.Line, region.Start.Column);
					//end = file.GetPositionFromLineColumn (region.End.Line, region.End.Column);
					//
					//snippet = file.GetText (begin, end);
					//
					//Console.WriteLine ("delete '{0}'", snippet);
				}
			}
			
			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Name = propName;
			
			prop.Type = ReturnTypeToDom (ctx, cls.CompilationUnit, field.ReturnType);
			prop.Attributes = attr | MemberAttributes.Final;
			if (field.IsStatic)
				prop.Attributes |= MemberAttributes.Static;
			
			prop.HasGet = true;
			prop.HasSet = generateSetter;
			
			EncapsulateFieldImpGetSet (ctx, cls, field, prop);
			
			return AddMember (ctx, cls, prop);
		}
		

		/// Method overridables ////////////////////////////
		
		protected virtual IMethod RenameMethod (RefactorerContext ctx, IType cls, IMethod method, string newName)
		{
			return null;
		}
		
		protected virtual IEnumerable<MemberReference> FindMethodReferences (RefactorerContext ctx, string fileName, IType cls, IMethod method, bool includeXmlComment)
		{
			return null;
		}
		

		/// Field overridables ////////////////////////////
		
		protected virtual IField RenameField (RefactorerContext ctx, IType cls, IField field, string newName)
		{
			return null;
		}
		
		protected virtual IEnumerable<MemberReference> FindFieldReferences (RefactorerContext ctx, string fileName, IType cls, IField field, bool includeXmlComment)
		{
			return null;
		}


		/// Property overridables ////////////////////////////
		
		protected virtual IProperty RenameProperty (RefactorerContext ctx, IType cls, IProperty property, string newName)
		{
			return null;
		}
		
		protected virtual IEnumerable<MemberReference> FindPropertyReferences (RefactorerContext ctx, string fileName, IType cls, IProperty property, bool includeXmlComment)
		{
			return null;
		}

		/// Event overridables ////////////////////////////		
		
		protected virtual IEvent RenameEvent (RefactorerContext ctx, IType cls, IEvent evnt, string newName)
		{
			return null;
		}
		
		protected virtual IEnumerable<MemberReference> FindEventReferences (RefactorerContext ctx, string fileName, IType cls, IEvent evnt, bool includeXmlComment)
		{
			return null;
		}


		/// LocalVariable overridables /////////////////////
		
		public virtual bool RenameVariable (RefactorerContext ctx, LocalVariable var, string newName)
		{
			IEditableTextFile file = ctx.GetFile (var.FileName);
			if (file == null)
				return false;
			
			int pos = GetVariableNamePosition (file, var);
			if (pos == -1)
				return false;
			
			string txt = file.GetText (pos, pos + var.Name.Length);
			if (txt != var.Name)
				return false;
			
			file.DeleteText (pos, txt.Length);
			file.InsertText (pos, newName);
			
			ProjectDomService.Parse (ctx.ParserContext.Project, file.Name, delegate () { return file.Text; });
			
			return true;
		}

		public virtual IEnumerable<MemberReference> FindVariableReferences (RefactorerContext ctx, string fileName, LocalVariable var)
		{
			return null;
		}


		/// Parameter overridables /////////////////////
		
		public virtual bool RenameParameter (RefactorerContext ctx, IParameter param, string newName)
		{
			IMember member = param.DeclaringMember;
			IEditableTextFile file = null;
			int pos = -1;
			
			// It'd be nice if we didn't have to worry about this being null
			if (!member.DeclaringType.CompilationUnit.FileName.IsNull) {
				if ((file = ctx.GetFile (member.DeclaringType.CompilationUnit.FileName)) != null)
					pos = GetParameterNamePosition (file, param);
			}
			
			// Plan B. - fallback to searching all partial class files for this parameter's parent member
			if (pos == -1) {
				IType cls = member.DeclaringType;
				
				foreach (IType part in cls.Parts) {
					if ((file = ctx.GetFile (part.CompilationUnit.FileName)) == null)
						continue;
					
					// sanity check, if the parent member isn't here then neither is the param
					//if ((pos = GetMemberNamePosition (file, member)) == -1)
					//	continue;
					
					if ((pos = GetParameterNamePosition (file, param)) != -1)
						break;
				}
				
				if (pos == -1)
					return false;
			}
			
			string txt = file.GetText (pos, pos + param.Name.Length);
			if (txt != param.Name)
				return false;
			
			file.DeleteText (pos, txt.Length);
			file.InsertText (pos, newName);
			
			ProjectDomService.Parse (ctx.ParserContext.Project, file.Name, delegate () { return file.Text; });
			
			return true;
		}

		public virtual IEnumerable<MemberReference> FindParameterReferences (RefactorerContext ctx, string fileName, IParameter param, bool includeXmlComment)
		{
			return null;
		}

		/// Helper overridables ////////////////////////////

		protected virtual int GetMemberNamePosition (IEditableTextFile file, IMember member)
		{
			return -1;
		}

		protected virtual int GetVariableNamePosition (IEditableTextFile file, LocalVariable var)
		{
			return -1;
		}
		
		protected virtual int GetParameterNamePosition (IEditableTextFile file, IParameter param)
		{
			return -1;
		}

		protected virtual DomRegion GetMemberBounds (IEditableTextFile file, IMember member)
		{
			int minLin = member.Location.Line;
			int minCol = member.Location.Column;
			int maxLin = member.BodyRegion.End.Line;
			int maxCol = member.BodyRegion.End.Column;
			
		
			foreach (IAttribute att in member.Attributes) {
				if (att.Region.Start.Line < minLin) {
					minLin = att.Region.Start.Line;
					minCol = att.Region.Start.Column;
				} else if (att.Region.Start.Line == minLin && att.Region.Start.Column < minCol) {
					minCol = att.Region.Start.Column;
				}
				
				if (att.Region.End.Line > maxLin) {
					maxLin = att.Region.End.Line;
					maxCol = att.Region.End.Column;
				} else if (att.Region.End.Line == maxLin && att.Region.End.Column > maxCol) {
					maxCol = att.Region.End.Column;
				}
			}
			return new DomRegion (minLin, minCol, maxLin, maxCol);
		}
		
		protected virtual string GenerateCodeFromMember (CodeTypeMember member)
		{
			CodeTypeDeclaration type = new CodeTypeDeclaration ("temp");
			type.Members.Add (member);
			CodeDomProvider provider = GetCodeDomProvider ();
			StringWriter sw = new StringWriter ();
			provider.GenerateCodeFromType (type, sw, GetOptions (member is CodeMemberMethod));
			
			string code = sw.ToString ();
			int i = code.IndexOf ('{');
			int j = code.LastIndexOf ('}');
			code = code.Substring (i+1, j-i-1);
			if (member is CodeMemberMethod)
				if ((i = code.IndexOf ('(')) != -1)
					code = code.Insert (i, " ");
			
			code = code.TrimEnd ('\n', '\r', ' ', '\t');
			
			// remove empty preceeding lines
			string eol;
			string[] lines = SplitLines (code, out eol);
			
			int firstLine = -1;
			for (int k = 0; k < lines.Length; k++) {
				if (lines[k].Trim ().Length != 0)
					break;
				firstLine = k + 1;
			}
			if (firstLine >= 0) 
				code = String.Join (eol, lines, firstLine, lines.Length - firstLine);
			
			return RemoveIndent (code) + eol;
		}
		

		/// Helper methods ////////////////////////////

		// Returns a reparsed IType instance that contains the generated code.
		protected IType GetGeneratedClass (RefactorerContext ctx, IEditableTextFile buffer, IType cls)
		{
			// Don't get the class from the parse results because in that class the types are not resolved.
			// Get the class from the database instead.
			ParsedDocument doc = ProjectDomService.Parse (ctx.ParserContext.Project, buffer.Name, delegate () { return buffer.Text; });
			IType result = ctx.ParserContext.GetType (cls.FullName, cls.TypeParameters.Count, true, true);
			if (result is CompoundType) {
				IType hintType = doc.CompilationUnit.GetType (cls.FullName, cls.TypeParameters.Count);
				if (hintType != null)
					((CompoundType)result).SetMainPart (buffer.Name, hintType.Location);
			}
			return result;
		}
		
		protected IMember FindGeneratedMember (RefactorerContext ctx, IEditableTextFile buffer, IType cls, CodeTypeMember member, int line)
		{
			IType rclass = GetGeneratedClass (ctx, buffer, cls);
			
			if (rclass != null) {
				if (member is CodeMemberField) {
					foreach (IField m in rclass.Fields)
						if (m.Name == member.Name && line == m.Location.Line)
							return m;
				} else if (member is CodeMemberProperty) {
					foreach (IProperty m in rclass.Properties)
						if (m.Name == member.Name && line == m.Location.Line)
							return m;
				} else if (member is CodeMemberEvent) {
					foreach (IEvent m in rclass.Events)
						if (m.Name == member.Name && line == m.Location.Line)
							return m;
				} else if (member is CodeMemberMethod) {
					foreach (IMethod m in rclass.Methods) {
						if (m.Name == member.Name && line == m.Location.Line)
							return m;
					}
				}
			}
			return null;
		}
		
		static string[] SplitLines (string code, out string eol)
		{
			List<string> lines = new List<string> ();
			eol = null;
			int lastLineOffset = 0;
			for (int i = 0; i < code.Length; i++) {
				int additionalEolChars = 0;
				switch (code[i]) {
				case '\r':
					if (i + 1 < code.Length && code[i + 1] == '\n') {
						i++;
						if (eol == null)
							eol = "\r\n";
						additionalEolChars = 1;
					}
					if (eol == null)
						eol = "\r";
					break;
				case '\n':
					if (eol == null)
						eol = "\n";
					break;
				default:
					continue;
				}
				lines.Add (code.Substring (lastLineOffset, i - lastLineOffset - additionalEolChars));
				lastLineOffset = i + 1;
			}
			if (lastLineOffset < code.Length)
				lines.Add (code.Substring (lastLineOffset, code.Length - lastLineOffset));
			if (eol == null)
				eol = Environment.NewLine;
			return lines.ToArray ();
		}
		
		static int GetBlockIndent (IEnumerable<string> lines) 
		{
			int result = int.MaxValue;
			
			foreach (string line in lines) {
//				System.Console.WriteLine (">" + line.Replace("\n", "\\n").Replace ("\r", "\\r").Replace ("\t", "\\t"));
				for (int i = 0; i < line.Length; i++) {
					char ch = line[i];
					if (ch != ' ' && ch != '\t') {
						if (i < result)
							result = i;
						break;
					}
				}
			}
			
			return result == int.MaxValue ? 0 : result;
		}
		
		public static string RemoveIndent (string code)
		{
			string eol;
			string[] lines = SplitLines (code, out eol);
			int minInd = GetBlockIndent (lines);
			
			for (int i = 0; i < lines.Length; i++) {
				if (minInd > lines[i].Length)
					continue;
				
				lines[i] = lines[i].Substring (minInd);
			}
			
			StringBuilder result = new StringBuilder ();
			foreach (string line in lines) {
				result.Append (line);
				result.Append (eol);
			}
			return result.ToString ();
		}
		
		public static string Indent (string code, string indent, bool indentFirstLine)
		{
			StringBuilder result = new StringBuilder ();
			if (indentFirstLine)
				result.Append (indent);
			for (int i = 0; i < code.Length; i++) {
				result.Append (code[i]);
				if (code[i] == '\r') {
					if (i + 1 < code.Length && code[i + 1] == '\n') {
						result.Append ('\n');
						i++;
					}
					result.Append (indent);
				} else if (code[i] == '\n')
					result.Append (indent);
			}
			
			return result.ToString ();
		}
		
		protected int EnsurePositionIsNotInRegionsAndIndented (Project p, IEditableTextFile buffer, string indent, int position)
		{
			ParsedDocument doc = ProjectDomService.Parse (p, buffer.Name, delegate () { return buffer.Text; });
			int line, column;
			buffer.GetLineColumnFromPosition (position, out line, out column);
			
			foreach (FoldingRegion region in doc.AdditionalFolds) {
				if (region.Region.Contains (line, column)) {
					line = region.Region.End.Line + 1;
					column = 1;
				}
			}
			
			int result = buffer.GetPositionFromLineColumn (line, column);
			
			if (column != 1) {
				string eolMarker = Environment.NewLine;
				buffer.InsertText (result, eolMarker);
				result += eolMarker.Length;
			}
			
			buffer.InsertText (result, indent);
			result += indent.Length;
			
			return result;
		}
		
		protected virtual int GetNewMemberPosition (IEditableTextFile buffer, IType cls, CodeTypeMember member)
		{
			if (member is CodeMemberField)
				return GetNewFieldPosition (buffer, cls);
			if (member is CodeMemberMethod)
				return GetNewMethodPosition (buffer, cls);
			if (member is CodeMemberEvent)
				return GetNewEventPosition (buffer, cls);
			if (member is CodeMemberProperty)
				return GetNewPropertyPosition (buffer, cls);
			throw new InvalidOperationException ("Invalid member type: " + member);
		}
		
		protected static IType GetMainPart (IType t)
		{
			return t.HasParts ? t.Parts.First () : t;
		}
		
		protected virtual int GetNewFieldPosition (IEditableTextFile buffer, IType cls)
		{
			cls = GetMainPart (cls);
			if (cls.FieldCount == 0) {
				int sp = buffer.GetPositionFromLineColumn (cls.BodyRegion.Start.Line, cls.BodyRegion.Start.Column);
				int ep = buffer.GetPositionFromLineColumn (cls.BodyRegion.End.Line, cls.BodyRegion.End.Column);
				string s = buffer.GetText (sp, ep);
				int i = s.IndexOf ('{');
				if (i == -1) return -1;
				string ind = GetLineIndent (buffer, cls.BodyRegion.Start.Line) ;
				int pos;
				if (cls.BodyRegion.Start.Line == cls.BodyRegion.End.Line) {
					buffer.InsertText (sp + i + 1, "\n" + ind);
					pos = sp + i + 2;
				} else  {
					pos = GetNextLine (buffer, sp + i + 1);
//					buffer.InsertText (pos, ind + "\n");
//					pos += ind.Length + 1;
				}
				return EnsurePositionIsNotInRegionsAndIndented (cls.SourceProject as Project, buffer, ind + "\t", pos);
			} else {
				IField f = cls.Fields.Last ();
				int pos = buffer.GetPositionFromLineColumn (f.Location.Line, f.Location.Column);
				string ind = GetLineIndent (buffer, f.Location.Line);
				if (cls.BodyRegion.Start.Line == cls.BodyRegion.End.Line) {
					int sp = buffer.GetPositionFromLineColumn (cls.BodyRegion.Start.Line, cls.BodyRegion.Start.Column);
					int ep = buffer.GetPositionFromLineColumn (cls.BodyRegion.End.Line, cls.BodyRegion.End.Column);
					string s = buffer.GetText (sp, ep);
					int i = s.IndexOf ('}');
					if (i == -1) return -1;
//					buffer.InsertText (sp + i, "\n" + ind + "\t\n" + ind);
					pos = sp + i + ind.Length + 2;
				} else {
					pos = GetNextLine (buffer, pos);
				}
//				buffer.InsertText (pos, ind);
				return EnsurePositionIsNotInRegionsAndIndented (cls.SourceProject as Project, buffer, ind, pos);
			}
		}
		
		protected virtual int GetNewMethodPosition (IEditableTextFile buffer, IType cls)
		{
			cls = GetMainPart (cls);
			if (cls.MethodCount + cls.ConstructorCount == 0) {
				return GetNewPropertyPosition (buffer, cls);
				/*int pos = GetNewPropertyPosition (buffer, cls);
				int line, col;
				buffer.GetLineColumnFromPosition (pos, out line, out col);
				string ind = GetLineIndent (buffer, line);
				pos = GetNextLine (buffer, pos);
				return EnsurePositionIsNotInRegionsAndIndented (cls.SourceProject as Project, buffer, ind, pos);*/
			} else {
				var m = cls.Members .Last ();
				
				int pos;
				if (!m.BodyRegion.IsEmpty && m.BodyRegion.End.Line > 1) {
					pos = buffer.GetPositionFromLineColumn (m.BodyRegion.End.Line, m.BodyRegion.End.Column);
					pos = GetNextLine (buffer, pos);
					pos = SkipBlankLine (buffer, pos);
				} else {
					// Abstract or P/Inboke methods don't have a body
					pos = buffer.GetPositionFromLineColumn (m.Location.Line, m.Location.Column);
					pos = GetNextLine (buffer, pos);
				}
				
//				buffer.InsertText (pos++, "\n");
				string ind = GetLineIndent (buffer, m.Location.Line);
				pos = EnsurePositionIsNotInRegionsAndIndented (cls.SourceProject as Project, buffer, ind, pos);
				
				return pos;
			}
		}
		
		protected virtual int GetNewPropertyPosition (IEditableTextFile buffer, IType cls)
		{
			cls = GetMainPart (cls);
			if (cls.PropertyCount == 0) {
				return GetNewFieldPosition (buffer, cls);
/*				int pos = GetNewFieldPosition (buffer, cls);
				int line, col;
				buffer.GetLineColumnFromPosition (pos, out line, out col);
				string indent = GetLineIndent (buffer, line);
				pos = GetNextLine (buffer, pos);
				return EnsurePositionIsNotInRegionsAndIndented (cls.SourceProject as Project, buffer, indent, pos);*/
			} else {
				IProperty m = cls.Properties.Last ();
				
				int pos = buffer.GetPositionFromLineColumn (m.BodyRegion.End.Line, m.BodyRegion.End.Column);
				pos = GetNextLine (buffer, pos);
				pos = SkipBlankLine (buffer, pos);
				string indent = GetLineIndent (buffer, m.Location.Line);
				return EnsurePositionIsNotInRegionsAndIndented (cls.SourceProject as Project, buffer, indent, pos);
			}
		}
		
		protected virtual int GetNewEventPosition (IEditableTextFile buffer, IType cls)
		{
			cls = GetMainPart (cls);
			if (cls.EventCount == 0) {
				return GetNewMethodPosition (buffer, cls);
/*				int pos = GetNewMethodPosition (buffer, cls);
				int line, col;
				buffer.GetLineColumnFromPosition (pos, out line, out col);
				string ind = GetLineIndent (buffer, line);
				pos = GetNextLine (buffer, pos);
				return EnsurePositionIsNotInRegionsAndIndented (cls.SourceProject as Project, buffer, ind, pos);*/
			} else {
				IEvent m = GetMainPart (cls).Events.Last ();
				
				int pos;
				if (!m.BodyRegion.IsEmpty) {
					pos = buffer.GetPositionFromLineColumn (m.BodyRegion.End.Line, m.BodyRegion.End.Column);
					pos = GetNextLine (buffer, pos);
					pos = SkipBlankLine (buffer, pos);
				} else {
					pos = buffer.GetPositionFromLineColumn (m.Location.Line, m.Location.Column);
					pos = GetNextLine (buffer, pos);
				}

//				buffer.InsertText (pos++, "\n");
				string ind = GetLineIndent (buffer, m.Location.Line);
				return EnsurePositionIsNotInRegionsAndIndented (cls.SourceProject as Project, buffer, ind, pos);
			}
		}
		
		protected virtual int SkipBlankLine (IEditableTextFile buffer, int pos)
		{
			int i = pos;
			while (i < buffer.Length) {
				char ch = buffer.GetCharAt (i);
				switch (ch) {
				case '\n':
					return i + 1;
				case '\r':
					if (i + 1 < buffer.Length && buffer.GetCharAt (i + 1) == '\n')
						i++;
					return i + 1;
				case ' ':
				case '\t':
					i++;
					break;
				default:
					return pos;
				}
			}
			return pos;
		}
		
		protected virtual int GetNextLine (IEditableTextFile buffer, int pos)
		{
			if (pos < 0)
				return 0;
			while (pos < buffer.Length) {
				char ch = buffer.GetCharAt (pos);
				switch (ch) {
				case '\n':
					return pos + 1;
				case '\r':
					if (pos + 1 < buffer.Length && buffer.GetCharAt (pos + 1) == '\n')
						pos++;
					return pos + 1;
/*				case ' ':
				case '\t':
					pos++;
					break;*/
				default:
					pos++;
					continue;
				}
			}
			return pos;
		}
		
		protected string GetLineIndent (IEditableTextFile buffer, int line)
		{
			int pos = buffer.GetPositionFromLineColumn (line, 1);
			StringBuilder result = new StringBuilder ();
			while (pos < buffer.Length) {
				char ch = buffer.GetCharAt (pos);
				if (ch == ' ' || ch == '\t') {
					result.Append (ch);
					pos++;
					continue;
				}
				break;
			}
			return result.ToString ();
		}
		
		protected virtual CodeGeneratorOptions GetOptions (bool isMethod)
		{
			CodeGeneratorOptions ops = new CodeGeneratorOptions ();
			ops.IndentString = "\t";
			if (isMethod)
				ops.BracingStyle = "C";
			return ops;
		}
		public abstract void AddGlobalNamespaceImport (RefactorerContext ctx, string fileName, string nsName);
		public abstract void AddLocalNamespaceImport (RefactorerContext ctx, string fileName, string nsName, DomLocation caretLocation);
		public abstract DomLocation CompleteStatement (RefactorerContext ctx, string fileName, DomLocation caretLocation);
		public abstract ValidationResult ValidateName (INode visitable, string name);
	}
}
