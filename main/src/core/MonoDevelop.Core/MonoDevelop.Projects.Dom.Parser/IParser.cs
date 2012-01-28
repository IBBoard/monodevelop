//
// IParser.cs
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
using System.IO;

namespace MonoDevelop.Projects.Dom.Parser
{
	public interface IParser
	{
		ParsedDocument Parse (ProjectDom dom, string fileName, string content);
		ParsedDocument Parse (ProjectDom dom, string fileName, TextReader content);
		
		/// <summary>
		/// Checks whether the parser can parse a file. Use for more granular checks after the 
		/// parser extension node's file extension check succeeds.
		/// </summary>
		bool CanParse (string fileName);
		
		IExpressionFinder CreateExpressionFinder (ProjectDom dom);
		IResolver         CreateResolver (ProjectDom dom, object editor, string fileName);
	}
	
	/// <summary>
	/// The folding parser is used for generating a preliminary parsed document that does not
	/// contain a full dom - only some basic lexical constructs like comments or pre processor directives.
	/// 
	/// This is useful for opening a document the first time to have some folding regions as start that are folded by default.
	/// Otherwise an irritating screen update will occur.
	/// </summary>
	public interface IFoldingParser
	{
		ParsedDocument Parse (string fileName, string content);
	}
}
