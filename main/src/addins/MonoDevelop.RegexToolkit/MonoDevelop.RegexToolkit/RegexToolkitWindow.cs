//
// RegexToolkitWindow.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using Gdk;
using Gtk;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using MonoDevelop.Core;
using MonoDevelop.Ide;


namespace MonoDevelop.RegexToolkit
{
	public partial class RegexToolkitWindow : Gtk.Window
	{
		ListStore optionsStore;
		TreeStore resultStore;
		TreeStore elementsStore;
		RegexLibraryWindow regexLib;
		Thread regexThread;
			
			
		public RegexToolkitWindow () : base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.TransientFor = IdeApp.Workbench.RootWindow;
			optionsStore = new ListStore (typeof(bool), typeof(string), typeof(Options));
			resultStore = new Gtk.TreeStore (typeof(string), typeof(string), typeof(int), typeof(int));
			
			FillOptionsBox ();
			
			this.buttonCancel.Clicked += delegate {
				this.Destroy ();
			};
			
			this.buttonLibrary.Visible = false;
			this.buttonStart.Sensitive = false;
			this.entryRegEx.Changed += UpdateStartButtonSensitivity;
			this.inputTextview.Buffer.Changed += UpdateStartButtonSensitivity;
			
			this.buttonStart.Clicked += delegate {
				if (regexThread != null && regexThread.IsAlive) {
					regexThread.Abort ();
					regexThread.Join ();
					SetButtonStart (GettextCatalog.GetString ("_Start Regular Expression"), "gtk-media-play");
					regexThread = null;
					return;
				}
				
				regexThread = new Thread (delegate() {
					PerformQuery (inputTextview.Buffer.Text, this.entryRegEx.Text, this.entryReplace.Text, GetOptions ());
				});
				
				regexThread.IsBackground = true;
				regexThread.Name = "regex thread";
				regexThread.Start ();
				SetButtonStart (GettextCatalog.GetString ("_Stop execution"), "gtk-media-stop");
				
				SetFindMode (!checkbuttonReplace.Active);
			};
			
			this.buttonLibrary.Clicked += delegate {
				if (regexLib == null) {
					regexLib = new RegexLibraryWindow ();
					regexLib.TransientFor = this;
					regexLib.Destroyed += delegate {
						regexLib = null;
					};
					regexLib.Show ();
				}
			};
			
			SetFindMode (true);
			
			var cellRendText = new CellRendererText ();
			cellRendText.Ellipsize = Pango.EllipsizeMode.End;
			var pix = new CellRendererPixbuf ();
			
			this.optionsTreeview.Model = this.optionsStore;
			this.optionsTreeview.HeadersVisible = false;
			
			CellRendererToggle cellRendToggle = new CellRendererToggle ();
			cellRendToggle.Toggled += new ToggledHandler (OptionToggled);
			cellRendToggle.Activatable = true;
			this.optionsTreeview.AppendColumn ("", cellRendToggle, "active", 0);
			this.optionsTreeview.AppendColumn ("", cellRendText, "text", 1);
			
			this.resultsTreeview.Model = this.resultStore;
			this.resultsTreeview.HeadersVisible = false;
			var col = new TreeViewColumn ();
			this.resultsTreeview.AppendColumn (col);
			col.PackStart (pix, false);
			col.AddAttribute (pix, "stock_id", 0);
			col.PackStart (cellRendText, true);
			col.AddAttribute (cellRendText, "text", 1);
			
			this.resultsTreeview.RowActivated += delegate(object sender, RowActivatedArgs e) {
				Gtk.TreeIter iter;
				if (resultStore.GetIter (out iter, e.Path)) {
					int index = (int)resultStore.GetValue (iter, 2);
					int length = (int)resultStore.GetValue (iter, 3);
					if (index >= 0) {
						this.inputTextview.Buffer.SelectRange (this.inputTextview.Buffer.GetIterAtOffset (index),
						                                       this.inputTextview.Buffer.GetIterAtOffset (index + length));
					} else {
						this.inputTextview.Buffer.SelectRange (this.inputTextview.Buffer.GetIterAtOffset (0), this.inputTextview.Buffer.GetIterAtOffset (0));
					}
				}
			};
			
