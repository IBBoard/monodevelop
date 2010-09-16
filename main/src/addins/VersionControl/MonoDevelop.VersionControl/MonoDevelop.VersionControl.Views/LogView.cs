using System;
using System.IO;
using Gtk;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide;
using System.Text;

namespace MonoDevelop.VersionControl.Views
{
	internal class LogView : BaseView, IAttachableViewContent 
	{
		string filepath;
		Widget widget;
		Revision [] history;
		Repository vc;
		VersionInfo vinfo;
		Gtk.ToolButton revertButton, revertToButton;
		
		TreeView loglist;
		ListStore changedpathstore;
		Toolbar commandbar;
		
		public static void Show (VersionControlItemList items, Revision since)
		{
			foreach (VersionControlItem item in items) {
				if (!item.IsDirectory) {
					var document = IdeApp.Workbench.OpenDocument (item.Path);
					DiffView.AttachViewContents (document, item);
					document.Window.SwitchView (4);
				} else if (item.Repository.IsHistoryAvailable (item.Path)) {
					new Worker (item.Repository, item.Path, item.IsDirectory, since).Start ();
				}
			}
		}
		
		class Worker : Task {
			Repository vc;
			string filepath;
			bool isDirectory;
			Revision since;
			Revision [] history;
						
			public Worker (Repository vc, string filepath, bool isDirectory, Revision since) {
				this.vc = vc;
				this.filepath = filepath;
				this.isDirectory = isDirectory;
				this.since = since;
			}
			
			protected override string GetDescription () {
				return GettextCatalog.GetString ("Retrieving history for {0}...", Path.GetFileName (filepath));
			}
			
			protected override void Run () {
				history = vc.GetHistory (filepath, since);
			}
		
			protected override void Finished() {
				if (history == null)
					return;
				LogView d = new LogView (filepath, isDirectory, history, vc);
				IdeApp.Workbench.OpenDocument (d, true);
			}
		}
		
		public static bool CanShow (VersionControlItemList items, Revision since)
		{
			bool found = false;
			foreach (VersionControlItem item in items) {
				if (item.Repository.IsHistoryAvailable (item.Path)) {
					return true;
				}
			}
			return found;
		}
		
		ListStore logstore;
		public void ShowHistory ()
		{
			if (history == null)
				return;
			foreach (Revision d in history) {
				logstore.AppendValues(
					d.ToString (),
					d.Time.ToString (),
					d.Author,
					d.Message == String.Empty ? GettextCatalog.GetString ("(No message)") : d.Message);
			}
		}
		VersionControlDocumentInfo info;
		public LogView (VersionControlDocumentInfo info) : base ("Log")
		{
			this.info = info;
		}
		
