//
// DocumentTests.cs
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
using NUnit.Framework;
using Mono.TextEditor.Utils;
using NUnit.Framework.SyntaxHelpers;
using System.Collections.Generic;

namespace Mono.TextEditor.Tests
{
	[TestFixture()]
	public class DocumentTests
	{
		[Test()]
		public void TestDocumentCreation ()
		{
			Document document = new Mono.TextEditor.Document ();
			
			string text = 
			"1234567890\n" +
			"12345678\n" +
			"1234567\n" +
			"123456\n" +
			"12345\n" +
			"1234\n" +
			"123\n" +
			"12\n" +
			"1\n" +
			"\n";
			document.Text = text;
			
			Assert.AreEqual (text, document.Text);
			Assert.AreEqual (11, document.LineCount);
		}
		
		[Test]
		public void TestDocumentInsert ()
		{
			Document document = new Mono.TextEditor.Document ();
			
			string top  = "1234567890\n";
			string text =
			"12345678\n" +
			"1234567\n" +
			"123456\n" +
			"12345\n" +
			"1234\n" +
			"123\n" +
			"12\n" +
			"1\n" +
			"\n";
			
			document.Text = top;
			((IBuffer)document).Insert (top.Length, text);
			Assert.AreEqual (top + text, document.Text);
		}
		
		[Test]
		public void TestDocumentRemove ()
		{
			Document document = new Mono.TextEditor.Document ();
			
			string top      = "1234567890\n";
			string testText =
			"12345678\n" +
			"1234567\n" +
			"123456\n" +
			"12345\n" +
			"1234\n" +
			"123\n" +
			"12\n" +
			"1\n" +
			"\n";
			document.Text = top + testText;
			((IBuffer)document).Remove (0, top.Length);
			Assert.AreEqual (document.Text, testText);
			
			((IBuffer)document).Remove (0, document.Length);
			LineSegment line = document.GetLine (1);
			Assert.AreEqual (0, line.Offset);
			Assert.AreEqual (0, line.Length);
			Assert.AreEqual (0, document.Length);
			Assert.AreEqual (1, document.LineCount);
		}
		
		[Test]
		public void TestDocumentBug1Test()
		{
			Document document = new Mono.TextEditor.Document ();
						
			string top    = "1234567890";
			document.Text = top;
			
			Assert.AreEqual (document.GetLine (1).Length, document.Length);
			
			((IBuffer)document).Remove(0, document.Length);
			
			LineSegment line = document.GetLine (1);
			Assert.AreEqual(0, line.Offset);
			Assert.AreEqual(0, line.Length);
			Assert.AreEqual(0, document.Length);
			Assert.AreEqual(1, document.LineCount);
		}
		
		[Test]
		public void TestDocumentBug2Test()
		{
			Document document = new Mono.TextEditor.Document ();
			
			string top      = "123\n456\n789\n0";
			string testText = "Hello World!";
			
			document.Text = top;
			
			((IBuffer)document).Insert (top.Length, testText);
			
			LineSegment line = document.GetLine (document.LineCount);
			
			Assert.AreEqual (top.Length - 1, line.Offset);
			Assert.AreEqual (testText.Length + 1, line.Length);
		}
		
		[Test]
		public void SplitterTest ()
		{
			Document document = new Mono.TextEditor.Document ();
			for (int i = 0; i < 100; i++) {
				((IBuffer)document).Insert (0, new string ('c', i) + Environment.NewLine);
			}
			Assert.AreEqual (101, document.LineCount);
			for (int i = 0; i < 100; i++) {
				LineSegment line = document.GetLine (i + 1 );
				Assert.AreEqual (99 - i, line.EditableLength);
				Assert.AreEqual (Environment.NewLine.Length, line.DelimiterLength);
			}
			
			for (int i = 0; i < 100; i++) {
				LineSegment line = document.GetLine (1);
				((IBuffer)document).Remove (line.EditableLength, line.DelimiterLength);
			}
			Assert.AreEqual (1, document.LineCount);
		}

		[Test]
		public void TestDiffWithNoChangesReturnsNothing ()
		{
			string docText = "Some string here";
			IEnumerable<Hunk> hunks = GenerateDiffHunks(docText, docText);
			int count = 0;
			
			foreach (Hunk hunk in hunks) {
				count++;
			}
			
			Assert.That (count, Is.EqualTo (0));
		}

		[Test]
		public void TestDiffWithOneAdditionReturnsOneHunk ()
		{
			string docText = "Some string here";
			IEnumerable<Hunk> hunks = GenerateDiffHunks(docText + "extra", docText);
			int count = 0;
			
			foreach (Hunk hunk in hunks) {
				count++;
			}
			
			Assert.That (count, Is.EqualTo (1));
		}