			elementsStore = new Gtk.TreeStore (typeof(string), typeof(string), typeof(string), typeof(string));
			this.elementsTreeview.Model = this.elementsStore;
			this.elementsTreeview.HeadersVisible = false;
			this.elementsTreeview.Selection.Mode = SelectionMode.Browse;
			
			col = new TreeViewColumn ();
			this.elementsTreeview.AppendColumn (col);
			col.PackStart (pix, false);
			col.AddAttribute (pix, "stock_id", 0);
			col.PackStart (cellRendText, true);
			
			col.AddAttribute (cellRendText, "text", 1);
			
			var cellRendText2 = new CellRendererText ();
			col.PackStart (cellRendText2, false);
			col.SetCellDataFunc (cellRendText2, ElementDescriptionFunc);
			
			this.elementsTreeview.Selection.Changed += delegate {
				ShowTooltipForSelectedEntry ();
			};
			
			this.LeaveNotifyEvent += delegate {
				this.HideTooltipWindow ();
			};
			
			
			this.elementsTreeview.MotionNotifyEvent += HandleMotionNotifyEvent;
			
			this.elementsTreeview.RowActivated += delegate (object sender, RowActivatedArgs e) {
				Gtk.TreeIter iter;
				if (elementsStore.GetIter (out iter, e.Path)) {
					string text = elementsStore.GetValue (iter, 3) as string;
					if (!System.String.IsNullOrEmpty (text)) {
						this.entryRegEx.InsertText (text);
					}
				}
			};
			this.entryReplace.Sensitive = this.checkbuttonReplace.Active = false;
			this.checkbuttonReplace.Toggled += delegate {
				this.entryReplace.Sensitive = this.checkbuttonReplace.Active;
			};
			FillElementsBox ();
			this.vbox4.Hide ();
		}

		void SetButtonStart (string text, string icon)
		{
			((Gtk.Label)((Gtk.HBox)((Gtk.Alignment)this.buttonStart.Child).Child).Children[1]).Text = text;
			((Gtk.Label)((Gtk.HBox)((Gtk.Alignment)this.buttonStart.Child).Child).Children[1]).UseUnderline = true;
			((Gtk.Image)((Gtk.HBox)((Gtk.Alignment)this.buttonStart.Child).Child).Children[0]).Pixbuf = global::Stetic.IconLoader.LoadIcon (this, icon, global::Gtk.IconSize.Menu);
		}

		
		void ElementDescriptionFunc (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			string str = (string)model.GetValue (iter, 2);
			if (string.IsNullOrEmpty (str)) {
				cell.Visible = false;
				return;
			}
			CellRendererText txtRenderer = (CellRendererText)cell;
			txtRenderer.Visible = true;
			txtRenderer.Text = str;
		}
		
		int ox = -1 , oy = -1;
		
		[GLib.ConnectBefore]
		void HandleMotionNotifyEvent(object o, MotionNotifyEventArgs args)
		{
			TreeIter iter;
				
				if (!elementsTreeview.Selection.GetSelected (out iter))
					return;
				Gdk.Rectangle rect = elementsTreeview.GetCellArea (elementsStore.GetPath (iter), elementsTreeview.GetColumn (0));
				int x, y;
				this.GdkWindow.GetOrigin (out x, out y);
				x += rect.X;
				y += rect.Y;
				if (this.tooltipWindow == null || ox != x || oy != y) {
					ShowTooltipForSelectedEntry ();
					ox = x;
					oy = y;
				}
		}
		
		void SetFindMode (bool findMode)
		{
			this.notebook2.ShowTabs = !findMode;
			if (findMode)
				this.notebook2.Page = 0;
		}
		