		void CreateControlFromInfo ()
		{
			this.vc = info.Item.Repository;
			this.filepath = info.Item.Path;
			var lw = new LogWidget (info);
			widget = lw;
			info.Updated += delegate {
				history = lw.History = this.info.History;
				vinfo   = this.info.VersionInfo;
			};
			lw.History = history = this.info.History;
			vinfo   = this.info.VersionInfo;
			/*
			// Widget setup
			VBox box = new VBox (false, 6);
			
			widget = box;

			// Create the toolbar
			commandbar = new Toolbar ();
			commandbar.ToolbarStyle = Gtk.ToolbarStyle.BothHoriz;
			commandbar.IconSize = Gtk.IconSize.Menu;
			box.PackStart (commandbar, false, false, 0);
				
			if (vinfo != null) {
				Gtk.ToolButton button = new Gtk.ToolButton (new Gtk.Image ("vc-diff", Gtk.IconSize.Menu), GettextCatalog.GetString ("View Changes"));
				button.IsImportant = true;
				button.Clicked += new EventHandler (DiffButtonClicked);
				commandbar.Insert (button, -1);
				
				button = new Gtk.ToolButton (new Gtk.Image (Gtk.Stock.Open, Gtk.IconSize.Menu), GettextCatalog.GetString ("View File"));
				button.IsImportant = true;
				button.Clicked += new EventHandler (ViewTextButtonClicked);
				commandbar.Insert (button, -1);
			}
			
			revertButton = new Gtk.ToolButton (new Gtk.Image ("vc-revert-command", Gtk.IconSize.Menu), GettextCatalog.GetString ("Revert changes from this revision"));
			revertButton.IsImportant = true;
			revertButton.Sensitive = false;
			revertButton.Clicked += new EventHandler (RevertRevisionClicked);
			commandbar.Insert (revertButton, -1);
			
			revertToButton = new Gtk.ToolButton (new Gtk.Image ("vc-revert-command", Gtk.IconSize.Menu), GettextCatalog.GetString ("Revert to this revision"));
			revertToButton.IsImportant = true;
			revertToButton.Sensitive = false;
			revertToButton.Clicked += new EventHandler (RevertToRevisionClicked);
			commandbar.Insert (revertToButton, -1);

			
			// A paned with two trees
			
			Gtk.VPaned paned = new Gtk.VPaned ();
			box.PackStart (paned, true, true, 0);
			
			// Create the log list
			
			loglist = new TreeView ();
			ScrolledWindow loglistscroll = new ScrolledWindow ();
			loglistscroll.ShadowType = Gtk.ShadowType.In;
			loglistscroll.Add (loglist);
			loglistscroll.HscrollbarPolicy = PolicyType.Automatic;
			loglistscroll.VscrollbarPolicy = PolicyType.Automatic;
			paned.Add1 (loglistscroll);
			((Paned.PanedChild)paned [loglistscroll]).Resize = true;
			
			TreeView changedPaths = new TreeView ();
			ScrolledWindow changedPathsScroll = new ScrolledWindow ();
			changedPathsScroll.ShadowType = Gtk.ShadowType.In;
			changedPathsScroll.HscrollbarPolicy = PolicyType.Automatic;
			changedPathsScroll.VscrollbarPolicy = PolicyType.Automatic;
			changedPathsScroll.Add (changedPaths);
			paned.Add2 (changedPathsScroll);
			((Paned.PanedChild)paned [changedPathsScroll]).Resize = false;

			widget.ShowAll ();
			
			// Revision list setup
			
			CellRendererText textRenderer = new CellRendererText ();
			textRenderer.Yalign = 0;
			
			TreeViewColumn colRevNum = new TreeViewColumn (GettextCatalog.GetString ("Revision"), textRenderer, "text", 0);
			colRevNum.Resizable = true;
			TreeViewColumn colRevDate = new TreeViewColumn (GettextCatalog.GetString ("Date"), textRenderer, "text", 1);
			colRevDate.Resizable = true;
			TreeViewColumn colRevAuthor = new TreeViewColumn (GettextCatalog.GetString ("Author"), textRenderer, "text", 2);
			colRevAuthor.Resizable = true;
			TreeViewColumn colRevMessage = new TreeViewColumn (GettextCatalog.GetString ("Message"), textRenderer, "text", 3);
			colRevMessage.Resizable = true;
			
			loglist.AppendColumn (colRevNum);
			loglist.AppendColumn (colRevDate);
			loglist.AppendColumn (colRevAuthor);
			loglist.AppendColumn (colRevMessage);
			
			logstore = new ListStore (typeof (string), typeof (string), typeof (string), typeof (string));
			loglist.Model = logstore;

			// Changed paths list setup
			
			changedpathstore = new ListStore (typeof(Gdk.Pixbuf), typeof (string), typeof(Gdk.Pixbuf), typeof (string));
			changedPaths.Model = changedpathstore;
			
			TreeViewColumn colOperation = new TreeViewColumn ();
			CellRendererText crt = new CellRendererText ();
			var crp = new CellRendererPixbuf ();
			colOperation.Title = GettextCatalog.GetString ("Operation");
			colOperation.PackStart (crp, false);
			colOperation.PackStart (crt, true);
			colOperation.AddAttribute (crp, "pixbuf", 0);
			colOperation.AddAttribute (crt, "text", 1);
			changedPaths.AppendColumn (colOperation);
			
			TreeViewColumn colChangedPath = new TreeViewColumn ();
			crp = new CellRendererPixbuf ();
			crt = new CellRendererText ();
			colChangedPath.Title = GettextCatalog.GetString ("File Path");
			colChangedPath.PackStart (crp, false);
			colChangedPath.PackStart (crt, true);
			colChangedPath.AddAttribute (crp, "pixbuf", 2);
			colChangedPath.AddAttribute (crt, "text", 3);
			changedPaths.AppendColumn (colChangedPath);
			
			loglist.Selection.Changed += new EventHandler (TreeSelectionChanged);
			
			
			info.Updated += delegate {
				history = this.info.History;
				vinfo   = this.info.VersionInfo;
				ShowHistory ();
			};
			history = this.info.History;
			vinfo   = this.info.VersionInfo;
			ShowHistory ();*/
		}
		
