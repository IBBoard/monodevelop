// 
// DocumentTypeWidget.cs
//  
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
// 
// Copyright (c) 2011 Xamarin <http://xamarin.com>
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
using MonoDevelop.Core.Collections;
using System.Text;
using System.Linq;
using Gtk;
using MonoDevelop.Ide.Projects;
using MonoDevelop.Projects;
using MonoDevelop.Core;
using MonoDevelop.Ide;

namespace MonoDevelop.MacDev.PlistEditor
{
	[System.ComponentModel.ToolboxItem(false)]
	public partial class DocumentTypeWidget : Gtk.Bin
	{
		readonly PDictionary dict;
		readonly ListStore iconStore = new ListStore (typeof (string));
		
		const string NameKey = "CFBundleTypeName";
		const string ContentTypesKey = "LSItemContentTypes";
		const string IconFilesKey = "CFBundleTypeIconFiles";
		
		Project proj;
		
		ClosableExpander expander;
		public ClosableExpander Expander {
			get {
				return expander;
			}
			set {
				expander = value;
				UpdateExpanderLabel ();
			}
		}
		
		public DocumentTypeWidget (Project proj, PDictionary dict)
		{
			if (dict == null)
				throw new ArgumentNullException ("dict");
			this.dict = dict;
			this.proj = proj;
			this.Build ();
			this.treeviewIcons.Model = iconStore;
			this.treeviewIcons.AppendColumn ("icon", new CellRendererText (), "text", 0);
			this.treeviewIcons.HeadersVisible = false;
			this.imagechooser1.PictureSize = new Gdk.Size (58, 58);
			dict.Changed += HandleDictChanged;			
			custompropertiesWidget.NSDictionary = dict;
			Update ();
			
			this.entryName.Changed += delegate {
				if (inUpdate)
					return;
				dict.Changed -= HandleDictChanged;
				dict.SetString (NameKey, entryName.Text);
				UpdateExpanderLabel ();
				dict.Changed += HandleDictChanged;
			};
			
			this.entryContentTypes.Changed += delegate {
				if (inUpdate)
					return;
				dict.Changed -= HandleDictChanged;
				dict.GetArray (ContentTypesKey).AssignStringList (entryContentTypes.Text);
				dict.Changed += HandleDictChanged;
			};
			
			this.buttonAdd.Clicked += AddIcon;
			this.buttonRemove.Clicked += RemoveIcon;
		}

		void RemoveIcon (object sender, EventArgs e)
		{
			dict.Changed -= HandleDictChanged;
			Gtk.TreeIter iter;
			if (!treeviewIcons.Selection.GetSelected (out iter))
				return;
			iconStore.Remove (ref iter);
			
			var iconFiles = dict.GetArray (IconFilesKey);
			iconFiles.Clear ();
			if (iconStore.GetIterFirst (out iter)) {
				do {
					iconFiles.Add ((PObject)(string)iconStore.GetValue (iter, 0));
				} while (iconStore.IterNext (ref iter));
			}
			iconFiles.QueueRebuild ();
			dict.Changed += HandleDictChanged;
		}

		void AddIcon (object sender, EventArgs e)
		{
			var dialog = new ProjectFileSelectorDialog (proj, null, "*.png");
			string newIcon;
			try {
				dialog.Title = GettextCatalog.GetString ("Select icon...");
				int response = MessageService.RunCustomDialog (dialog);
				if (response == (int)Gtk.ResponseType.Ok && dialog.SelectedFile != null) {
					newIcon  = dialog.SelectedFile.ProjectVirtualPath;
				} else {
					return;
				}
			} finally {
				dialog.Destroy ();
			}
			
			
			dict.Changed -= HandleDictChanged;
			var iconFiles = dict.GetArray (IconFilesKey);
			
			iconFiles.Add (new PString (newIcon));
			iconFiles.QueueRebuild ();
			iconStore.AppendValues (newIcon);
			dict.Changed += HandleDictChanged;
		}

		void HandleDictChanged (object sender, EventArgs e)
		{
			Update ();
		}
			
		void UpdateExpanderLabel ()
		{
			if (Expander != null)
				Expander.ContentLabel = dict.Get<PString> (NameKey) ?? "";
		}
		
		bool inUpdate = false;
		void Update ()
		{
			inUpdate = true;
			
			entryName.Text = dict.Get<PString> (NameKey) ?? "";
			UpdateExpanderLabel ();
			
			var contentTypes = dict.Get<PArray> (ContentTypesKey);
			entryContentTypes.Text = contentTypes != null ? contentTypes.ToStringList () : "";
			
			iconStore.Clear ();
			var iconFiles = dict.Get<PArray> (IconFilesKey);
			if (iconFiles != null) {
				foreach (PString str in iconFiles.Where (o => o is PString)) {
					iconStore.AppendValues (str.Value);
				}
			}
			inUpdate = false;
		}
	}
}

