//
// DomCecilType.cs
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
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using MonoDevelop.Projects.Dom;
using System.Text;

namespace MonoDevelop.Projects.Dom
{
	public class DomCecilType : MonoDevelop.Projects.Dom.DomType
	{
		TypeDefinition typeDefinition;
		internal bool LoadMonotouchDocumentation { get; private set; }
		
		static ClassType GetClassType (TypeDefinition typeDefinition)
		{
			if (typeDefinition.BaseType != null && (typeDefinition.BaseType.FullName == "System.Delegate" || typeDefinition.BaseType.FullName == "System.MulticastDelegate"))
				return ClassType.Delegate;
			if (typeDefinition.IsInterface)
				return ClassType.Interface;
			if (typeDefinition.IsEnum)
				return ClassType.Enum;
			if (typeDefinition.IsValueType)
				return ClassType.Struct;
			return ClassType.Class;
		}
		
		public static string RemoveGenericParamSuffix (string name)
		{
			name = name.Replace ('/', '.'); // nested classes are represented as A/Nested and in the dom it's just A.Nested
			int idx = name.IndexOf('`');
			
			if (idx > 0) {
				StringBuilder result = new StringBuilder ();
				bool gotTypeParams = false;
				foreach (char ch in name) {
					if (ch == '`') {
						gotTypeParams = true;
						continue;
					}
					if (gotTypeParams && Char.IsDigit (ch))
						continue;
					gotTypeParams = false;
					result.Append (ch);
				}
				return result.ToString ();
			}
			return name;
		}
		
		public override IEnumerable<IMember> Members {
			get {
				CheckInitialization ();
				return base.Members;
			}
		}

		public DomCecilType (TypeDefinition typeDefinition) : this (typeDefinition, true)
		{
		}
		
		public DomCecilType (TypeReference typeReference)
		{
			this.classType = ClassType.Unknown;
			this.Modifiers = Modifiers.None;
			this.Name      = DomCecilType.RemoveGenericParamSuffix (typeReference.Name);
			this.Namespace = typeReference.Namespace;
		}
		
		public DomCecilType (TypeDefinition typeDefinition, bool loadInternal, bool loadMonotouchDocumentation = true)
		{
			this.LoadMonotouchDocumentation = loadMonotouchDocumentation;
			this.typeDefinition = typeDefinition;
			this.loadInternal = loadInternal;
			this.classType = GetClassType (typeDefinition);
			
			this.Name = DomCecilType.RemoveGenericParamSuffix (typeDefinition.Name);
			this.Namespace = typeDefinition.Namespace;
			
			this.Modifiers = GetModifiers (typeDefinition.Attributes);
			
			if (typeDefinition.BaseType != null)
				this.baseType = DomCecilMethod.GetReturnType (typeDefinition.BaseType);
			DomCecilMethod.AddAttributes (this, typeDefinition.CustomAttributes);
			
			foreach (TypeReference interfaceReference in typeDefinition.Interfaces) {
				this.AddInterfaceImplementation (DomCecilMethod.GetReturnType (interfaceReference));
			}
			foreach (GenericParameter parameter in typeDefinition.GenericParameters) {
				TypeParameter tp = new TypeParameter (parameter.FullName);
				tp.Variance = (TypeParameterVariance)(((uint)parameter.Attributes) & 3);
				if (parameter.HasDefaultConstructorConstraint)
					tp.TypeParameterModifier |= TypeParameterModifier.HasDefaultConstructorConstraint;
				foreach (TypeReference tr in parameter.Constraints) {
					tp.AddConstraint (DomCecilMethod.GetReturnType (tr));
				}
				AddTypeParameter (tp);
			}
			AddDocumentation (this);
		}

		bool loadInternal;
		bool isInitialized = false;

