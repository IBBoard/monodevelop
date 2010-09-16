//
// DomTypeNodeBuilder.cs
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
using System.Text;
using System.Linq;
using Mono.Cecil;

using MonoDevelop.Core;
using MonoDevelop.Projects.Dom;
using MonoDevelop.Projects.Dom.Output;
using MonoDevelop.Ide.Gui.Components;
using Mono.TextEditor.Highlighting;
using MonoDevelop.Ide;

namespace MonoDevelop.AssemblyBrowser
{
	class DomTypeNodeBuilder : AssemblyBrowserTypeNodeBuilder, IAssemblyBrowserNodeBuilder
	{
		public override Type NodeDataType {
			get { return typeof(IType); }
		}
		
		public override string ContextMenuAddinPath {
			get { return "/MonoDevelop/AssemblyBrowser/TypeNode/ContextMenu"; }
		}
		
		public DomTypeNodeBuilder (AssemblyBrowserWidget widget) : base (widget)
		{
			
		}
		
/*		AssemblyBrowserWidget widget;
		
		public DomTypeNodeBuilder (AssemblyBrowserWidget widget)
		{
			this.widget = widget;
		}*/
		internal static OutputSettings settings;
		static SyntaxMode mode = SyntaxModeService.GetSyntaxMode ("text/x-csharp");

		internal static string MarkupKeyword (string text)
		{
			foreach (Keywords words in mode.Keywords) {
				foreach (string word in words.Words) {
					if (word == text) {
						return "<span style=\"" + words.Color +  "\">" + text + "</span>";
					}
				}
			}
			return text;
		}
		
