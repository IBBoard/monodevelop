//
// AbstractMember.cs
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
using System.Linq;
using MonoDevelop.Projects.Dom.Parser;
using System.Collections.ObjectModel;
using MonoDevelop.Core;

namespace MonoDevelop.Projects.Dom
{
	public abstract class AbstractMember : AbstractNode, IMember
	{
		protected List<IReturnType> explicitInterfaces = null;
		
		protected IType declaringType;
		
		public abstract MemberType MemberType {
			get;
		}
		
		public virtual ProjectDom SourceProjectDom {
			get {
				return DeclaringType.SourceProjectDom;
			}
			set {
				// nothing
			}
		}
		
		
		public IType DeclaringType {
			get {
				return declaringType;
			}
			set {
				this.Parent = this.declaringType = value;
				fullNameIsDirty = true;
			}
		}
		
		string fullName;
		public virtual string FullName {
			get {
				if (fullNameIsDirty) {
					fullName = CalculateFullName ();
					fullNameIsDirty = false;
				}
				return fullName;
			}
		}
		
		protected bool fullNameIsDirty = true;
		protected virtual string CalculateFullName ()
		{
			return DeclaringType != null ? DeclaringType.FullName + "." + Name : Name;
		}
		
		public virtual IReturnType ReturnType {
			get;
			set;
		}
		
		public IEnumerable<IReturnType> ExplicitInterfaces {
			get {
				return (IEnumerable<IReturnType>)explicitInterfaces ?? new IReturnType [0];
			}
		}
		
		public bool IsExplicitDeclaration {
			get {
				return explicitInterfaces != null && explicitInterfaces.Count > 0;
			}
		}
		
		protected string name;
		public virtual string Name {
			get {
				return name;
			}
			set {
				name = value;
				fullNameIsDirty = true;
			}
		}
		
		public virtual string Documentation {
			get;
			set;
		}
		
		public virtual DomLocation Location {
			get;
			set;
		}
		
		public virtual DomRegion BodyRegion {
			get;
			set;
		}
		
		public virtual Modifiers Modifiers {
			get;
			set;
		}
		
		public virtual bool IsObsolete {
			get {
				foreach (IAttribute attr in Attributes) {
					switch (attr.Name) {
					case "System.Obsolete":
					case "System.ObsoleteAttribute":
					case "Obsolete":
					case "ObsoleteAttribute":
						return true;
					}
				}
				return false;
			}
		}
		
		public virtual bool CanHaveParameters {
			get {
				return false;
			}
		}
		public virtual ReadOnlyCollection<IParameter> Parameters {
			get {
				throw new InvalidOperationException ();
			}
		}
		
		public virtual void Add (IParameter parameter)
		{
			throw new InvalidOperationException ();
		}
		
		public void Add (IEnumerable<IParameter> parameters)
		{
			if (parameters == null)
				return;
			foreach (IParameter parameter in parameters) {
				Add (parameter);
			}
		}
		
		List<IAttribute> attributes = null;
		static readonly IAttribute[] emptyAttributes = new IAttribute[0];
		public virtual IEnumerable<IAttribute> Attributes {
			get {
				return (IEnumerable<IAttribute>)attributes ?? emptyAttributes;
			}
		}
		
		public void AddExplicitInterface (IReturnType iface)
		{
			if (explicitInterfaces == null) 
				explicitInterfaces = new List<IReturnType> ();
			explicitInterfaces.Add (iface);
		}
		
		protected void ClearAttributes ()
		{
			if (attributes != null)
				attributes.Clear ();
		}
		
		public void Add (IAttribute attribute)
		{
			if (attributes == null)
				attributes = new List<IAttribute> ();
			attributes.Add (attribute);
		}
		
		public void AddRange (IEnumerable<IAttribute> attributes)
		{
			if (attributes == null)
				return;
			foreach (IAttribute attribute in attributes) {
				Add (attribute);
			}
		}
		