		bool AddDocumentation (IMember member)
		{
			if (!LoadMonotouchDocumentation)
				return false;
			try {
				var node = member.GetMonodocDocumentation ();
				if (node == null)
					return true;
				string innerXml = (node.InnerXml ?? "").Trim ();
				var sb = new StringBuilder ();
				bool wasWhiteSpace = false;
				for (int i = 0; i < innerXml.Length; i++) {
					char ch = innerXml [i];
					switch (ch) {
					case '\n':
					case '\r':
						break;
					default:
						bool isWhiteSpace = Char.IsWhiteSpace (ch);
						if (isWhiteSpace && wasWhiteSpace)
							continue;
						wasWhiteSpace = isWhiteSpace;
						sb.Append (ch);
						break;
					}
				}
				member.Documentation = sb.ToString ();
				return true;
			} catch (Exception) {
				LoadMonotouchDocumentation = false;
				return false;
			}
		}
		
		void CheckInitialization ()
		{
			if (isInitialized)
				return;
			isInitialized = true;
			
			foreach (FieldDefinition fieldDefinition in typeDefinition.Fields) {
				if (!loadInternal && DomCecilCompilationUnit.IsInternal (DomCecilType.GetModifiers (fieldDefinition)))
					continue;
				var field = new DomCecilField (fieldDefinition);
				base.Add (field);
				AddDocumentation (field);
			}
			foreach (MethodDefinition methodDefinition in typeDefinition.Methods.Where (m => !m.IsConstructor)) {
				if (!loadInternal && DomCecilCompilationUnit.IsInternal (DomCecilType.GetModifiers (methodDefinition)))
					continue;
				var method = new DomCecilMethod (methodDefinition);
				base.Add (method);
				AddDocumentation (method);
			}
			
			bool internalOnly = true;
			bool hasConstructors = false;
			foreach (MethodDefinition methodDefinition in typeDefinition.Methods.Where (m => m.IsConstructor)) {
				hasConstructors = true;
				if (!loadInternal && DomCecilCompilationUnit.IsInternal (DomCecilType.GetModifiers (methodDefinition)))
					continue;
				internalOnly = false;
				base.Add (new DomCecilMethod (methodDefinition));
			}
			if (hasConstructors && internalOnly) 
				base.TypeModifier |= TypeModifier.HasOnlyHiddenConstructors;
			
			foreach (PropertyDefinition propertyDefinition in typeDefinition.Properties) {
				if (!loadInternal && DomCecilCompilationUnit.IsInternal (DomCecilType.GetModifiers (propertyDefinition.Attributes)))
					continue;
				base.Add (new DomCecilProperty (propertyDefinition));
			}
			foreach (EventDefinition eventDefinition in typeDefinition.Events) {
				if (!loadInternal && DomCecilCompilationUnit.IsInternal (DomCecilType.GetModifiers (eventDefinition.Attributes)))
					continue;
				base.Add (new DomCecilEvent (eventDefinition));
			}
			
			foreach (TypeDefinition nestedType in typeDefinition.NestedTypes) {
				if (!loadInternal && DomCecilCompilationUnit.IsInternal (DomCecilType.GetModifiers (nestedType.Attributes)))
					continue;
				DomCecilType innerType = new DomCecilType (nestedType, loadInternal);
				base.Add (innerType);
				if (typeParameters != null && innerType.typeParameters != null)
					innerType.typeParameters.RemoveAll (para => typeParameters.Any (myPara => myPara.Name == para.Name));
			}
		}
		
		public TypeDefinition TypeDefinition {
			get {
				return typeDefinition;
			}
		}
		
