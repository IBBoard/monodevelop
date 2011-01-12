//
// TypeParameter.cs
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

namespace MonoDevelop.Projects.Dom
{
	public class TypeParameter : AbstractNode, ITypeParameter
	{
		string name;
		public string Name {
			get {
				return name;
			}
		}
		
		public TypeParameterVariance Variance { get; set; }

		public TypeParameterModifier TypeParameterModifier { get; set; }
		
		List<IAttribute> attributes = null;
		static readonly IAttribute[] emptyAttributes = new IAttribute[0];
		public IEnumerable<IAttribute> Attributes {
			get {
				if (attributes == null)
					return emptyAttributes;
				return attributes;
			}
		}

		List<IReturnType> constraints = null;
		static readonly IReturnType[] emptyConstraints = new IReturnType[0];
		public IList<IReturnType> Constraints {
			get {
				if (constraints == null)
					return emptyConstraints;
				return constraints;
			}
		}
		
		public TypeParameter (string name)
		{
			this.name = name;
		}
		
		public override string ToString ()
		{
			return string.Format ("[TypeParameter: Name={0}, Variance={1}, TypeParameterModifier={2}, Attributes={3}, Constraints={4}]", Name, Variance, TypeParameterModifier, Attributes, Constraints);
		}
 		
		public void AddConstraint (IReturnType type)
		{
			if (constraints == null)
				constraints = new List<IReturnType> ();
			this.constraints.Add (type);
		}

		public void AddAttribute (IAttribute attr)
		{
			if (attributes == null)
				attributes = new List<IAttribute> ();
			attributes.Add (attr);
		}
	}
}
