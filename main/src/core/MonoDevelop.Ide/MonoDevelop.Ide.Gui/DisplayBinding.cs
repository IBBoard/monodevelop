// DefaultDisplayBinding.cs
//
// Author:
//   Lluis Sanchez Gual <lluis@novell.com>
//
// Copyright (c) 2007 Novell, Inc (http://www.novell.com)
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
//
//


using System;
using MonoDevelop.Ide.Codons;
using System.IO;
using MonoDevelop.Ide.Desktop;

namespace MonoDevelop.Ide.Gui
{
	public abstract class ViewDisplayBinding: IViewDisplayBinding
	{
		public virtual bool CanHandleMimeType (string mimetype)
		{
			return false;
		}
		
		public virtual bool CanHandleFile (string filename)
		{
			return false;
		}

		public virtual IViewContent CreateContentForFile (string filename)
		{
			throw new NotSupportedException ();
		}
		
		public virtual IViewContent CreateContentForMimeType (string mimeType, System.IO.Stream content)
		{
			throw new NotSupportedException ();
		}
		
		public virtual bool CanUseAsDefault {
			get { return true; }
		}
		
		public abstract string Name { get; }
	}
	
	public abstract class ExternalDisplayBinding : IExternalDisplayBinding
	{
		public virtual bool CanHandleMimeType (string mimetype)
		{
			return false;
		}
		
		public virtual bool CanHandleFile (string filename)
		{
			return false;
		}
		
		public DesktopApplication GetApplicationForMimeType (string mimeType)
		{
			throw new NotSupportedException ();
		}
		
		public DesktopApplication GetApplicationForFile (string filename)
		{
			throw new NotSupportedException ();
		}
		
		public virtual bool CanUseAsDefault {
			get { return true; }
		}
	}
	
	//dummy binding, anchor point for extension tree
	class DefaultDisplayBinding : ExternalDisplayBinding
	{
	}
}