		public LogView (string filepath, bool isDirectory, Revision [] history, Repository vc) 
			: base (Path.GetFileName (filepath) + " Log")
		{
			this.vc = vc;
			this.filepath = filepath;
			this.history = history;
			
			try {
				this.vinfo = vc.GetVersionInfo (filepath, false);
			}
			catch (Exception ex) {
				MessageService.ShowException (ex, GettextCatalog.GetString ("Version control command failed."));
			}

			// Widget setup
			
			VBox box = new VBox (false, 6);
			
			widget = box;

			// Create the toolbar
			commandbar = new Toolbar ();
			commandbar.ToolbarStyle = Gtk.ToolbarStyle.BothHoriz;
			commandbar.IconSize = Gtk.IconSize.Menu;
			box.PackStart (commandbar, false, false, 0);
				
			if (vinfo != null) {
				Gtk.ToolButton button = new Gtk.ToolButton (new Gtk.Image ("vc-diff", Gtk.IconSize.Menu), GettextCatalog.GetString ("View Changes"));
				button.IsImportant = true;
				if (isDirectory) {
					button.Clicked += new EventHandler (DirDiffButtonClicked);
					commandbar.Insert (button, -1);
				} else {
					button.Clicked += new EventHandler (DiffButtonClicked);
					commandbar.Insert (button, -1);
					
					button = new Gtk.ToolButton (new Gtk.Image (Gtk.Stock.Open, Gtk.IconSize.Menu), GettextCatalog.GetString ("View File"));
					button.IsImportant = true;
					button.Clicked += new EventHandler (ViewTextButtonClicked);
					commandbar.Insert (button, -1);
				}
			}
			
			revertButton = new Gtk.ToolButton (new Gtk.Image ("vc-revert-command", Gtk.IconSize.Menu), GettextCatalog.GetString ("Revert changes from this revision"));
			revertButton.IsImportant = true;
			revertButton.Sensitive = false;
			revertButton.Clicked += new EventHandler (RevertRevisionClicked);
			commandbar.Insert (revertButton, -1);
			
			revertToButton = new Gtk.ToolButton (new Gtk.Image ("vc-revert-command", Gtk.IconSize.Menu), GettextCatalog.GetString ("Revert to this revision"));
			revertToButton.IsImportant = true;
			revertToButton.Sensitive = false;
			revertToButton.Clicked += new EventHandler (RevertToRevisionClicked);
			commandbar.Insert (revertToButton, -1);

			
			// A paned with two trees
			
			Gtk.VPaned paned = new Gtk.VPaned ();
			box.PackStart (paned, true, true, 0);
			
			// Create the log list
			
			loglist = new TreeView ();
			ScrolledWindow loglistscroll = new ScrolledWindow ();
			loglistscroll.ShadowType = Gtk.ShadowType.In;
			loglistscroll.Add (loglist);
			loglistscroll.HscrollbarPolicy = PolicyType.Automatic;
			loglistscroll.VscrollbarPolicy = PolicyType.Automatic;
			paned.Add1 (loglistscroll);
			((Paned.PanedChild)paned [loglistscroll]).Resize = true;
			
			TreeView changedPaths = new TreeView ();
			ScrolledWindow changedPathsScroll = new ScrolledWindow ();
			changedPathsScroll.ShadowType = Gtk.ShadowType.In;
			changedPathsScroll.HscrollbarPolicy = PolicyType.Automatic;
			changedPathsScroll.VscrollbarPolicy = PolicyType.Automatic;
			changedPathsScroll.Add (changedPaths);
			paned.Add2 (changedPathsScroll);
			((Paned.PanedChild)paned [changedPathsScroll]).Resize = false;

			widget.ShowAll ();
			
			// Revision list setup
			
			CellRendererText textRenderer = new CellRendererText ();
			textRenderer.Yalign = 0;
			
			TreeViewColumn colRevNum = new TreeViewColumn (GettextCatalog.GetString ("Revision"), textRenderer, "text", 0);
			colRevNum.Resizable = true;
			TreeViewColumn colRevDate = new TreeViewColumn (GettextCatalog.GetString ("Date"), textRenderer, "text", 1);
			colRevDate.Resizable = true;
			TreeViewColumn colFiles = new TreeViewColumn (GettextCatalog.GetString ("Files"), textRenderer, "text", 4);
			colFiles.Resizable = true;
			colFiles.Sizing = TreeViewColumnSizing.Fixed;
			colFiles.FixedWidth = 100;
			TreeViewColumn colRevAuthor = new TreeViewColumn (GettextCatalog.GetString ("Author"), textRenderer, "text", 2);
			colRevAuthor.Resizable = true;
			TreeViewColumn colRevMessage = new TreeViewColumn (GettextCatalog.GetString ("Message"), textRenderer, "text", 3);
			colRevMessage.Resizable = true;
			
			loglist.AppendColumn (colRevNum);
			loglist.AppendColumn (colRevDate);
			loglist.AppendColumn (colRevAuthor);
			loglist.AppendColumn (colFiles);
			loglist.AppendColumn (colRevMessage);
			
			ListStore logstore = new ListStore (typeof (string), typeof (string), typeof (string), typeof (string), typeof(string));
			loglist.Model = logstore;
			 
			foreach (Revision d in history) {
				StringBuilder sb = new StringBuilder ();
				foreach (RevisionPath rp in d.ChangedFiles) {
					if (sb.Length != 0) sb.Append (", ");
					sb.Append (Path.GetFileName (rp.Path));
				}
				logstore.AppendValues(
					d.ToString (),
					d.Time.ToString (),
					d.Author,
					d.Message == String.Empty ? GettextCatalog.GetString ("(No message)") : d.Message,
					sb.ToString ()
					);
			}

			// Changed paths list setup
			
			changedpathstore = new ListStore (typeof(Gdk.Pixbuf), typeof (string), typeof(Gdk.Pixbuf), typeof (string));
			changedPaths.Model = changedpathstore;
			
			TreeViewColumn colOperation = new TreeViewColumn ();
			CellRendererText crt = new CellRendererText ();
			var crp = new CellRendererPixbuf ();
			colOperation.Title = GettextCatalog.GetString ("Operation");
			colOperation.PackStart (crp, false);
			colOperation.PackStart (crt, true);
			colOperation.AddAttribute (crp, "pixbuf", 0);
			colOperation.AddAttribute (crt, "text", 1);
			changedPaths.AppendColumn (colOperation);
			
			TreeViewColumn colChangedPath = new TreeViewColumn ();
			crp = new CellRendererPixbuf ();
			crt = new CellRendererText ();
			colChangedPath.Title = GettextCatalog.GetString ("File Path");
			colChangedPath.PackStart (crp, false);
			colChangedPath.PackStart (crt, true);
			colChangedPath.AddAttribute (crp, "pixbuf", 2);
			colChangedPath.AddAttribute (crt, "text", 3);
			changedPaths.AppendColumn (colChangedPath);
			
			loglist.Selection.Changed += new EventHandler (TreeSelectionChanged);
		}

