//
// InternalLogPad.cs
//
// Author:
//   Lluis Sanchez Gual
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

using System;
using System.Text;
using System.Drawing;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

using MonoDevelop.Core;
using MonoDevelop.Core.Logging;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Tasks;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Commands;

using Gtk;
using MonoDevelop.Components.Docking;

namespace MonoDevelop.Ide.Gui.Pads
{
	internal class InternalLogPad : IPadContent, ILogger
	{
		Widget control;
		ScrolledWindow sw;
		MonoDevelop.Ide.Gui.Components.PadTreeView view;
		ListStore store;
		TreeModelFilter filter;
		ToggleButton errorBtn, warnBtn, msgBtn, debugBtn;
		IPadWindow window;
		bool needsReload;

		Clipboard clipboard;

		Gdk.Pixbuf iconWarning;
		Gdk.Pixbuf iconError;
		Gdk.Pixbuf iconInfo;
		Gdk.Pixbuf iconDebug;
		
		const string showErrorsPropertyName = "MonoDevelop.LogList.ShowErrors";
		const string showWarningsPropertyName = "MonoDevelop.LogList.ShowWarnings";
		const string showMessagesPropertyName = "MonoDevelop.LogList.ShowMessages";
		const string showDebugPropertyName = "MonoDevelop.LogList.ShowDebug";

		enum Columns
		{
			Type,
			Description,
			Time,
			TypeString,
			Message
		}

		public Gtk.Widget Control {
			get {
				return control;
			}
		}

		public string Id {
			get { return "MonoDevelop.Ide.Gui.Pads.InternalLogPad"; }
		}
		
		public void RedrawContent()
		{
			// FIXME
		}

