// 
// RemoveBackingStore.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
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
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Ast;
using MonoDevelop.Core;
using Mono.TextEditor;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.CodeGeneration;
using MonoDevelop.Ide;
using MonoDevelop.Ide.FindInFiles;

namespace MonoDevelop.Refactoring.ConvertPropery
{
	public class RemoveBackingStore : RefactoringOperation
	{
		public override string GetMenuDescription (RefactoringOptions options)
		{
			return GettextCatalog.GetString ("_Remove backing store");
		}
		
		public override bool IsValid (RefactoringOptions options)
		{
			MemberResolveResult resolveResult = options.ResolveResult as MemberResolveResult;
			if (resolveResult == null)
				return false;
			IProperty property = resolveResult.ResolvedMember as IProperty;
			if (property == null || resolveResult.CallingMember == null || resolveResult.CallingMember.FullName != property.FullName || !property.HasGet || property.DeclaringType == null)
				return false;
			
			TextEditorData data = options.GetTextEditorData ();
			if (property.HasGet && data.Document.GetCharAt (data.Document.LocationToOffset (property.GetRegion.End.Line, property.GetRegion.End.Column - 1)) == ';')
				return false;
			if (property.HasSet && data.Document.GetCharAt (data.Document.LocationToOffset (property.SetRegion.End.Line, property.SetRegion.End.Column - 1)) == ';')
				return false;
			INRefactoryASTProvider astProvider = options.GetASTProvider ();
			string backingStoreName = RetrieveBackingStore (options, astProvider, property);
			if (string.IsNullOrEmpty (backingStoreName))
				return false;
			
			// look if there is a valid backing store field that doesn't have any attributes.
			int backinStoreStart;
			int backinStoreEnd;
			IField backingStore = GetBackingStoreField (options, backingStoreName, out backinStoreStart, out backinStoreEnd);
			if (backingStore == null || backingStore.Attributes.Any ())
				return false;
			return true;
		}
		
		public override List<Change> PerformChanges (RefactoringOptions options, object prop)
		{
			List<Change> result = new List<Change> ();
			TextEditorData data = options.GetTextEditorData ();
			MemberResolveResult resolveResult = options.ResolveResult as MemberResolveResult;
			IProperty property = resolveResult.ResolvedMember as IProperty;
			INRefactoryASTProvider astProvider = options.GetASTProvider ();
			string backingStoreName = RetrieveBackingStore (options, astProvider, property);
			
			int backinStoreStart;
			int backinStoreEnd;
			IField backingStore = GetBackingStoreField (options, backingStoreName, out backinStoreStart, out backinStoreEnd);
			
			if (backingStore != null) {
				foreach (MemberReference memberRef in ReferenceFinder.FindReferences (backingStore)) {
					result.Add (new TextReplaceChange () {
						FileName = memberRef.FileName,
						Offset = memberRef.Position,
						RemovedChars = memberRef.Name.Length,
						InsertedText = property.Name
					});
				}
				
				result.RemoveAll (c => backinStoreStart <= ((TextReplaceChange)c).Offset  && ((TextReplaceChange)c).Offset <= backinStoreEnd);
				result.Add (new TextReplaceChange () {
					FileName = options.Document.FileName,
					Offset = backinStoreStart,
					RemovedChars = backinStoreEnd - backinStoreStart
				});
			}
			
			if (property.HasGet) {
				int startOffset = data.Document.LocationToOffset (property.GetRegion.Start.ToDocumentLocation (data.Document));
				int endOffset = data.Document.LocationToOffset (property.GetRegion.End.ToDocumentLocation (data.Document));
				
				string text = astProvider.OutputNode (options.Dom, new PropertyGetRegion (null, null), options.GetIndent (property) + "\t").Trim ();
				
				result.RemoveAll (c => startOffset <= ((TextReplaceChange)c).Offset  && ((TextReplaceChange)c).Offset <= endOffset);
				result.Add (new TextReplaceChange () {
					FileName = options.Document.FileName,
					Offset = startOffset,
					RemovedChars = endOffset - startOffset,
					InsertedText = text
				});
			}
			
			int setStartOffset;
			int setEndOffset;
			PropertySetRegion setRegion = new PropertySetRegion (null, null);
			string setText;
			if (property.HasSet) {
				setStartOffset = data.Document.LocationToOffset (property.SetRegion.Start.ToDocumentLocation (data.Document));
				setEndOffset = data.Document.LocationToOffset (property.SetRegion.End.ToDocumentLocation (data.Document));
				setText = astProvider.OutputNode (options.Dom, setRegion, options.GetIndent (property) + "\t").Trim ();
			} else {
				setEndOffset = setStartOffset = data.Document.LocationToOffset (property.GetRegion.End.ToDocumentLocation (data.Document));
				setRegion.Modifier = ICSharpCode.NRefactory.Ast.Modifiers.Private;
				setText = Environment.NewLine + astProvider.OutputNode (options.Dom, setRegion, options.GetIndent (property) + "\t").TrimEnd ();
			}
			result.RemoveAll (c => setStartOffset <= ((TextReplaceChange)c).Offset  && ((TextReplaceChange)c).Offset <= setEndOffset);
			result.Add (new TextReplaceChange () {
				FileName = options.Document.FileName,
				Offset = setStartOffset,
				RemovedChars = setEndOffset - setStartOffset,
				InsertedText = setText
			});
			return result;
		}

		static IField GetBackingStoreField (MonoDevelop.Refactoring.RefactoringOptions options, string backingStoreName, out int backinStoreStart, out int backinStoreEnd)
		{
			TextEditorData data = options.GetTextEditorData ();
			MemberResolveResult resolveResult = options.ResolveResult as MemberResolveResult;
			IProperty property = resolveResult.ResolvedMember as IProperty;
			
			List<IMember> members = property.DeclaringType.SearchMember (backingStoreName, true);
			IMember backingStore = null;
			backinStoreStart = 0;
			backinStoreEnd = 0;
			foreach (IMember member in members) {
				if (member.MemberType == MemberType.Field) {
					DocumentLocation location = member.Location.ToDocumentLocation (data.Document);
					LineSegment line = data.Document.GetLine (location.Line);
					backinStoreStart = line.Offset;
					backinStoreEnd = line.EndOffset;
					backingStore = member;
					break;
				}
			}
			return backingStore as IField;
		}

		string RetrieveBackingStore (MonoDevelop.Refactoring.RefactoringOptions options, MonoDevelop.Refactoring.INRefactoryASTProvider astProvider, MonoDevelop.Projects.Dom.IProperty property)
		{
			ICSharpCode.NRefactory.Ast.CompilationUnit compilationUnit = astProvider.ParseFile (options.Document.Editor.Text);
			PropertyVisitor visitor = new PropertyVisitor (property);
			compilationUnit.AcceptVisitor (visitor, null);
			return visitor.BackingStoreName;
		}

	}
}