		Revision GetSelectedRev ()
		{
			int [] indices;
			return GetSelectedRev (out indices);
		}
		
		Revision GetSelectedRev (out int [] indices)
		{
			indices = null;
			TreePath path;
			TreeViewColumn col;
			
			loglist.GetCursor (out path, out col);
			if (path == null)
				return null;

			indices = path.Indices;
			return history [indices [0]];
		}
		
		void TreeSelectionChanged (object o, EventArgs args) {
			int [] indices;
			Revision d = GetSelectedRev (out indices);
			
			revertButton.Sensitive = (d != null);
			revertToButton.Sensitive = ((d != null) &&
			                            (indices.Length == 1) && //no sense to revert to *many* revs
			                            (indices [0] != 0)); //no sense to revert to *current* rev
			
			changedpathstore.Clear ();
			foreach (RevisionPath rp in d.ChangedFiles) 
			{
				Gdk.Pixbuf actionIcon;
				string action = null;
				if (rp.Action == RevisionAction.Add) {
					action = GettextCatalog.GetString ("Add");
					actionIcon = ImageService.GetPixbuf (Gtk.Stock.Add, Gtk.IconSize.Menu);
				}
				else if (rp.Action == RevisionAction.Delete) {
					action = GettextCatalog.GetString ("Delete");
					actionIcon = ImageService.GetPixbuf (Gtk.Stock.Remove, Gtk.IconSize.Menu);
				}
				else if (rp.Action == RevisionAction.Modify) {
					action = GettextCatalog.GetString ("Modify");
					actionIcon = ImageService.GetPixbuf ("gtk-edit", Gtk.IconSize.Menu);
				}
				else if (rp.Action == RevisionAction.Replace) {
					action = GettextCatalog.GetString ("Replace");
					actionIcon = ImageService.GetPixbuf ("gtk-edit", Gtk.IconSize.Menu);
				} else {
					action = rp.ActionDescription;
					actionIcon = ImageService.GetPixbuf (MonoDevelop.Ide.Gui.Stock.Empty, Gtk.IconSize.Menu);
				}
				
				Gdk.Pixbuf fileIcon = DesktopService.GetPixbufForFile (rp.Path, Gtk.IconSize.Menu);
				changedpathstore.AppendValues (actionIcon, action, fileIcon, rp.Path);
			}
		}
		