		void UpdateStartButtonSensitivity (object sender, EventArgs args)
		{
			this.buttonStart.Sensitive = this.entryRegEx.Text.Length > 0 && inputTextview.Buffer.CharCount > 0;
			labelStatus.Text = string.Empty;
		}
		
		void ShowTooltipForSelectedEntry ()
		{
			TreeIter iter;
			if (elementsTreeview.Selection.GetSelected (out iter)) {
				string description = elementsStore.GetValue (iter, 2) as string;
				if (!String.IsNullOrEmpty (description)) {
					Gdk.Rectangle rect = elementsTreeview.GetCellArea (elementsStore.GetPath (iter), elementsTreeview.GetColumn (0));
					int wx, wy, wy2; 
					elementsTreeview.TranslateCoordinates (this, rect.X, rect.Bottom, out wx, out wy);
					elementsTreeview.TranslateCoordinates (this, rect.X, rect.Y, out wx, out wy2);
					ShowTooltip (description, wx, wy, wy2);
				} else {
					HideTooltipWindow ();
				}
			} else {
				HideTooltipWindow ();
			}
		}
		
		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			if (optionsStore != null) {
				optionsStore.Dispose ();
				optionsStore = null;
			}
			if (resultStore != null) {
				resultStore.Dispose ();
				resultStore = null;
			}
			if (elementsStore != null) {
				elementsStore.Dispose ();
				elementsStore = null;
			}
			