		public void Initialize (IPadWindow window)
		{
			this.window = window;
			window.PadShown += delegate {
				if (needsReload)
					Refresh ();
			};
			
			DockItemToolbar toolbar = window.GetToolbar (PositionType.Top);
			
			errorBtn = new ToggleButton ();
			errorBtn.Active = (bool)PropertyService.Get (showErrorsPropertyName, true);
			string errorTipText;
			if ((InternalLog.EnabledLoggingLevel & EnabledLoggingLevel.Error) != EnabledLoggingLevel.Error
				&& (InternalLog.EnabledLoggingLevel & EnabledLoggingLevel.Fatal) != EnabledLoggingLevel.Fatal) {
				errorBtn.Sensitive = false;
				errorTipText = GettextCatalog.GetString ("Logging of errors is not enabled");
			} else {
				errorTipText = GettextCatalog.GetString ("Show errors");
			}
			errorBtn.Image = new Gtk.Image (Gtk.Stock.DialogError, Gtk.IconSize.Menu);
			errorBtn.Image.Show ();
			errorBtn.Toggled += new EventHandler (FilterChanged);
			errorBtn.TooltipText = errorTipText;
			UpdateErrorsNum();
			toolbar.Add (errorBtn);
			
			warnBtn = new ToggleButton ();
			warnBtn.Active = (bool)PropertyService.Get (showWarningsPropertyName, true);
			string warnTipText;
			if ((InternalLog.EnabledLoggingLevel & EnabledLoggingLevel.Warn) != EnabledLoggingLevel.Warn) {
				warnBtn.Sensitive = false;
				warnTipText = GettextCatalog.GetString ("Logging of warnings is not enabled");
			} else {
				warnTipText = GettextCatalog.GetString ("Show warnings");
			}
			warnBtn.Image = new Gtk.Image (Gtk.Stock.DialogWarning, Gtk.IconSize.Menu);
			warnBtn.Image.Show ();
			warnBtn.Toggled += new EventHandler (FilterChanged);
			warnBtn.TooltipText = warnTipText;
			UpdateWarningsNum();
			toolbar.Add (warnBtn);
			
			msgBtn = new ToggleButton ();
			msgBtn.Active = (bool)PropertyService.Get (showMessagesPropertyName, true);
			string msgTipText;
			if ((InternalLog.EnabledLoggingLevel & EnabledLoggingLevel.Info) != EnabledLoggingLevel.Info) {
				msgBtn.Sensitive = false;
				msgTipText = GettextCatalog.GetString ("Logging of informational messages is not enabled");
			} else {
				msgTipText = GettextCatalog.GetString ("Show messages");
			}
			msgBtn.Image = new Gtk.Image (Gtk.Stock.DialogInfo, Gtk.IconSize.Menu);
			msgBtn.Image.Show ();
			msgBtn.Toggled += new EventHandler (FilterChanged);
			msgBtn.TooltipText = msgTipText;
			UpdateMessagesNum();
			toolbar.Add (msgBtn);
			
			debugBtn = new ToggleButton ();
			debugBtn.Active = (bool)PropertyService.Get (showDebugPropertyName, true);
			string debugTipText;
			if ((InternalLog.EnabledLoggingLevel & EnabledLoggingLevel.Debug) != EnabledLoggingLevel.Debug) {
				debugBtn.Sensitive = false;
				debugTipText = GettextCatalog.GetString ("Logging of debug messages is not enabled");
			} else {
				debugTipText = GettextCatalog.GetString ("Show debug");
			}
			debugBtn.Image = new Gtk.Image (Gtk.Stock.DialogQuestion, Gtk.IconSize.Menu);
			debugBtn.Image.Show ();
			debugBtn.Toggled += new EventHandler (FilterChanged);
			debugBtn.TooltipText = debugTipText;
			UpdateDebugNum();
			toolbar.Add (debugBtn);
			
			toolbar.Add (new SeparatorToolItem ());
			
			Gtk.Button clearBtn = new Gtk.Button (new Gtk.Image (Gtk.Stock.Clear, Gtk.IconSize.Menu));
			clearBtn.Clicked += new EventHandler (OnClearList);
			toolbar.Add (clearBtn);
			toolbar.ShowAll ();

			// Content
			
			store = new Gtk.ListStore (typeof (Gdk.Pixbuf),      // image - type
			                           typeof (string),          // desc
			                           typeof (string),          // time
			                           typeof (string),          // type string
			                           typeof (LogMessage));     // message

			TreeModelFilterVisibleFunc filterFunct = new TreeModelFilterVisibleFunc (FilterTaskTypes);
			filter = new TreeModelFilter (store, null);
            filter.VisibleFunc = filterFunct;
			
			view = new MonoDevelop.Ide.Gui.Components.PadTreeView (new Gtk.TreeModelSort (filter));
			view.RulesHint = true;
			view.HeadersClickable = true;
			view.Selection.Mode = SelectionMode.Multiple;
			
			view.DoPopupMenu = (evt) =>
				IdeApp.CommandService.ShowContextMenu (view, evt, new CommandEntrySet () {
					new CommandEntry (EditCommands.Copy),
					new CommandEntry (EditCommands.SelectAll),
				});
			
			AddColumns ();
			
			sw = new Gtk.ScrolledWindow ();
			sw.ShadowType = ShadowType.None;
			sw.Add (view);
			
			LoggingService.AddLogger (this);
						
			iconWarning = sw.RenderIcon (Gtk.Stock.DialogWarning, Gtk.IconSize.Menu, "");
			iconError = sw.RenderIcon (Gtk.Stock.DialogError, Gtk.IconSize.Menu, "");
			iconInfo = sw.RenderIcon (Gtk.Stock.DialogInfo, Gtk.IconSize.Menu, "");
			iconDebug = sw.RenderIcon (Gtk.Stock.DialogQuestion, Gtk.IconSize.Menu, "");
			
			control = sw;
			sw.ShowAll ();
			
			Refresh ();

			store.SetSortFunc ((int)Columns.Time, TimeSortFunc);
			((TreeModelSort)view.Model).SetSortColumnId ((int)Columns.Time, SortType.Descending);
		}

			
		void Refresh ()
		{
			store.Clear ();
			lock (InternalLog.Messages) {
				// Load existing messages
				foreach (LogMessage msg in InternalLog.Messages) {
					AddMessage (msg);
				}
			}
			needsReload = false;
		}

		void AddColumns ()
		{
			Gtk.CellRendererPixbuf iconRender = new Gtk.CellRendererPixbuf ();
			iconRender.Yalign = 0;
			iconRender.Ypad = 2;
			view.TextRenderer.Yalign = 0;

			TreeViewColumn col;
			col = view.AppendColumn ("", iconRender, "pixbuf", Columns.Type);
			col.SortColumnId = (int) Columns.TypeString;
			col = view.AppendColumn (GettextCatalog.GetString ("Time"), view.TextRenderer, "text", Columns.Time);
			col.SortColumnId = (int) Columns.Time;
			col = view.AppendColumn (GettextCatalog.GetString ("Description"), view.TextRenderer, "text", Columns.Description);
			col.SortColumnId = (int) Columns.Description;
		}

		[CommandHandler (EditCommands.SelectAll)]
		internal void OnSelectAll ()
		{
			view.Selection.SelectAll ();
		}
		
		[CommandHandler (EditCommands.Copy)]
		internal void OnCopy ()
		{
			TreeModel model;
			StringBuilder txt = new StringBuilder ();
			foreach (Gtk.TreePath p in view.Selection.GetSelectedRows (out model)) {
				TreeIter it;
				if (!model.GetIter (out it, p))
					continue;
				LogMessage msg = (LogMessage) model.GetValue (it, (int) Columns.Message);
				if (txt.Length > 0)
					txt.Append ('\n');
				txt.AppendFormat ("{0} - {1} - {2}", msg.Level, msg.TimeStamp.ToLongTimeString (), msg.Message);
			}
			clipboard = Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", false));
			clipboard.Text = txt.ToString ();
			clipboard = Clipboard.Get (Gdk.Atom.Intern ("PRIMARY", false));
			clipboard.Text = txt.ToString ();
		}