		void DiffButtonClicked (object src, EventArgs args) {
			Revision d = GetSelectedRev ();
			if (d == null)
				return;
			
			
			DiffView comparisonView = new DiffView (info, d.GetPrevious (), d);
			IdeApp.Workbench.OpenDocument (comparisonView, true);
		}
		
		void DirDiffButtonClicked (object src, EventArgs args) {
			Revision d = GetSelectedRev ();
			if (d == null)
				return;
			new DirectoryDiffWorker (filepath, vc, d).Start ();
		}
		
		void ViewTextButtonClicked (object src, EventArgs args) {
			Revision d = GetSelectedRev ();
			if (d == null)
				return;
			HistoricalFileView.Show (filepath, vc, vinfo.RepositoryPath, d);
		}
		
		void RevertToRevisionClicked (object src, EventArgs args) {
			Revision d = GetSelectedRev ();
			if (RevertRevisionsCommands.RevertToRevision (vc, filepath, d, false))
				VersionControlService.SetCommitComment (filepath, 
				  string.Format ("(Revert to revision {0})", d.ToString ()), true);
		}
		
		void RevertRevisionClicked (object src, EventArgs args) {
			Revision d = GetSelectedRev ();
			if (RevertRevisionsCommands.RevertRevision (vc, filepath, d, false))
				VersionControlService.SetCommitComment (filepath, 
				  string.Format ("(Revert revision {0})", d.ToString ()), true);
		}
		
		public override Gtk.Widget Control { 
			get {
				if (widget == null)
					CreateControlFromInfo ();
				return widget; 
			}
		}
		
