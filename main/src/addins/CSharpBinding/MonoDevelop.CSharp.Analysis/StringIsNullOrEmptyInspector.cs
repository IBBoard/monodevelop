// 
// StringIsNullOrEmptyInspector.cs
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
using MonoDevelop.Core;
using MonoDevelop.AnalysisCore;
using MonoDevelop.CSharp.QuickFix;
using MonoDevelop.Projects.Dom;

namespace MonoDevelop.CSharp.Analysis
{
	public class StringIsNullOrEmptyInspector : CSharpInspector
	{
		static BinaryOperatorExpression[] Matches;
		static BinaryOperatorExpression[] NegatedMatches;
		
		static StringIsNullOrEmptyInspector ()
		{
			// str == null && str == "" equals string.IsNullOrEmpty (str)
			Matches = new [] {
				new BinaryOperatorExpression (
					new BinaryOperatorExpression (new NullReferenceExpression (), BinaryOperatorType.Equality, new AnyNode ()),
					BinaryOperatorType.ConditionalAnd,
					new BinaryOperatorExpression (new PrimitiveExpression (""), BinaryOperatorType.Equality, new AnyNode ())
				),
				new BinaryOperatorExpression (
					new BinaryOperatorExpression (new NullReferenceExpression (), BinaryOperatorType.Equality, new AnyNode ()),
					BinaryOperatorType.ConditionalAnd,
					new BinaryOperatorExpression (new AnyNode (), BinaryOperatorType.Equality, new PrimitiveExpression (""))
				),
				new BinaryOperatorExpression (
					new BinaryOperatorExpression (new AnyNode (), BinaryOperatorType.Equality, new NullReferenceExpression ()),
					BinaryOperatorType.ConditionalAnd,
					new BinaryOperatorExpression (new PrimitiveExpression (""), BinaryOperatorType.Equality, new AnyNode ())
				),
				new BinaryOperatorExpression (
					new BinaryOperatorExpression (new AnyNode (), BinaryOperatorType.Equality, new NullReferenceExpression ()),
					BinaryOperatorType.ConditionalAnd,
					new BinaryOperatorExpression (new AnyNode (), BinaryOperatorType.Equality, new PrimitiveExpression (""))
				)
			};
			
			// str != null && str != "" equals !string.IsNullOrEmpty (str)
			NegatedMatches = new [] {
				new BinaryOperatorExpression (
					new BinaryOperatorExpression (new NullReferenceExpression (), BinaryOperatorType.InEquality, new AnyNode ()),
					BinaryOperatorType.ConditionalAnd,
					new BinaryOperatorExpression (new PrimitiveExpression (""), BinaryOperatorType.InEquality, new AnyNode ())
				),
				new BinaryOperatorExpression (
					new BinaryOperatorExpression (new NullReferenceExpression (), BinaryOperatorType.InEquality, new AnyNode ()),
					BinaryOperatorType.ConditionalAnd,
					new BinaryOperatorExpression (new AnyNode (), BinaryOperatorType.InEquality, new PrimitiveExpression (""))
				),
				new BinaryOperatorExpression (
					new BinaryOperatorExpression (new AnyNode (), BinaryOperatorType.InEquality, new NullReferenceExpression ()),
					BinaryOperatorType.ConditionalAnd,
					new BinaryOperatorExpression (new PrimitiveExpression (""), BinaryOperatorType.InEquality, new AnyNode ())
				),
				new BinaryOperatorExpression (
					new BinaryOperatorExpression (new AnyNode (), BinaryOperatorType.InEquality, new NullReferenceExpression ()),
					BinaryOperatorType.ConditionalAnd,
					new BinaryOperatorExpression (new AnyNode (), BinaryOperatorType.InEquality, new PrimitiveExpression (""))
				)
			};
		}
		
		static bool ComparesEqualNodes (BinaryOperatorExpression binOp)
		{
			var left = binOp.Left as BinaryOperatorExpression;
			var right = binOp.Right as BinaryOperatorExpression;
			
			return left.Left.IsMatch (right.Right) || left.Left.IsMatch (right.Left) ||
				left.Right.IsMatch (right.Right) || left.Right.IsMatch (right.Left);
		}
		