		[Test]
		public void TestDiffWithOneChangeReturnsExpectedHunk ()
		{
			string docText = "Some string here";
			string extra = "extra";
			IEnumerable<Hunk> hunks = GenerateDiffHunks(docText + extra, docText);
			IEnumerator<Mono.TextEditor.Utils.Hunk> enumerator = hunks.GetEnumerator ();
			enumerator.MoveNext();
			Hunk hunk = enumerator.Current;
			Assert.That(hunk.Inserted, Is.EqualTo(1));
			Assert.That(hunk.InsertStart, Is.EqualTo(1));
			Assert.That(hunk.Removed, Is.EqualTo(1));
			Assert.That(hunk.RemoveStart, Is.EqualTo(1));
		}
		
		[Test]
		public void TestMultilineDiffWithOneAdditionReturnsOneHunk ()
		{
			string docText = "Some string here\nSomeOtherText";
			string extra = "extra";
			IEnumerable<Hunk> hunks = GenerateDiffHunks(docText + extra, docText);
			int count = 0;
			
			foreach (Hunk hunk in hunks) {
				count++;
			}
			
			Assert.That (count, Is.EqualTo (1));
		}
		
		[Test]
		public void TestMultilineDiffWithOneAdditionReturnsExpectedHunk ()
		{
			string docText = "Some string here\nSomeOtherText";
			string extra = "extra";
			IEnumerable<Hunk> hunks = GenerateDiffHunks(docText + extra, docText);
			IEnumerator<Mono.TextEditor.Utils.Hunk> enumerator = hunks.GetEnumerator ();
			enumerator.MoveNext();
			Hunk hunk = enumerator.Current;
			Assert.That(hunk.Inserted, Is.EqualTo(1));
			Assert.That(hunk.InsertStart, Is.EqualTo(2));
			Assert.That(hunk.Removed, Is.EqualTo(1));
			Assert.That(hunk.RemoveStart, Is.EqualTo(2));
		}
		
		[Test]
		public void TestMultilineDiffWithAddedLineReturnsOneHunk ()
		{
			string docText = "Some string here\nSomeOtherText";
			string extra = "\nThirdLine";
			IEnumerable<Hunk> hunks = GenerateDiffHunks(docText + extra, docText);
			int count = 0;
			
			foreach (Hunk hunk in hunks) {
				count++;
			}
			
			Assert.That (count, Is.EqualTo (1));
		}
		
		[Test]
		public void TestMultilineDiffWithAddedLineReturnsExpectedHunk ()
		{
			string docText = "Some string here\nSomeOtherText";
			string extra = "\nThirdLine";
			IEnumerable<Hunk> hunks = GenerateDiffHunks(docText + extra, docText);
			IEnumerator<Mono.TextEditor.Utils.Hunk> enumerator = hunks.GetEnumerator ();
			enumerator.MoveNext();
			Hunk hunk = enumerator.Current;
			Assert.That(hunk.Inserted, Is.EqualTo(1));
			Assert.That(hunk.InsertStart, Is.EqualTo(3));
			Assert.That(hunk.Removed, Is.EqualTo(0));
			Assert.That(hunk.RemoveStart, Is.EqualTo(3));
		}
		
		[Test]
		public void TestChangeInMiddleOfMultiLineReturnsOneHunk()
		{
			string text = "12345678\n" +
			"1234567\n" +
			"123456\n" +
			"12345\n" +
			"1234\n" +
			"123\n" +
			"12\n" +
			"1\n" +
			"\n";
			IEnumerable<Hunk> hunks = GenerateDiffHunks(text + "middle line\n" + text, text + text);			
			int count = 0;
			
			foreach (Hunk hunk in hunks) {
				count++;
			}
			
			Assert.That (count, Is.EqualTo (1));
		}
		
		[Test]
		public void TestChangeInMiddleOfMultiLineReturnsExpectedHunk()
		{
			string text = "12345678\n" +
			"1234567\n" +
			"123456\n" +
			"12345\n" +
			"1234\n" +
			"123\n" +
			"12\n" +
			"1\n" +
			"\n";
			IEnumerable<Hunk> hunks = GenerateDiffHunks(text + "middle line\n" + text, text + text);
			IEnumerator<Mono.TextEditor.Utils.Hunk> enumerator = hunks.GetEnumerator ();
			enumerator.MoveNext();
			Hunk hunk = enumerator.Current;
			Assert.That(hunk.Inserted, Is.EqualTo(1));
			Assert.That(hunk.InsertStart, Is.EqualTo(10));
			Assert.That(hunk.Removed, Is.EqualTo(0));
			Assert.That(hunk.RemoveStart, Is.EqualTo(10));
		}
		
		private IEnumerable<Hunk> GenerateDiffHunks(string currentDocText, string originalText)
		{
			Document document = new Document (originalText);
			Document changedDocument = new Document (currentDocText);
			return document.Diff (changedDocument);
		}

		[TestFixtureSetUp] 
		public void SetUp()
		{
			Gtk.Application.Init ();
		}
		
		[TestFixtureTearDown] 
		public void Dispose()
		{
		}
		
		
	}
}