		public override void Dispose ()
		{
			if (widget != null) {
				widget.Destroy ();
				widget = null;
			}
			if (changedpathstore != null) {
				changedpathstore.Dispose ();
				changedpathstore = null;
			}
			base.Dispose ();
		}

		
/*		internal class DiffWorker : Task {
			Repository vc;
			string name;
			Revision revision;
			string text1, text2;
			string revPath;
						
			public DiffWorker (string name, Repository vc, string revPath, Revision revision) {
				this.name = name;
				this.vc = vc;
				this.revPath = revPath;
				this.revision = revision;
			}
			
			protected override string GetDescription () {
				return GettextCatalog.GetString ("Retrieving changes in {0} at revision {1}...", name, revision);
			}
			
			protected override void Run () {
				Log (GettextCatalog.GetString ("Getting text of {0} at revision {1}...", revPath, revision.GetPrevious ()));
				try {
					text1 = vc.GetTextAtRevision (revPath, revision.GetPrevious ());
				} catch {
					// If the file was added in this revision, no previous
					// text exists.
					text1 = String.Empty;
				}
				Log (GettextCatalog.GetString ("Getting text of {0} at revision {1}...", revPath, revision));
				text2 = vc.GetTextAtRevision (revPath, revision);
			}
		
			protected override void Finished () {
				if (text1 == null || text2 == null) return;
				DiffView.Show (name + " (revision " + revision.ToString () + ")", DesktopService.GetMimeTypeForUri (revPath), text1, text2);
			}
		}*/
		
		/// Background worker to create a revision-specific diff for a directory
		internal class DirectoryDiffWorker: Task
		{
			FilePath path;
			Repository repo;
			Revision revision;
			string name;
			string patch;
			
			public DirectoryDiffWorker (FilePath path, Repository repo, Revision revision)
			{
				this.path = path;
				name = string.Format ("{0} (revision {1})", path.FileName, revision);
				this.repo = repo;
				this.revision = revision;
			}
			
			protected override string GetDescription ()
			{
				return GettextCatalog.GetString ("Retrieving changes in {0} ...", name, revision);
			}
			
			
			protected override void Run ()
			{
				DiffInfo[] diffs = repo.PathDiff (path, revision.GetPrevious (), revision);
				patch = repo.CreatePatch (diffs);
			}
			
			protected override void Finished ()
			{
				if (patch != null)
					IdeApp.Workbench.NewDocument (name, "text/x-diff", patch);
			}
		}
		
		#region IAttachableViewContent implementation
		public void Selected ()
		{
			if (info != null)
				info.Start ();
		}

		public void Deselected ()
		{
		}

		public void BeforeSave ()
		{
		}

		public void BaseContentChanged ()
		{
		}
		#endregion
	}

	internal class HistoricalFileView
	{
		public static void Show (string name, string file, string text) {
			string mimeType = DesktopService.GetMimeTypeForUri (file);
			if (mimeType == null || mimeType.Length == 0)
				mimeType = "text/plain";
			Document doc = IdeApp.Workbench.NewDocument (name, mimeType, text);
			doc.IsDirty = false;
		}
			
		public static void Show (string file, Repository vc, string revPath, Revision revision) {
			new Worker (Path.GetFileName (file) + " (revision " + revision.ToString () + ")",
				file, vc, revPath, revision).Start ();
		}
		
			
		internal class Worker : Task {
			Repository vc;
			string name, file;
			string revPath;
			Revision revision;
			string text;
						
			public Worker (string name, string file, Repository vc, string revPath, Revision revision) {
				this.name = name;
				this.file = file;
				this.vc = vc;
				this.revPath = revPath;
				this.revision = revision;
			}
			
			protected override string GetDescription () {
				return GettextCatalog.GetString ("Retreiving content of {0} at revision {1}...", name, revision);
			}
			
			protected override void Run () {
				text = vc.GetTextAtRevision (revPath, revision);
			}
		
			protected override void Finished () {
				if (text == null)
					return;
				HistoricalFileView.Show (name, file, text);
			}
		}
	}

}