		public override void Attach (ICSharpCode.NRefactory.CSharp.ObservableAstVisitor visitior)
		{
			visitior.BinaryOperatorExpressionVisited += delegate(BinaryOperatorExpression binOp) {
				foreach (var match in Matches) {
					if (match.IsMatch (binOp) && ComparesEqualNodes (binOp)) {
						results.Add (new FixableResult (
							new DomRegion (binOp.StartLocation.Line, binOp.StartLocation.Column, binOp.EndLocation.Line, binOp.EndLocation.Column),
							GettextCatalog.GetString ("Use string.IsNullOrEmpty"),
							ResultLevel.Suggestion, ResultCertainty.High, ResultImportance.Medium,
							new UseStringIsNullOrEmptyFix (binOp)));
						
					}
				}
				foreach (var match in NegatedMatches) {
					if (match.IsMatch (binOp) && ComparesEqualNodes (binOp)) {
						results.Add (new FixableResult (
							new DomRegion (binOp.StartLocation.Line, binOp.StartLocation.Column, binOp.EndLocation.Line, binOp.EndLocation.Column),
							GettextCatalog.GetString ("Use string.IsNullOrEmpty"),
							ResultLevel.Suggestion, ResultCertainty.High, ResultImportance.Medium,
							new UseStringIsNullOrEmptyFix (binOp, true)));
						
					}
				}
			};
		}
		
		internal class UseStringIsNullOrEmptyFix : IAnalysisFix
		{
			public BinaryOperatorExpression BinaryOperatorExpression { get; private set; }
			public bool IsNegated { get; private set; }
			
			public UseStringIsNullOrEmptyFix (BinaryOperatorExpression binaryOperatorExpression, bool negated = false)
			{
				this.BinaryOperatorExpression = binaryOperatorExpression;
				this.IsNegated = negated;
			}
			
			public Expression GetParameter ()
			{
				var left = BinaryOperatorExpression.Left as BinaryOperatorExpression;
				var right = BinaryOperatorExpression.Right as BinaryOperatorExpression;
			
				if (left.Left.IsMatch (right.Right))
					return left.Left.Clone ();
				if (left.Right.IsMatch (right.Right))
					return left.Right.Clone ();
				if (left.Left.IsMatch (right.Left))
					return left.Left.Clone ();
				return left.Right.Clone ();
			}
			
			#region IAnalysisFix implementation
			public string FixType {
				get {
					return "UseStringIsNullOrEmptyFix";
				}
			}
			#endregion
			
		}
	}
	class UseStringIsNullOrEmptyFixHandler : IFixHandler
	{
			#region IFixHandler implementation
		public System.Collections.Generic.IEnumerable<IAnalysisFixAction> GetFixes (MonoDevelop.Ide.Gui.Document doc, object fix)
		{
			var useStringIsNullOrEmptyFix = fix as StringIsNullOrEmptyInspector.UseStringIsNullOrEmptyFix;
				
			yield return new UseStringIsNullOrEmptyFixAction () {
					Document = doc,
					UseStringIsNullOrEmptyFix = useStringIsNullOrEmptyFix
				};
				
		}
			#endregion
	}
		
	class UseStringIsNullOrEmptyFixAction : IAnalysisFixAction
	{
		internal MonoDevelop.Ide.Gui.Document Document;
		internal StringIsNullOrEmptyInspector.UseStringIsNullOrEmptyFix UseStringIsNullOrEmptyFix;
			
		#region IAnalysisFixAction implementation
		public void Fix ()
		{
			Expression invocation = new InvocationExpression (new MemberReferenceExpression (new TypeReferenceExpression (new PrimitiveType ("string")), "IsNullOrEmpty"), UseStringIsNullOrEmptyFix.GetParameter ());
			
			if (UseStringIsNullOrEmptyFix.IsNegated)
				invocation = new UnaryOperatorExpression (UnaryOperatorType.Not, invocation);
			
			string text = CSharpQuickFix.OutputNode (Document, invocation, "").Trim ();
				
			int offset = Document.Editor.LocationToOffset (UseStringIsNullOrEmptyFix.BinaryOperatorExpression.StartLocation.Line, UseStringIsNullOrEmptyFix.BinaryOperatorExpression.StartLocation.Column);
			int endOffset = Document.Editor.LocationToOffset (UseStringIsNullOrEmptyFix.BinaryOperatorExpression.EndLocation.Line, UseStringIsNullOrEmptyFix.BinaryOperatorExpression.EndLocation.Column);
				
			Document.Editor.Replace (offset, endOffset - offset, text);
			Document.Editor.Document.CommitUpdateAll ();
		}
			
		public string Label {
			get {
				return GettextCatalog.GetString ("Use string.IsNullOrEmpty");
			}
		}
		#endregion
	}	
}