		public static MonoDevelop.Projects.Dom.Modifiers GetModifiers (Mono.Cecil.TypeAttributes attr)
		{
			MonoDevelop.Projects.Dom.Modifiers result = MonoDevelop.Projects.Dom.Modifiers.None;
			if ((attr & TypeAttributes.Abstract) == TypeAttributes.Abstract)
				result |= Modifiers.Abstract;
			if ((attr & TypeAttributes.Sealed) == TypeAttributes.Sealed)
				result |= Modifiers.Sealed;
			if ((attr & TypeAttributes.SpecialName) == TypeAttributes.SpecialName)
				result |= Modifiers.SpecialName;
			
			if ((attr & TypeAttributes.NestedPrivate) == TypeAttributes.NestedPrivate) {
				result |= Modifiers.Private;
			} else if ((attr & TypeAttributes.Public) == TypeAttributes.Public || (attr & TypeAttributes.NestedPublic) == TypeAttributes.NestedPublic) {
				result |= Modifiers.Public;
			} else if ((attr & TypeAttributes.NestedFamANDAssem) == TypeAttributes.NestedFamANDAssem) {
				result |= Modifiers.ProtectedAndInternal;
			} else if ((attr & TypeAttributes.NestedFamORAssem) == TypeAttributes.NestedFamORAssem) {
				result |= Modifiers.ProtectedOrInternal;
			} else if ((attr & TypeAttributes.NestedFamily) == TypeAttributes.NestedFamily) {
				result |= Modifiers.Protected;
			} else {
				result |= Modifiers.Private;
			}
			
			if ((attr & TypeAttributes.NestedAssembly) == TypeAttributes.NestedAssembly) {
				result |= Modifiers.Internal;
			} 
			return result;
		}
		
		public static MonoDevelop.Projects.Dom.Modifiers GetModifiers (Mono.Cecil.FieldDefinition field)
		{
			MonoDevelop.Projects.Dom.Modifiers result = MonoDevelop.Projects.Dom.Modifiers.None;
			if (field.IsStatic)
				result |= Modifiers.Static;
			
			if (field.IsLiteral) 
				result |= Modifiers.Literal;
			
			if (field.IsInitOnly)
				result |= Modifiers.Readonly;
			
			if (field.IsPublic) {
				result |= Modifiers.Public;
			} else if (field.IsPrivate) {
				result |= Modifiers.Private;
			} else {
				result |= Modifiers.Protected;
			}
			if (field.IsSpecialName)
				result |= Modifiers.SpecialName;
			if (field.IsAssembly)
				result |= Modifiers.Internal;
			return result;
		}
		
		public static MonoDevelop.Projects.Dom.Modifiers GetModifiers (Mono.Cecil.EventAttributes attr)
		{
			MonoDevelop.Projects.Dom.Modifiers result = MonoDevelop.Projects.Dom.Modifiers.None;
			if ((attr & EventAttributes.SpecialName) == EventAttributes.SpecialName) 
				result |= Modifiers.SpecialName;
			return result;
		}
		
		public static MonoDevelop.Projects.Dom.Modifiers GetModifiers (Mono.Cecil.MethodDefinition method)
		{
			MonoDevelop.Projects.Dom.Modifiers result = MonoDevelop.Projects.Dom.Modifiers.None;
			
			if (method.IsStatic) 
				result |= Modifiers.Static;
				
			if (method.IsAbstract) {
				result |= Modifiers.Abstract;
			} else if (method.IsFinal) {
				result |= Modifiers.Final;
			} else if (method.IsVirtual) {
				result |= Modifiers.Virtual;
			}

			if (method.IsPublic) {
				result |= Modifiers.Public;
			} else if (method.IsPrivate) {
				result |= Modifiers.Private;
			} else {
				result |= Modifiers.Protected;
			}
			if (method.IsAssembly)
				result |= Modifiers.Internal;
			
			if (method.IsSpecialName)
				result |= Modifiers.SpecialName;
			
			return result;
		}
		
		public static MonoDevelop.Projects.Dom.Modifiers GetModifiers (Mono.Cecil.PropertyAttributes attr)
		{
			MonoDevelop.Projects.Dom.Modifiers result = MonoDevelop.Projects.Dom.Modifiers.None;
			if ((attr & PropertyAttributes.SpecialName) == PropertyAttributes.SpecialName) 
				result |= Modifiers.SpecialName;
			return result;
		}
	}
}