		public void Dispose ()
		{
			LoggingService.RemoveLogger (((ILogger)this).Name);
		}
		
		void FilterChanged (object sender, EventArgs e)
		{
			PropertyService.Set (showErrorsPropertyName, errorBtn.Active);
			PropertyService.Set (showWarningsPropertyName, warnBtn.Active);
			PropertyService.Set (showMessagesPropertyName, msgBtn.Active);
			PropertyService.Set (showDebugPropertyName, debugBtn.Active);
			
			filter.Refilter ();
		}

		bool FilterTaskTypes (TreeModel model, TreeIter iter)
		{
			try {
				LogMessage msg = (LogMessage) store.GetValue (iter, (int)Columns.Message);
				if (msg == null)
					return true;
				if ((msg.Level == LogLevel.Error || msg.Level == LogLevel.Fatal) && errorBtn.Active)
					return true;
				else if (msg.Level == LogLevel.Warn && warnBtn.Active)
					return true;
				else if (msg.Level == LogLevel.Info && msgBtn.Active)
					return true;
				else if (msg.Level == LogLevel.Debug && debugBtn.Active)
					return true;
			} catch {
				//Not yet fully added
			}
			return false;
		}

		public void OnClearList (object sender, EventArgs e)
		{
			InternalLog.Reset ();
			store.Clear ();
			UpdateErrorsNum ();
			UpdateWarningsNum ();
			UpdateMessagesNum ();
			UpdateDebugNum ();
		}
		
		public void AddMessage (LogMessage message)
		{
			Gdk.Pixbuf stock;
			
			switch (message.Level) {
				case LogLevel.Fatal:
				case LogLevel.Error:
					stock = iconError;
					UpdateErrorsNum ();
					break; 
				case LogLevel.Warn:
					stock = iconWarning;
					UpdateWarningsNum ();	
					break;
				case LogLevel.Info:
					stock = iconInfo;
					UpdateWarningsNum ();	
					break;
				case LogLevel.Debug:
					stock = iconDebug;
					UpdateDebugNum ();
					break;
				default:
					stock = iconDebug;
					break;
			}

			store.AppendValues (stock,
			                    message.Message,
			                    message.TimeStamp.ToLongTimeString (),
			                    message.Level.ToString (),
			                    message);
			filter.Refilter ();
		}

		void UpdateErrorsNum () 
		{
			errorBtn.Label = " " + string.Format(GettextCatalog.GetPluralString("{0} Error", "{0} Errors", InternalLog.ErrorCount), InternalLog.ErrorCount);
			errorBtn.Image.Show ();
		}

		void UpdateWarningsNum ()
		{
			warnBtn.Label = " " + string.Format(GettextCatalog.GetPluralString("{0} Warning", "{0} Warnings", InternalLog.WarningCount), InternalLog.WarningCount); 
			warnBtn.Image.Show ();
		}

		void UpdateMessagesNum ()
		{
			msgBtn.Label = " " + string.Format(GettextCatalog.GetPluralString("{0} Message", "{0} Messages", InternalLog.InfoCount), InternalLog.InfoCount);
			msgBtn.Image.Show ();
		}

		void UpdateDebugNum ()
		{
			debugBtn.Label = " " + string.Format(GettextCatalog.GetString("{0} Debug", InternalLog.DebugCount));
			debugBtn.Image.Show ();
		}

		private int TimeSortFunc (TreeModel model, TreeIter iter1, TreeIter iter2)
		{
			LogMessage m1 = (LogMessage) model.GetValue (iter1, (int)Columns.Message);
			LogMessage m2 = (LogMessage) model.GetValue (iter2, (int)Columns.Message);

			if (m1 == null || m2 == null)
				return 0;
			
			SortType order;
			int sid;
			store.GetSortColumnId (out sid, out order);
			
			if (order == SortType.Ascending)
				return DateTime.Compare (m1.TimeStamp, m2.TimeStamp);
			else
				return DateTime.Compare (m2.TimeStamp, m1.TimeStamp);
		}
		
#region ILogger	implementation
		
		void ILogger.Log (LogLevel level, string message)
		{
			if (window != null && window.Visible) {
				LogMessage msg = new LogMessage (level, message);
				Gtk.Application.Invoke (delegate {
					AddMessage (msg);
				});
			} else {
				needsReload = true;
			}
		}
		
		string ILogger.Name {
			get { return Id; }
		}

		EnabledLoggingLevel ILogger.EnabledLevel {
			get { return InternalLog.EnabledLoggingLevel; }
		}
		
#endregion
	}
}
