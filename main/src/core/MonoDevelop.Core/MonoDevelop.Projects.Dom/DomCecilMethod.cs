//
// DomCecilMethod.cs
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

using Mono.Cecil;

namespace MonoDevelop.Projects.Dom
{
	public class DomCecilMethod : MonoDevelop.Projects.Dom.DomMethod
	{
		MethodDefinition methodDefinition;
		
		public MethodDefinition MethodDefinition {
			get {
				return methodDefinition;
			}
		}
		
		/*
			// Check if 'type' has some decorations applied to it
				if (type is Mono.Cecil.TypeSpecification) {
					// Go through all levels of 'indirection', 'array dimensions'
					// and 'generic types' - in the end, we should get the actual
					// type of the ReturnType (but all data about its array
					// dimensions, levels of indirection and even its generic
					// parameters is correctly stored within ArrayCount and
					// ArrayDimensions, PointerNestingLevel and GenericArguments
					// respectively).
					if (type is ArrayType) {
						// This return type is obviously an array - add the rank
						ArrayType at = (ArrayType) type;
						if (arrays == null)
							arrays = new Stack<int>();
						arrays.Push(at.Rank);
						type = at.ElementType;
					} else else if (type is Mono.Cecil.ReferenceType) {
						Mono.Cecil.ReferenceType rt = (Mono.Cecil.ReferenceType) type;
						byRef = true;
						type = rt.ElementType;
					} else if (type is PointerType) {
						// The type is a pointer
						PointerType pt = (PointerType) type;
						++pointerNestingLevel;
						type = pt.ElementType;
						// Go down one level
					} else {
						// TODO: Check if we loose some relevant info here
						type = ((TypeSpecification)type).ElementType;
					}*/
		public static DomReturnType GetReturnType (TypeReference typeReference)
		{
			if (typeReference == null)
				return new DomReturnType (DomReturnType.Void.ToInvariantString ());
			
			if (typeReference is Mono.Cecil.GenericInstanceType) {
				Mono.Cecil.GenericInstanceType genType = (Mono.Cecil.GenericInstanceType)typeReference;
				DomReturnType result = GetReturnType (genType.ElementType);
				
				foreach (TypeReference typeRef in genType.GenericArguments) {
					DomReturnType param = GetReturnType (typeRef);
					
					foreach (IReturnTypePart part in result.Parts) {
						if (part.Tag is TypeDefinition) {
							TypeDefinition typeDef = (TypeDefinition)part.Tag;
							foreach (TypeReference typeParam in typeDef.GenericParameters) {
								if (typeParam.Name == param.Name) {
									part.AddTypeParameter (param);
									goto skip;
								}
							}
						}
					}
					result.AddTypeParameter (param);
				skip:;
				}
				return result;
			}
			
			if (typeReference is Mono.Cecil.ArrayType) {
				Mono.Cecil.ArrayType arrType = (Mono.Cecil.ArrayType)typeReference;
				DomReturnType result = GetReturnType (arrType.ElementType);
				result.ArrayDimensions++;
				result.SetDimension (result.ArrayDimensions - 1, arrType.Rank - 1);
				return result;
			}
			
			if (typeReference is Mono.Cecil.PointerType) {
				Mono.Cecil.PointerType ptrType = (Mono.Cecil.PointerType)typeReference;
				DomReturnType result = GetReturnType (ptrType.ElementType);
				if (result.ArrayDimensions > 0)
					result.ArrayPointerNestingLevel++;
				else 
					result.PointerNestingLevel++;
				return result;
			}
			if (typeReference is Mono.Cecil.ByReferenceType)
				return GetReturnType (((Mono.Cecil.ByReferenceType)typeReference).ElementType);
			
			if (typeReference is Mono.Cecil.TypeDefinition) {
				Mono.Cecil.TypeDefinition typeDefinition = (Mono.Cecil.TypeDefinition)typeReference;
				DomReturnType result;
				if (typeDefinition.DeclaringType != null) {
					result = GetReturnType (typeDefinition.DeclaringType);
					result.Parts.Add (new ReturnTypePart (typeDefinition.Name));
					result.Tag = typeDefinition;
				} else {
					result = new DomReturnType (typeDefinition.Name);
					result.Namespace = typeDefinition.Namespace;
					result.Tag = typeDefinition;
				}
				return result;
			}
			
			return new DomReturnType (DomCecilType.RemoveGenericParamSuffix (typeReference.FullName)); 
		}
		
		public static IReturnType GetReturnType (MethodReference methodReference)
		{
			if (methodReference == null)
				return DomReturnType.Void;
			return DomReturnType.GetSharedReturnType (DomCecilType.RemoveGenericParamSuffix (methodReference.DeclaringType.FullName));
		}
		
		public static void AddAttributes (AbstractMember member, IList<CustomAttribute> attributes)
		{
			foreach (CustomAttribute customAttribute in attributes) {
				member.Add (new DomCecilAttribute (customAttribute));
			}
		}
				
		public DomCecilMethod (MethodDefinition methodDefinition)
		{
			this.methodDefinition = methodDefinition;
			this.name = methodDefinition.Name;
			if (methodDefinition.Name == ".ctor") {
				MethodModifier |= MethodModifier.IsConstructor;
			}
			
			foreach (GenericParameter param in methodDefinition.GenericParameters) {
				TypeParameter tp = new TypeParameter (param.FullName);
				tp.Variance = (TypeParameterVariance)(((uint)param.Attributes) & 3);
				foreach (TypeReference tr in param.Constraints)
					tp.AddConstraint (DomCecilMethod.GetReturnType (tr));
				AddTypeParameter (tp);
			}
				
			AddAttributes (this, methodDefinition.CustomAttributes);
			base.Modifiers  = DomCecilType.GetModifiers (methodDefinition);
			base.ReturnType = DomCecilMethod.GetReturnType (methodDefinition.ReturnType);
			foreach (ParameterDefinition paramDef in methodDefinition.Parameters) {
				Add (new DomCecilParameter (paramDef));
			}
			
			if (this.IsStatic) {
				foreach (IAttribute attr in this.Attributes) {
					if (attr.Name == "System.Runtime.CompilerServices.ExtensionAttribute") {
						MethodModifier |= MethodModifier.IsExtension;
						break;
					}
				}
			}
			
			foreach (MethodReference overrideRef in methodDefinition.Overrides) {
				if (overrideRef.Name == this.name && IsPublic) 
					continue; 
				AddExplicitInterface (GetReturnType (overrideRef.DeclaringType));
			}
		}
	}
}
