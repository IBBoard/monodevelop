//
// ClassBrowserPadWidget.cs
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
using System.Threading;
using System.Collections.Generic;

using Gdk;
using Gtk;

using MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Components.Docking;
using ICSharpCode.NRefactory.TypeSystem;
using MonoDevelop.TypeSystem;

namespace MonoDevelop.Ide.Gui.Pads.ClassBrowser
{
	
	class ClassBrowserPadWidget : VBox
	{
		ExtensibleTreeView treeView;
		TreeView searchResultsTreeView = new Gtk.TreeView ();
		ListStore list;
		TreeModelSort model;
		Entry searchEntry;
		Button buttonSearch, buttonCancelSearch;
		Notebook notebook;
		
		
		bool isInBrowerMode = true;
		public ClassBrowserPadWidget (ExtensibleTreeView treeView, IPadWindow window)
		{
			this.treeView = treeView;
			
			DockItemToolbar searchBox = window.GetToolbar (PositionType.Top);
			
			searchEntry = new Entry ();
			searchBox.Add (searchEntry, true);
			buttonSearch = new Button (new Gtk.Image (Gtk.Stock.Find, IconSize.Menu));
			buttonSearch.Relief = ReliefStyle.None;
			buttonCancelSearch = new Button (new Gtk.Image (Gtk.Stock.Stop, IconSize.Menu));
			buttonCancelSearch.Relief = ReliefStyle.None;
			searchBox.Add (buttonSearch);
			searchBox.Add (buttonCancelSearch);
			searchBox.ShowAll ();
			
			notebook = new Notebook ();
			notebook.ShowTabs = false;
			notebook.ShowBorder = false;
			this.PackEnd (notebook, true, true, 0);
			
			notebook.AppendPage (treeView, null);
			ScrolledWindow scrolledWindow = new ScrolledWindow ();
			scrolledWindow.Add (searchResultsTreeView);
			notebook.AppendPage (scrolledWindow, null);
			
			list = new ListStore (new Type[] {
				typeof (Pixbuf),
				typeof (string),
				typeof (IType)
			});
			model = new TreeModelSort (list);
			searchResultsTreeView.Model = model;
			searchResultsTreeView.AppendColumn (string.Empty, new Gtk.CellRendererPixbuf (), "pixbuf", 0);
			searchResultsTreeView.AppendColumn (string.Empty, new Gtk.CellRendererText (), "text", 1);
			searchResultsTreeView.HeadersVisible = false;
			searchResultsTreeView.RowActivated += SearchRowActivated;
			IdeApp.Workspace.WorkspaceItemOpened += OnOpenCombine;
			IdeApp.Workspace.WorkspaceItemClosed += OnCloseCombine;
					
			this.searchEntry.Changed += SearchEntryChanged;
			this.buttonCancelSearch.Clicked += CancelSearchClicked;
			this.searchEntry.Activated += SearchClicked;
			this.searchEntry.KeyPressEvent += SearchEntryKeyPressEvent;
			this.buttonSearch.Clicked += SearchClicked;
			
			this.ShowAll ();
		}

		void SearchEntryKeyPressEvent(object o, KeyPressEventArgs args)
		{
			if (args.Event.Key == Gdk.Key.Escape)
				CancelSearchClicked (this, System.EventArgs.Empty);
		}
		
		List<ITypeDefinition> searchResults = new List<ITypeDefinition> ();
		Thread searchThread;
		object matchLock   = new object ();
		string matchString = string.Empty;

		void SearchEntryChanged (object sender, EventArgs e)
		{
			if (isInBrowerMode)
				return;
			PerformSearch ();
		}
		
		void SearchRowActivated (object sender, RowActivatedArgs args)
		{
			JumpToSelectedType ();
		}
		
		void JumpToSelectedType ()
		{
			TreeModel model;
			TreeIter iter;
			
			TreeSelection sel = this.searchResultsTreeView.Selection;
			if (sel.GetSelected (out model, out iter)) {
				IType type = (IType)model.GetValue (iter, 2);
				IdeApp.ProjectOperations.JumpToDeclaration (type);
			}
		}
		
		void PerformSearch ()
		{
			if (list == null)
				return;
			
		//	userSelecting = false;
			
			string toMatch = this.searchEntry.Text.ToUpper ();
				
			lock (matchLock) {
				matchString = toMatch;
			}
			
			lock (searchResults) {
				// Clean the results list
				searchResults.Clear ();
			}
			
			if (searchThread != null) {
				searchThread.Abort ();
				searchThread = null;
			}
			
			// Queuing this seems to prevent things getting
			// added from queued events after the clear.
			DispatchService.GuiDispatch (list.Clear);
			
			ThreadStart start = new ThreadStart (SearchThread);
			searchThread = new Thread (start) {
				Name = "Class pad search",
				IsBackground = true,
				Priority = ThreadPriority.Lowest,
			};
			searchThread.Start ();
		}
		
		bool ShouldAdd (IType type)
		{
			
			return matchString.Length > 0 && type.FullName.ToUpper ().Contains (matchString);
		}
				
		void SearchThread ()
		{
			if (!IdeApp.Workspace.IsOpen)
				return;
			foreach (Project project in IdeApp.Workspace.GetAllProjects ()) {
				var dom = TypeSystemService.GetCompilation (project);
				if (dom == null)
					continue;
				foreach (var type in dom.MainAssembly.TopLevelTypeDefinitions) {
					if (ShouldAdd (type)) {
						lock (searchResults) {
							searchResults.Add (type);
							GLib.Idle.Add (AddItemGui);
						}
					}
				}
			}
		}
		
		bool AddItemGui ()
		{
			// Add items to the tree. Do it in blocks of 50, to avoid freezing
			// the GUI when there are a lot of items to add.
			lock (searchResults) {
				int max = Math.Min (50, searchResults.Count);
				for (int i = 0; i < max; i++) {
					list.AppendValues (ImageService.GetPixbuf (searchResults[i].GetStockIcon (), Gtk.IconSize.Menu), searchResults[i].Name, searchResults[i]);
				}
				searchResults.RemoveRange (0, max);
				return searchResults.Count > 0;
			}
		}
				
		void SearchClicked (object sender, EventArgs e)
		{
			if (!isInBrowerMode)
				return;
			this.notebook.Page = 1;
			isInBrowerMode = false;
			PerformSearch ();
		}
		
		void CancelSearchClicked (object sender, EventArgs e)
		{
			if (isInBrowerMode)
				return;
			this.notebook.Page = 0;
			isInBrowerMode = true;
		}
			
		void OnOpenCombine (object sender, WorkspaceItemEventArgs e)
		{
			treeView.LoadTree (e.Item);
		}
		
		void OnCloseCombine (object sender, WorkspaceItemEventArgs e)
		{
			treeView.Clear ();
		}		
	}
}
