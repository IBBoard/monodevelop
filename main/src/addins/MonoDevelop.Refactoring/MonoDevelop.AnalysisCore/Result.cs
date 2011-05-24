// 
// Result.cs
//  
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (c) 2010 Novell, Inc.
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
using MonoDevelop.Projects.Dom;
using MonoDevelop.AnalysisCore.Extensions;

namespace MonoDevelop.AnalysisCore
{
	public class Result
	{
		public Result (DomRegion region, string message, ResultLevel level,
			ResultCertainty certainty, ResultImportance importance)
		{
			this.Region = region;
			this.Message = message;
			this.Level = level;
			this.Certainty = certainty;
			this.Importance = importance;
		}
		
		public string Message { get; private set; }
		public ResultLevel Level { get; private set; }
		public ResultCertainty Certainty { get; private set; }
		public ResultImportance Importance { get; private set; }
		public DomRegion Region { get; private set; }
		
		internal AnalysisRuleAddinNode Source { get; set; }
	}
	
	public enum ResultLevel
	{
		Error,
		Warning,
		Suggestion
	}
	
	public enum ResultCertainty
	{
		High,
		Medium,
		Low
	}
	
	public enum ResultImportance
	{
		High,
		Medium,
		Low
	}
}