		/// <summary>
		/// This method is used to look up special methods that are connected to
		/// the member (like set/get method for events).
		/// </summary>
		/// <param name="prefix">
		/// A <see cref="System.String"/> for the prefix. For example the property Name has the method set_Name attacehd
		/// and 'set_' is the prefix.
		/// </param>
		/// <returns>
		/// A <see cref="IMethod"/> when the special method is found, null otherwise.
		/// </returns>
		protected IMethod LookupSpecialMethod (string prefix)
		{
			if (DeclaringType == null)
				return null;
			string specialMethodName = prefix + Name;
			foreach (IMethod method in DeclaringType.Methods) {
				if (method.IsSpecialName && method.Name == specialMethodName)
					return method;
			}
			return null;
		}
		
		public abstract string HelpUrl {
			get;
		}
		
		public abstract IconId StockIcon {
			get;
		}
		
		/// <summary>
		/// Help method used for getting the right icon for a member.
		/// </summary>
		/// <param name="modifier">
		/// A <see cref="Modifiers"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> 
		/// </returns>
		protected static int ModifierToOffset (Modifiers modifier)
		{
			if ((modifier & Modifiers.Private) == Modifiers.Private)
				return 1;
			if ((modifier & Modifiers.Protected) == Modifiers.Protected)
				return 2;
			if ((modifier & Modifiers.Internal) == Modifiers.Internal)
				return 3;
			return 0;
		}
//		Dictionary<string, bool> protectedTable = new Dictionary<string, bool> ();
		public virtual bool IsAccessibleFrom (ProjectDom dom, IType calledType, IMember member, bool includeProtected)
		{
			if (member == null)
				return IsStatic || IsPublic;
			if (member is MonoDevelop.Projects.Dom.BaseResolveResult.BaseMemberDecorator) 
				return IsPublic | IsProtected;
	//		if (member.IsStatic && !IsStatic)
	//			return false;
			if (IsPublic || calledType != null && calledType.ClassType == ClassType.Interface && !IsProtected)
				return true;
			
			if (this.DeclaringType != null && this.DeclaringType.ClassType == ClassType.Interface) 
				return this.DeclaringType.IsAccessibleFrom (dom, calledType, member, includeProtected);
			
			if (IsProtected && !(IsProtectedOrInternal && !includeProtected))
				return includeProtected;
			
			if (IsInternal || IsProtectedAndInternal) {
				IType type1 = this is IType ? (IType)this : DeclaringType;
				IType type2 = member is IType ? (IType)member : member.DeclaringType;
				bool result;
				// easy case, projects are the same
				if (type1.SourceProjectDom == type2.SourceProjectDom) {
					result = true;
				} else if (type1.SourceProjectDom != null && type1.SourceProjectDom.Project != null) {
					// maybe type2 hasn't project dom set (may occur in some cases), check if the file is in the project
					result = type1.SourceProjectDom.Project.GetProjectFile (type2.CompilationUnit.FileName) != null;
				} else if (type2.SourceProjectDom != null && type2.SourceProjectDom.Project != null) {
					result = type2.SourceProjectDom.Project.GetProjectFile (type1.CompilationUnit.FileName) != null;
				} else {
					// should never happen !
					result = true;
				}
				return IsProtectedAndInternal ? includeProtected && result : result;
			}
			
			if (!(member is IType) && (member.DeclaringType == null || DeclaringType == null))
				return false;
			
			// inner class 
			IType declaringType = member.DeclaringType;
			while (declaringType != null) {
				if (declaringType.Equals (DeclaringType))
					return true;
				declaringType = declaringType.DeclaringType;
			}
			
			
			return member.DeclaringType != null && DeclaringType.FullName == member.DeclaringType.FullName;
		}
		
		
		public virtual int CompareTo (object obj)
		{
			if (obj is IMember)
				return Name.CompareTo (((IMember)obj).Name);
			return 1;
		}
		