			HideTooltipWindow ();
		}

		
		CustomTooltipWindow tooltipWindow = null;
		public void HideTooltipWindow ()
		{
			if (tooltipWindow != null) {
				tooltipWindow.Destroy ();
				tooltipWindow = null;
			}
		}
		const int tooltipXOffset = 100;
		public void ShowTooltip (string text, int x, int y, int altY)
		{
			if (tooltipWindow != null) {
				tooltipWindow.Hide ();
			} else {
				tooltipWindow = new CustomTooltipWindow ();
				tooltipWindow.TransientFor = this;
				tooltipWindow.DestroyWithParent = true;
			}
			tooltipWindow.Tooltip = text;
			int ox, oy;
			this.GdkWindow.GetOrigin (out ox, out oy);
			int w = tooltipWindow.Child.SizeRequest().Width;
			int h = tooltipWindow.Child.SizeRequest().Height;
			
			Gdk.Rectangle geometry = DesktopService.GetUsableMonitorGeometry (Screen, Screen.GetMonitorAtWindow (this.GdkWindow));
			
			if (ox + x + w + tooltipXOffset >= geometry.Right ||
			    oy + y + h >= geometry.Bottom) {
				tooltipWindow.Move (ox + x - w, oy + altY - h);
			} else 
				tooltipWindow.Move (ox + x + tooltipXOffset, oy + y);
			tooltipWindow.ShowAll ();
		}
			
		public class CustomTooltipWindow : MonoDevelop.Components.TooltipWindow
		{
			string tooltip;
			public string Tooltip {
				get {
					return tooltip;
				}
				set {
					tooltip = value;
					label.Text = tooltip;
				}
			}
			
			Label label = new Label ();
			public CustomTooltipWindow ()
			{
				label.Xalign = 0;
				label.Xpad = 3;
				label.Ypad = 3;
				Add (label);
			}
		}
		
		void PerformQuery (string input, string pattern, string replacement, RegexOptions options)
		{
			try {
				Regex regex = new Regex (pattern, options);
				Application.Invoke (delegate {
					this.resultStore.Clear ();
					foreach (Match match in regex.Matches (input)) {
						TreeIter iter = this.resultStore.AppendValues (Stock.Find, String.Format (GettextCatalog.GetString("Match '{0}'"), match.Value), match.Index, match.Length);
						int i = 0;
						foreach (Group group in match.Groups) {
							TreeIter groupIter;
							if (group.Success) {
								groupIter = this.resultStore.AppendValues (iter, Stock.Apply, String.Format (GettextCatalog.GetString("Group '{0}':'{1}'"), regex.GroupNameFromNumber (i), group.Value), group.Index, group.Length);
								foreach (Capture capture in match.Captures) {
									this.resultStore.AppendValues (groupIter, null, String.Format (GettextCatalog.GetString("Capture '{0}'"), capture.Value), capture.Index, capture.Length);
								}
							} else {
								groupIter = this.resultStore.AppendValues (iter, Stock.Cancel, String.Format (GettextCatalog.GetString("Group '{0}' not found"), regex.GroupNameFromNumber (i)), -1, -1);
							}
							i++;
						}
					}
					if (!String.IsNullOrEmpty (replacement))
						this.replaceResultTextview.Buffer.Text = regex.Replace (input, replacement);
				});
			} catch (ThreadAbortException) {
				Thread.ResetAbort ();
			} catch (ArgumentException) {
				Application.Invoke (delegate {
					labelStatus.Text = GettextCatalog.GetString ("Invalid expression");
				});
			} finally {
				regexThread = null;
				Application.Invoke (delegate {
					SetButtonStart (GettextCatalog.GetString ("_Start Regular Expression"), "gtk-media-play");
				});
			}
		}
		
		RegexOptions GetOptions ()
		{
			RegexOptions result = RegexOptions.None;
			Gtk.TreeIter iter;
			if (this.optionsStore.GetIterFirst (out iter)) { 
				do {
					bool toggled = (bool)this.optionsStore.GetValue (iter, 0);
					if (toggled) {
						result |= ((Options)this.optionsStore.GetValue (iter, 2)).RegexOptions; 
					}
				} while (this.optionsStore.IterNext (ref iter));
			}
			return result;
		}
		
		void OptionToggled (object sender, ToggledArgs e)
		{
			TreeIter iter;
			if (this.optionsStore.GetIterFromString (out iter, e.Path)) {
				bool toggled = (bool)this.optionsStore.GetValue (iter, 0);
				this.optionsStore.SetValue (iter, 0, !toggled);
			}
		}
		
		class Options 
		{
			RegexOptions options;
			string       name;
			
			public string Name {
				get {
					return name;
				}
			}
			
			public RegexOptions RegexOptions {
				get {
					return options;
				}
			}
			
			public Options (RegexOptions options, string name)
			{
				this.options = options;
				this.name    = name;
			}
		}
		
		void FillOptionsBox ()
		{
			Options[] options = {
				new Options (RegexOptions.IgnorePatternWhitespace, GettextCatalog.GetString("Ignore Whitespace")),
				new Options (RegexOptions.IgnoreCase, GettextCatalog.GetString("Ignore case")),
				new Options (RegexOptions.Singleline, GettextCatalog.GetString("Single line")),
				new Options (RegexOptions.Multiline, GettextCatalog.GetString("Multi line")),
				new Options (RegexOptions.ExplicitCapture, GettextCatalog.GetString("Explicit Capture")),
				new Options (RegexOptions.RightToLeft, GettextCatalog.GetString("Right to left"))
			};
			foreach (Options option in options) {
				this.optionsStore.AppendValues (false, option.Name, option);
			}
		}
		
		void FillElementsBox ()
		{
			Stream stream = typeof (RegexToolkitWindow).Assembly.GetManifestResourceStream ("RegexElements.xml");
			if (stream == null)
				return;
			XmlReader reader = new XmlTextReader (stream);
			while (reader.Read ()) {
				if (reader.NodeType != XmlNodeType.Element)
					continue;
				switch (reader.LocalName) {
				case "Group":
					TreeIter groupIter = this.elementsStore.AppendValues (Stock.Info,
						GettextCatalog.GetString (reader.GetAttribute ("_name")), "", "");
					while (reader.Read ()) {
						if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "Group") 
							break;
						switch (reader.LocalName) {
							case "Element":
								this.elementsStore.AppendValues (groupIter, null, 
							        	GettextCatalog.GetString (reader.GetAttribute ("_name")),
									GettextCatalog.GetString (reader.GetAttribute ("_description")),
									reader.ReadElementString ());
								break;
						}
					}
					break;
				}
			}
		}
	}
}