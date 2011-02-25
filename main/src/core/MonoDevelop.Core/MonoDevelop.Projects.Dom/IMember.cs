//
// IMember.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using MonoDevelop.Projects.Dom.Parser;
using System.Collections.ObjectModel;

namespace MonoDevelop.Projects.Dom
{
	public enum MemberType
	{
		Field,
		Method,
		Property,
		Event,
		Type,
		Namespace,
		LocalVariable,
		Parameter
	}
	
	public interface IMember : IBaseMember, IComparable
	{
		ProjectDom SourceProjectDom {
			get;
		}
		
		string FullName {
			get;
		}
		
		IType DeclaringType {
			get;
			set;
		}
		
		IEnumerable<IReturnType> ExplicitInterfaces {
			get;
		}
		
		/// <summary>
		/// Gets or sets the documentation. To get the Documentation from a node the method GetDocumentation
		/// method from the ProjectDom should be used. This property only stores the documentation retrieved from the source
		/// code files.
		/// </summary>
		/// <value>
		/// The documentation.
		/// </value>
		string Documentation {
			get;
			set;
		}
		
		DomRegion BodyRegion {
			get;
		}
		
		Modifiers Modifiers {
			get;
		}
		
		IEnumerable<IAttribute> Attributes {
			get;
		}
		
		string HelpUrl {
			get;
		}
		
		bool IsExplicitDeclaration {
			get;
		}
		
		bool CanHaveParameters {
			get;
		}
		ReadOnlyCollection<IParameter> Parameters {
			get;
		}
		
		System.Xml.XmlNode GetMonodocDocumentation ();
		bool IsAccessibleFrom (ProjectDom dom, IType calledType, IMember member, bool includeProtected);
		
		#region ModifierAccessors
		bool IsObsolete { get; }
		bool IsPrivate   { get; }
		bool IsInternal  { get; }
		bool IsProtected { get; }
		bool IsPublic    { get; }
		bool IsProtectedAndInternal { get; }
		bool IsProtectedOrInternal { get; }
		
		bool IsAbstract  { get; }
		bool IsVirtual   { get; }
		bool IsSealed    { get; }
		bool IsStatic    { get; }
		bool IsOverride  { get; }
		bool IsReadonly  { get; }
		bool IsConst	 { get; }
		bool IsNew       { get; }
		bool IsPartial   { get; }
		
		bool IsExtern    { get; }
		bool IsVolatile  { get; }
		bool IsUnsafe    { get; }
		bool IsOverloads  { get; }
		bool IsWithEvents { get; }
		bool IsDefault    { get; }
		bool IsFixed      { get; }
		
		bool IsSpecialName { get; }
		bool IsFinal       { get; }
		bool IsLiteral     { get; }
		#endregion		
		
	}
}