		#region ModifierAccessors
		public bool IsPrivate { 
			get {
				return (this.Modifiers & Modifiers.Private) == Modifiers.Private;
			}
		}
		public bool IsInternal { 
			get {
				return (this.Modifiers & Modifiers.Internal) == Modifiers.Internal;
			}
		}
		public bool IsProtected { 
			get {
				return (this.Modifiers & Modifiers.Protected) == Modifiers.Protected;
			}
		}
		public bool IsPublic { 
			get {
				return (this.Modifiers & Modifiers.Public) == Modifiers.Public;
			}
		}
		public bool IsProtectedAndInternal { 
			get {
				return (this.Modifiers & Modifiers.ProtectedAndInternal) == Modifiers.ProtectedAndInternal;
			}
		}
		public bool IsProtectedOrInternal { 
			get {
				return (this.Modifiers & Modifiers.ProtectedOrInternal) == Modifiers.ProtectedOrInternal;
			}
		}
		
		public bool IsAbstract { 
			get {
				return (this.Modifiers & Modifiers.Abstract) == Modifiers.Abstract;
			}
		}
		public bool IsVirtual { 
			get {
				return (this.Modifiers & Modifiers.Virtual) == Modifiers.Virtual;
			}
		}
		public bool IsSealed { 
			get {
				return (this.Modifiers & Modifiers.Sealed) == Modifiers.Sealed;
			}
		}
		public bool IsStatic { 
			get {
				return (this.Modifiers & Modifiers.Static) == Modifiers.Static;
			}
		}
		public bool IsOverride { 
			get {
				return (this.Modifiers & Modifiers.Override) == Modifiers.Override;
			}
		}
		public bool IsReadonly { 
			get {
				return (this.Modifiers & Modifiers.Readonly) == Modifiers.Readonly;
			}
		}
		public bool IsConst { 
			get {
				return (this.Modifiers & Modifiers.Const) == Modifiers.Const;
			}
		}
		public bool IsNew { 
			get {
				return (this.Modifiers & Modifiers.New) == Modifiers.New;
			}
		}
		public bool IsPartial { 
			get {
				return (this.Modifiers & Modifiers.Partial) == Modifiers.Partial;
			}
		}
		
		public bool IsExtern { 
			get {
				return (this.Modifiers & Modifiers.Extern) == Modifiers.Extern;
			}
		}
		public bool IsVolatile { 
			get {
				return (this.Modifiers & Modifiers.Volatile) == Modifiers.Volatile;
			}
		}
		public bool IsUnsafe { 
			get {
				return (this.Modifiers & Modifiers.Unsafe) == Modifiers.Unsafe;
			}
		}
		public bool IsOverloads { 
			get {
				return (this.Modifiers & Modifiers.Overloads) == Modifiers.Overloads;
			}
		}
		public bool IsWithEvents { 
			get {
				return (this.Modifiers & Modifiers.WithEvents) == Modifiers.WithEvents;
			}
		}
		public bool IsDefault { 
			get {
				return (this.Modifiers & Modifiers.Default) == Modifiers.Default;
			}
		}
		public bool IsFixed { 
			get {
				return (this.Modifiers & Modifiers.Fixed) == Modifiers.Fixed;
			}
		}
		
		public bool IsSpecialName { 
			get {
				return (this.Modifiers & Modifiers.SpecialName) == Modifiers.SpecialName;
			}
		}
		public bool IsFinal { 
			get {
				return (this.Modifiers & Modifiers.Final) == Modifiers.Final;
			}
		}
		public bool IsLiteral { 
			get {
				return (this.Modifiers & Modifiers.Literal) == Modifiers.Literal;
			}
		}
		#endregion
		
		public virtual System.Xml.XmlNode GetMonodocDocumentation ()
		{
			if (DeclaringType == null)
				return null;
			
			if (DeclaringType.HelpXml != null)  {
				System.Xml.XmlNode result = DeclaringType.HelpXml.SelectSingleNode ("/Type/Members/Member[@MemberName='" + Name + "']/Docs");
				return result;
			}
			return null;
		}
		
		public override bool Equals (object obj)
		{
			IMember m = obj as IMember;
			if (m == null)
				return false;
			return this.Location == m.Location && this.FullName == m.FullName;
		}
		
		public override int GetHashCode ()
		{
			return this.Location.GetHashCode () ^ this.FullName.GetHashCode ();
		}
	}
}
