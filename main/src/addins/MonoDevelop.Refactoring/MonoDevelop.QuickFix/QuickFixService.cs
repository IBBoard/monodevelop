// 
// QuickFixService.cs
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
using System.Collections.Generic;
using MonoDevelop.Projects.Dom;
using System.Threading;
using System.Linq;
using MonoDevelop.Core;

namespace MonoDevelop.QuickFix
{
	public static class QuickFixService
	{
		static List<QuickFix> quickFixes = new List<QuickFix> ();
		
		
		static QuickFixService ()
		{
			quickFixes.Add (new ConvertDecToHexQuickFix ());
		}
		
		//TODO: proper job scheduler and discarding superseded jobs
		public static void QueueAnalysis (ParsedDocument doc, DomLocation loc, Action<List<QuickFix>> callback)
		{
			ThreadPool.QueueUserWorkItem (delegate {
				try {
					List<QuickFix > availableFixes = new List<QuickFix> (quickFixes.Where (fix => fix.IsValid (doc, loc)));
					callback (availableFixes);
				} catch (Exception ex) {
					LoggingService.LogError ("Error in analysis service", ex);
				}
			});
		}
	}
}

