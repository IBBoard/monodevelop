//
// ICompilationUnit.cs
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
using System.Collections.ObjectModel;
using MonoDevelop.Core;

namespace MonoDevelop.Projects.Dom
{
	public interface ICompilationUnit : INode
	{
		FilePath FileName {
			get;
		}
				
		ReadOnlyCollection<IUsing> Usings {
			get;
		}
		
		IEnumerable<IAttribute> Attributes {
			get;
		}
		
		ReadOnlyCollection<IType> Types {
			get;
		}
		
		IType GetType (string fullName, int genericParameterCount);
		IType GetTypeAt (int line, int column);
		IType GetTypeAt (DomLocation location);
		
		IMember GetMemberAt (int line, int column);
		IMember GetMemberAt (DomLocation location);
		
		void GetNamespaceContents (List<IMember> list, string subNameSpace, bool caseSensitive);
		
		bool IsNamespaceUsedAt (string name, int line, int column);
		bool IsNamespaceUsedAt (string name, DomLocation location);
		
		IReturnType ShortenTypeName (IReturnType fullyQualfiedType, DomLocation location);
		IReturnType ShortenTypeName (IReturnType fullyQualfiedType, int line, int column);
	}
}
