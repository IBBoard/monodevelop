// 
// ParameterDeclarationExpression.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.CSharp.Ast
{
	
	public enum ParameterModifier {
		None,
		Ref,
		Out,
		Params,
		This
	}
	
	public class ParameterDeclaration : AstNode
	{
		public override NodeType NodeType {
			get {
				return NodeType.Unknown;
			}
		}
		
		public ParameterModifier ParameterModifier {
			get;
			set;
		}
		
		public Identifier Identifier {
			get {
				return (Identifier)GetChildByRole (Roles.Identifier) ?? Identifier.Null;
			}
		}
		
		public string Name {
			get {
				Identifier i = this.Identifier;
				return i != null ? i.Name : null;
			}
		}
		
		public AstNode DefaultExpression {
			get {
				return GetChildByRole (Roles.Expression) ?? AstNode.Null;
			}
		}
		
		public AstNode Type {
			get { return GetChildByRole (Roles.ReturnType) ?? AstNode.Null; }
		}
		
		public IEnumerable<AttributeSection> Attributes {
			get {
				return base.GetChildrenByRole (Roles.Attribute).Cast <AttributeSection>();
			}
		}
		
		public override S AcceptVisitor<T, S> (AstVisitor<T, S> visitor, T data)
		{
			return visitor.VisitParameterDeclaration (this, data);
		}
	}
}

