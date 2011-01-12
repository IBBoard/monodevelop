//
// DomMethodNodeBuilder.cs
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
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using MonoDevelop.Core;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Output;
using MonoDevelop.Ide.Gui.Components;
using Cecil.Decompiler.Languages;
using MonoDevelop.Ide;

namespace MonoDevelop.AssemblyBrowser
{
	class DomMethodNodeBuilder : AssemblyBrowserTypeNodeBuilder, IAssemblyBrowserNodeBuilder
	{
		public override Type NodeDataType {
			get { return typeof(IMethod); }
		}
		
		public DomMethodNodeBuilder (AssemblyBrowserWidget widget) : base (widget)
		{
			
		}
		
		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			IMethod method = (IMethod)dataObject;
			return method.FullName;
		}
		
		public static string FormatPrivate (string label)
		{
			return "<span foreground= \"#666666\">" + label + "</span>";	
		}
		
		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, ref string label, ref Gdk.Pixbuf icon, ref Gdk.Pixbuf closedIcon)
		{
			IMethod method = (IMethod)dataObject;
			
			label = Ambience.GetString (method, OutputFlags.ClassBrowserEntries  | OutputFlags.IncludeMarkup);
			if (method.IsPrivate || method.IsInternal)
				label = DomMethodNodeBuilder.FormatPrivate (label);
			
			icon = ImageService.GetPixbuf (method.StockIcon, Gtk.IconSize.Menu);
		}
		
		public override int CompareObjects (ITreeNavigator thisNode, ITreeNavigator otherNode)
		{
			if (otherNode.DataItem is BaseTypeFolder)
				return 1;
			if (otherNode.DataItem is IMethod)
				return ((IMethod)thisNode.DataItem).Name.CompareTo (((IMethod)otherNode.DataItem).Name);
			
			return -1;
		}
		
		#region IAssemblyBrowserNodeBuilder
		internal static void PrintDeclaringType (StringBuilder result, ITreeNavigator navigator)
		{
			IType type = (IType)navigator.GetParentDataItem (typeof (IType), false);
			if (type == null)
				return;
			
			result.Append (String.Format (GettextCatalog.GetString ("<b>Declaring Type:</b>\t{0}"), type.FullName));
			result.AppendLine ();
		}
		
		string IAssemblyBrowserNodeBuilder.GetDescription (ITreeNavigator navigator)
		{
			IMethod method = (IMethod)navigator.DataItem;
			StringBuilder result = new StringBuilder ();
			result.Append ("<span font_family=\"monospace\">");
			result.Append (Ambience.GetString (method, OutputFlags.AssemblyBrowserDescription));
			result.Append ("</span>");
			result.AppendLine ();
			PrintDeclaringType (result, navigator);
			DomTypeNodeBuilder.PrintAssembly (result, navigator);
			return result.ToString ();
		}
		
		static string GetInstructionOffset (Instruction instruction)
		{
			return String.Format ("IL_{0:X4}", instruction.Offset);
		}
		
		
		public static string Decompile (DomCecilMethod method, bool markup)
		{
			if (method.MethodDefinition.IsPInvokeImpl)
				return GettextCatalog.GetString ("Method is P/Invoke");
			if (method.MethodDefinition.Body == null) {
				IType type = method.DeclaringType;
				return type == null || type.ClassType == ClassType.Interface ? GettextCatalog.GetString ("Interface method") : GettextCatalog.GetString ("Abstract method");
			}
			
			StringBuilder result = new StringBuilder ();
			try {
				//ControlFlowGraph controlFlowGraph = ControlFlowGraph.Create (method.MethodDefinition);
				ILanguage lang = CSharp.GetLanguage (CSharpVersion.V3);
				ColoredCSharpFormatter formatter = new ColoredCSharpFormatter ();
				ILanguageWriter langWriter = lang.GetWriter (formatter);
				langWriter.Write (method.MethodDefinition);
				result.Append (formatter.Text);
			} catch (Exception e) {
				result.Append ("Decompilation failed: \n" + e);
			}
			return result.ToString ();
		}
		
		internal static string GetAttributes (Ambience ambience, IEnumerable<IAttribute> attributes)
		{
			StringBuilder result = new StringBuilder ();
			foreach (IAttribute attr in attributes) {
				if (result.Length > 0)
					result.AppendLine ();
				result.Append (ambience.GetString (attr, OutputFlags.AssemblyBrowserDescription));
			}
			if (result.Length > 0)
				result.AppendLine ();
			return result.ToString ();
		}
		
		public string GetDecompiledCode (ITreeNavigator navigator)
		{
			DomCecilMethod method = navigator.DataItem as DomCecilMethod;
			if (method == null)
				return "";
			
			StringBuilder result = new StringBuilder ();
/*			result.Append (GetAttributes (Ambience, method.Attributes));
			result.Append (Ambience.GetString (method, DomTypeNodeBuilder.settings));
			result.AppendLine ();
			result.Append ("{");
			result.AppendLine ();*/
			result.Append (Decompile (method, true));
//			result.Append ("}");
			return result.ToString ();
		}
		
		static void AppendLink (StringBuilder sb, string link, string text)
		{
			sb.Append ("<span style=\"text.link\"><u><a ref=\"");
			sb.Append (AssemblyBrowserWidget.FormatText (link.Replace ("<", "").Replace (">", "")));
			sb.Append ("\">");
			sb.Append (AssemblyBrowserWidget.FormatText (text.Replace ("::", ".").Replace ("<", "").Replace (">", "")));
			sb.Append ("</a></u></span>");
		}
		
		public static string Disassemble (DomCecilMethod method, bool markup)
		{
			if (method.MethodDefinition.IsPInvokeImpl)
				return GettextCatalog.GetString ("Method is P/Invoke");
			if (method.MethodDefinition.Body == null) {
				IType type = method.DeclaringType;
				return type == null || type.ClassType == ClassType.Interface ? GettextCatalog.GetString ("Interface method") : GettextCatalog.GetString ("Abstract method");
			}
			
			StringBuilder result = new StringBuilder ();
			foreach (Instruction instruction in method.MethodDefinition.Body.Instructions ) {
				if (markup)
					result.Append ("<b>");
				result.Append (GetInstructionOffset (instruction));
				result.Append (markup ? ":</b> " : ": ");
				result.Append (instruction.OpCode);
				if (markup)
					result.Append ("<i>");
				if (instruction.Operand != null) {
					result.Append (' ');
					if (instruction.Operand is string) {
						result.Append ('"');
						result.Append (AssemblyBrowserWidget.FormatText (instruction.Operand.ToString ()));
						result.Append ('"');
					} else if (instruction.Operand is Mono.Cecil.Cil.Instruction) {
						result.Append (GetInstructionOffset ((Mono.Cecil.Cil.Instruction)instruction.Operand));
					} else if (instruction.Operand is Mono.Cecil.TypeDefinition) {
						AppendLink (result, 
						            new DomCecilType ((Mono.Cecil.TypeDefinition)instruction.Operand).HelpUrl,
						            instruction.Operand.ToString ());
					} else if (instruction.Operand is Mono.Cecil.MethodDefinition) {
						Mono.Cecil.MethodDefinition md = instruction.Operand as Mono.Cecil.MethodDefinition;
						AppendLink (result, 
						            new DomCecilMethod (md) { DeclaringType = new DomCecilType (md.DeclaringType) } .HelpUrl, 
						            instruction.Operand.ToString ());
					} else if (instruction.Operand is Mono.Cecil.FieldDefinition) {
						Mono.Cecil.FieldDefinition fd = instruction.Operand as Mono.Cecil.FieldDefinition;
						AppendLink (result, 
						            new DomCecilField (fd) { DeclaringType = new DomCecilType (fd.DeclaringType) }.HelpUrl,  
						            instruction.Operand.ToString ());
					} else if (instruction.Operand is Mono.Cecil.PropertyDefinition) {
						Mono.Cecil.PropertyDefinition pd = instruction.Operand as Mono.Cecil.PropertyDefinition;
						AppendLink (result, 
						            new DomCecilProperty (pd) { DeclaringType = new DomCecilType (pd.DeclaringType) } .HelpUrl, 
						            instruction.Operand.ToString ());
					} else if (instruction.Operand is Mono.Cecil.TypeReference) {
						AppendLink (result, 
						            "T:" + ((TypeReference)instruction.Operand).FullName,
						            instruction.Operand.ToString ());
					} else if (instruction.Operand is Mono.Cecil.MethodReference) {
						Mono.Cecil.MethodReference mr = instruction.Operand as Mono.Cecil.MethodReference;
						StringBuilder id = new StringBuilder (mr.DeclaringType.ToString ());
						bool isConstructor = mr.Name == ".ctor";
						if (!isConstructor)
							id.Append ("." + mr.Name);
						id.Append ("(");
						for (int i = 0; i < mr.Parameters.Count; i++) {
							if (i > 0)
								id.Append (',');
							id.Append (mr.Parameters[i].ParameterType.FullName);
						}
						id.Append (")");
						AppendLink (result, (isConstructor ? "C:" : "M:") + id, id.ToString ());
					} else if (instruction.Operand is Mono.Cecil.FieldReference) {
						Mono.Cecil.FieldReference fr = instruction.Operand as Mono.Cecil.FieldReference;
						string id = fr.DeclaringType + "." + fr.Name;
						AppendLink (result, "F:" + id, id);
					} else if (instruction.Operand is Mono.Cecil.PropertyReference) {
						Mono.Cecil.PropertyReference pr = instruction.Operand as Mono.Cecil.PropertyReference;
						string id = pr.DeclaringType + "." + pr.Name;
						AppendLink (result, "P:" + id, id);
					} else {
						result.Append (AssemblyBrowserWidget.FormatText (instruction.Operand.ToString ()));
					}
				}
				if (markup)
					result.Append ("</i>");
				result.AppendLine ();
			}
			result.AppendLine ();
			return result.ToString ();
		}
		
		string IAssemblyBrowserNodeBuilder.GetDisassembly (ITreeNavigator navigator)
		{
			DomCecilMethod method = navigator.DataItem as DomCecilMethod;
			if (method == null)
				return "";

			NetAmbience netAmbience = new NetAmbience ();
			StringBuilder result = new StringBuilder ();
			var settings = DomTypeNodeBuilder.settings;
			settings.OutputFlags |= OutputFlags.IncludeConstraints;
			result.Append (netAmbience.GetString (method, settings));
			settings.OutputFlags &= ~OutputFlags.IncludeConstraints;
			result.AppendLine ();
			result.AppendLine ();
			result.Append (Disassemble (method, true));
			return result.ToString ();
		}
		
		string IAssemblyBrowserNodeBuilder.GetDocumentationMarkup (ITreeNavigator navigator)
		{
			DomCecilMethod method = navigator.DataItem as DomCecilMethod;
			StringBuilder result = new StringBuilder ();
			result.Append ("<big>");
			result.Append (Ambience.GetString (method, OutputFlags.AssemblyBrowserDescription | OutputFlags.IncludeConstraints));
			result.Append ("</big>");
			result.AppendLine ();
			
			AmbienceService.DocumentationFormatOptions options = new AmbienceService.DocumentationFormatOptions ();
			options.MaxLineLength = -1;
			options.BigHeadings = true;
			options.Ambience = Ambience;
			result.AppendLine ();
			
			result.Append (AmbienceService.GetDocumentationMarkup (AmbienceService.GetDocumentation (method), options));
			
			return result.ToString ();
		}
		#endregion

	}
}