		static DomTypeNodeBuilder ()
		{
			DomTypeNodeBuilder.settings = new OutputSettings (OutputFlags.AssemblyBrowserDescription);
			
			DomTypeNodeBuilder.settings.MarkupCallback += delegate (string text) {
				return "<span style=\"text\">" + text + "</span>";
			};
			DomTypeNodeBuilder.settings.EmitModifiersCallback = delegate (string text) {
				return "<span style=\"keyword.modifier\">" + text + "</span>";
			};
			DomTypeNodeBuilder.settings.EmitKeywordCallback = delegate (string text) {
				return MarkupKeyword (text);
			};
			DomTypeNodeBuilder.settings.EmitNameCallback = delegate (INode domVisitable, ref string outString) {
				if (domVisitable is IType) {
					Console.WriteLine (((IType)domVisitable).HelpUrl);
					outString = "<span style=\"text.link\"><u><a ref=\"" + ((IType)domVisitable).HelpUrl + "\">" + outString + "</a></u></span>";
				} else {
					outString = "<span style=\"text\">" + outString + "</span>";
				}
			};
			DomTypeNodeBuilder.settings.PostProcessCallback = delegate (INode domVisitable, ref string outString) {
				if (domVisitable is IReturnType) {
					outString = "<span style=\"text.link\"><u><a ref=\"" + ((IReturnType)domVisitable).HelpUrl + "\">" + outString + "</a></u></span>";
				}
			};
		}
		
		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			IType type = (IType)dataObject;
			return type.Name;
		}
		
		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, ref string label, ref Gdk.Pixbuf icon, ref Gdk.Pixbuf closedIcon)
		{
			IType type = (IType)dataObject;
			label = Ambience.GetString (type, OutputFlags.ClassBrowserEntries  | OutputFlags.IncludeMarkup);
			if (type.IsPrivate || type.IsInternal)
				label = DomMethodNodeBuilder.FormatPrivate (label);
			icon = ImageService.GetPixbuf (type.StockIcon, Gtk.IconSize.Menu);
		}
		
		public override void BuildChildNodes (ITreeBuilder ctx, object dataObject)
		{
			IType type = (IType)dataObject;
			ctx.AddChild (new BaseTypeFolder (type));
			bool publicOnly = ctx.Options ["PublicApiOnly"];
			ctx.AddChildren (type.Members.Where (member => !(member.IsSpecialName && !(member is IMethod && ((IMethod)member).IsConstructor)) && !(publicOnly && !(member.IsPublic || member.IsProtected))));
		}
		
		public override bool HasChildNodes (ITreeBuilder builder, object dataObject)
		{
			return true;
		}
		
		#region IAssemblyBrowserNodeBuilder
		internal static void PrintAssembly (StringBuilder result, ITreeNavigator navigator)
		{
			AssemblyDefinition assemblyDefinition = (AssemblyDefinition)navigator.GetParentDataItem (typeof (AssemblyDefinition), false);
			if (assemblyDefinition == null)
				return;
			
			result.Append (String.Format (GettextCatalog.GetString ("<b>Assembly:</b>\t{0}, Version={1}"),
			                              assemblyDefinition.Name.Name,
			                              assemblyDefinition.Name.Version));
			result.AppendLine ();
		}
		
		public string GetDescription (ITreeNavigator navigator)
		{
			IType type = (IType)navigator.DataItem;
			StringBuilder result = new StringBuilder ();
			result.Append ("<span font_family=\"monospace\">");
			result.Append (Ambience.GetString (type, OutputFlags.AssemblyBrowserDescription));
			result.Append ("</span>");
			result.AppendLine ();
			result.Append (String.Format (GettextCatalog.GetString ("<b>Name:</b>\t{0}"),
			                              type.FullName));
			result.AppendLine ();
			PrintAssembly (result, navigator);
			return result.ToString ();
		}
		
		public string GetDisassembly (ITreeNavigator navigator)
		{
			bool publicOnly = navigator.Options ["PublicApiOnly"];
			IType type = (IType)navigator.DataItem;
			StringBuilder result = new StringBuilder ();
			result.Append (DomMethodNodeBuilder.GetAttributes (Ambience, type.Attributes));
			result.Append (Ambience.GetString (type, settings));
			bool first = true;
			
			if (type.ClassType == ClassType.Enum) {
				result.Append ("<span style=\"text\"> {</span>");
				result.Append ("");
				result.AppendLine ();
				int length = result.Length;
				foreach (IField field in type.Fields) {
					if ((field.Modifiers & Modifiers.SpecialName) == Modifiers.SpecialName)
						continue;
					result.Append ("<span style=\"text\"> \t");
					result.Append (field.Name);
					length = result.Length;
					result.Append (",</span>");
					result.AppendLine ();
				}
				result.Length = length;
				result.AppendLine ();
				result.Append ("<span style=\"text\">}</span>");
				return result.ToString ();
			}
			result.AppendLine ();
			result.Append ("<span style=\"text\">{</span>");
			
//			Style colorStyle = TextEditorOptions.Options.GetColorStyle (widget);
//			ChunkStyle comments = colorStyle.GetChunkStyle ("comment");
//			string commentSpan = String.Format ("<span foreground=\"#{0:X6}\">", comments.Color.Pixel);
			string commentSpan = "<span style=\"comment\">";
			foreach (IField field in type.Fields) {
				if (publicOnly && !(field.IsPublic || field.IsProtected))
					continue;
				if ((field.Modifiers & Modifiers.SpecialName) == Modifiers.SpecialName)
					continue;
				if (first) {
					result.AppendLine ();
					result.Append ("\t");
					result.Append (commentSpan);
					result.Append (Ambience.SingleLineComment (GettextCatalog.GetString ("Fields")));
					result.Append ("</span>");
					result.AppendLine ();
				}
				first = false;
				result.Append ("\t");
				result.Append (Ambience.GetString (field, settings));
				result.Append ("<span style=\"text\">;</span>");
				result.AppendLine ();
			}
			first = true;
			foreach (IEvent evt in type.Events) {
				if (publicOnly && !(evt.IsPublic || evt.IsProtected))
					continue;
				if (first) {
					result.AppendLine ();
					result.Append ("\t");
					result.Append (commentSpan);
					result.Append (Ambience.SingleLineComment (GettextCatalog.GetString ("Events")));
					result.Append ("</span>");
					result.AppendLine ();
				}
				first = false;
				result.Append ("\t");
				result.Append (Ambience.GetString (evt, settings));
				result.Append ("<span style=\"text\">;</span>");
				result.AppendLine ();
			}
			first = true;
			foreach (IMethod method in type.Methods) {
				if (publicOnly && !(method.IsPublic || method.IsProtected))
					continue;
				if (!method.IsConstructor)
					continue;
				if (first) {
					result.AppendLine ();
					result.Append ("\t");
					result.Append (commentSpan);
					result.Append (Ambience.SingleLineComment (GettextCatalog.GetString ("Constructors")));
					result.Append ("</span>");
					result.AppendLine ();
				}
				first = false;
				result.Append ("\t");
				result.Append (Ambience.GetString (method, settings));
				result.Append ("<span style=\"text\">;</span>");
				result.AppendLine ();
			}
			first = true;
			foreach (IMethod method in type.Methods) {
				if (publicOnly && !(method.IsPublic || method.IsProtected))
					continue;
				if ((method.Modifiers & Modifiers.SpecialName) == Modifiers.SpecialName || method.IsConstructor)
					continue;
				if (first) {
					result.AppendLine ();
					result.Append ("\t");
					result.Append (commentSpan);
					result.Append (Ambience.SingleLineComment (GettextCatalog.GetString ("Methods")));
					result.Append ("</span>");
					result.AppendLine ();
				}
				first = false;
				result.Append ("\t");
				result.Append (Ambience.GetString (method, settings));
				result.Append ("<span style=\"text\">;</span>");
				result.AppendLine ();
			}
			first = true;
			foreach (IProperty property in type.Properties) {
				if (publicOnly && !(property.IsPublic || property.IsProtected))
					continue;
				if (first) {
					result.AppendLine ();
					result.Append ("\t");
					result.Append (commentSpan);
					result.Append (Ambience.SingleLineComment (GettextCatalog.GetString ("Properties")));
					result.Append ("</span>");
					result.AppendLine ();
				}
				first = false;
				result.Append ("\t");
				result.Append (Ambience.GetString (property, settings));
				result.Append (" <span style=\"text\">{</span>");
				if (property.HasGet) {
					if (property.GetterModifier != property.Modifiers) {
						result.Append (" <span style=\"keyword.modifier\">");
						result.Append (Ambience.GetString (property.GetterModifier));
						result.Append ("</span>");
					}
					result.Append (" <span style=\"keyword.property\">get</span><span style=\"text\">;</span>");
				}
				if (property.HasSet) {
					if (property.SetterModifier != property.Modifiers) {
						result.Append (" <span style=\"keyword.modifier\">");
						result.Append (Ambience.GetString (property.SetterModifier));
						result.Append ("</span>");
					}
					result.Append (" <span style=\"keyword.property\">set</span><span style=\"text\">;</span>");
				}
				result.Append (" <span style=\"text\">}</span>");
				result.AppendLine ();
			}
			result.Append ("<span style=\"text\">}</span>");
			
			result.AppendLine ();
			return result.ToString ();
		}
		
		public string GetDecompiledCode (ITreeNavigator navigator)
		{
			IType type = (IType)navigator.DataItem;
			if (type.ClassType == ClassType.Delegate) {
				settings.OutputFlags |= OutputFlags.ReformatDelegates;
				string result =  Ambience.GetString (type, settings);
				settings.OutputFlags &= ~OutputFlags.ReformatDelegates;
				return result;
			}
			return GetDisassembly (navigator);
		}
		
		string IAssemblyBrowserNodeBuilder.GetDocumentationMarkup (ITreeNavigator navigator)
		{
			IType type = (IType)navigator.DataItem;
			StringBuilder result = new StringBuilder ();
			result.Append ("<big>");
			result.Append (Ambience.GetString (type, OutputFlags.AssemblyBrowserDescription));
			result.Append ("</big>");
			result.AppendLine ();
			
			AmbienceService.DocumentationFormatOptions options = new AmbienceService.DocumentationFormatOptions ();
			options.MaxLineLength = -1;
			options.BigHeadings = true;
			options.Ambience = Ambience;
			result.AppendLine ();
			
			result.Append (AmbienceService.GetDocumentationMarkup (AmbienceService.GetDocumentation (type), options));
			
			return result.ToString ();
		}
		#endregion
		
	}
}
