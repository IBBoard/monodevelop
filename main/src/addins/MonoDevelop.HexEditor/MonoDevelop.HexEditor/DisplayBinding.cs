// 
// DisplayBinding.cs
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

using System.IO;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.HexEditor
{
	public class HexEditorDisplayBinding : ViewDisplayBinding
	{
		public override string Name {
			get {
				return GettextCatalog.GetString ("Hex Editor");
			}
		}
		
		public override MonoDevelop.Ide.Gui.IViewContent CreateContentForFile (string fileName)
		{
			return new HexEditorView ();
		}

		public override bool CanHandleMimeType (string mimetype)
		{
			return true;
		}

		public override MonoDevelop.Ide.Gui.IViewContent CreateContentForMimeType (string mimeType, System.IO.Stream content)
		{
			HexEditorView result = new HexEditorView ();
/*			result.Document.MimeType = mimeType;
			if (content != null) {
				using (StreamReader reader = new StreamReader (content)) {
					result.Document.Text = reader.ReadToEnd ();
				}
			}*/
			return result;
		}
		
		public override bool CanHandleFile (string filename)
		{
			return true;
		}
		
		public override bool CanUseAsDefault { get { return false; } } 
	}
}
