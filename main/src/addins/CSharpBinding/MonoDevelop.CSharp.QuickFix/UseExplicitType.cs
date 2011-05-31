// 
// UseExplicitType.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
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
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Core;
using System.Collections.Generic;
using Mono.TextEditor;
using System.Linq;
using MonoDevelop.Refactoring;

namespace MonoDevelop.CSharp.QuickFix
{
	public class UseExplicitType : CSharpQuickFix
	{
		public UseExplicitType ()
		{
			Description = GettextCatalog.GetString ("Converts local variable declaration to be explicit typed.");
		}
		
		public override string GetMenuText (MonoDevelop.Ide.Gui.Document document, DomLocation loc)
		{
			return GettextCatalog.GetString ("Use explicit type");
		}

		VariableDeclarationStatement GetVariableDeclarationStatement (MonoDevelop.Ide.Gui.Document doc, DomLocation loc)
		{
			var unit = doc.ParsedDocument.LanguageAST as ICSharpCode.NRefactory.CSharp.CompilationUnit;
			if (unit == null)
				return null;
			
			var result = unit.GetNodeAt<VariableDeclarationStatement> (loc.Line, loc.Column);
			if (result != null && result.Variables.Count == 1 && !result.Variables.First ().Initializer.IsNull && result.Type.Contains (loc.Line, loc.Column) && result.Type.IsMatch (new SimpleType ("var"))) {
				var resolver = GetResolver (doc);
				var resolveResult = resolver.Resolve (result.Variables.First ().Initializer.ToString (), loc);
				if (resolveResult == null || resolveResult.ResolvedType == null || string.IsNullOrEmpty (resolveResult.ResolvedType.FullName))
					return null;
				return result;
				
			}
			return null;
		}
		
		public override bool IsValid (MonoDevelop.Ide.Gui.Document document, DomLocation loc)
		{
			return GetVariableDeclarationStatement (document, loc) != null;
		}
		
		public override void Run (MonoDevelop.Ide.Gui.Document document, DomLocation loc)
		{
			var varDecl = GetVariableDeclarationStatement (document, loc);
			if (varDecl == null)
				return;
			
			var resolver = GetResolver (document);
			var resolveResult = resolver.Resolve (varDecl.Variables.First ().Initializer.ToString (), loc);
			
			int offset = document.Editor.LocationToOffset (varDecl.Type.StartLocation.Line, varDecl.Type.StartLocation.Column);
			int endOffset = document.Editor.LocationToOffset (varDecl.Type.EndLocation.Line, varDecl.Type.EndLocation.Column);
			string text = OutputNode (document, ShortenTypeName (document, resolveResult.ResolvedType), "").Trim ();
			document.Editor.Replace (offset, endOffset - offset, text);
			document.Editor.Caret.Offset = offset + text.Length;
			document.Editor.Document.CommitUpdateAll ();
		}
	}
}

