
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.Ide.Gui.Dialogs
{
	public partial class OptionsDialog
	{
		private global::Gtk.Alignment alignment;
		private global::Gtk.HBox mainHBox;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.TreeView tree;
		private global::Gtk.VBox vbox3;
		private global::Gtk.HBox hbox2;
		private global::Gtk.Image image;
		private global::Gtk.Label labelTitle;
		private global::Gtk.HSeparator hseparator1;
		private global::Gtk.HBox pageFrame;
		private global::Gtk.Button buttonCancel;
		private global::Gtk.Button buttonOk;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.Ide.Gui.Dialogs.OptionsDialog
			this.Name = "MonoDevelop.Ide.Gui.Dialogs.OptionsDialog";
			this.Title = global::Mono.Unix.Catalog.GetString ("Options");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.Modal = true;
			// Internal child MonoDevelop.Ide.Gui.Dialogs.OptionsDialog.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.alignment = new global::Gtk.Alignment (0.5F, 0.5F, 1F, 1F);
			this.alignment.Name = "alignment";
			this.alignment.LeftPadding = ((uint)(6));
			this.alignment.TopPadding = ((uint)(6));
			this.alignment.RightPadding = ((uint)(6));
			this.alignment.BottomPadding = ((uint)(6));
			this.alignment.BorderWidth = ((uint)(6));
			// Container child alignment.Gtk.Container+ContainerChild
			this.mainHBox = new global::Gtk.HBox ();
			this.mainHBox.Name = "mainHBox";
			this.mainHBox.Spacing = 6;
			// Container child mainHBox.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.HscrollbarPolicy = ((global::Gtk.PolicyType)(2));
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.tree = new global::Gtk.TreeView ();
			this.tree.CanFocus = true;
			this.tree.Name = "tree";
			this.GtkScrolledWindow.Add (this.tree);
			this.mainHBox.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.mainHBox [this.GtkScrolledWindow]));
			w3.Position = 0;
			w3.Expand = false;
			// Container child mainHBox.Gtk.Box+BoxChild
			this.vbox3 = new global::Gtk.VBox ();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 9;
			// Container child vbox3.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.image = new global::Gtk.Image ();
			this.image.Name = "image";
			this.image.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-preferences", global::Gtk.IconSize.LargeToolbar);
			this.hbox2.Add (this.image);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.image]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.labelTitle = new global::Gtk.Label ();
			this.labelTitle.Name = "labelTitle";
			this.labelTitle.Xalign = 0F;
			this.labelTitle.LabelProp = global::Mono.Unix.Catalog.GetString ("<span weight=\"bold\" size=\"x-large\">Title</span>");
			this.labelTitle.UseMarkup = true;
			this.hbox2.Add (this.labelTitle);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.labelTitle]));
			w5.Position = 1;
			this.vbox3.Add (this.hbox2);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.hbox2]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.hseparator1 = new global::Gtk.HSeparator ();
			this.hseparator1.Name = "hseparator1";
			this.vbox3.Add (this.hseparator1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.hseparator1]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.pageFrame = new global::Gtk.HBox ();
			this.pageFrame.Name = "pageFrame";
			this.pageFrame.Spacing = 6;
			this.vbox3.Add (this.pageFrame);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.pageFrame]));
			w8.Position = 2;
			this.mainHBox.Add (this.vbox3);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.mainHBox [this.vbox3]));
			w9.Position = 1;
			w9.Padding = ((uint)(6));
			this.alignment.Add (this.mainHBox);
			w1.Add (this.alignment);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(w1 [this.alignment]));
			w11.Position = 0;
			// Internal child MonoDevelop.Ide.Gui.Dialogs.OptionsDialog.ActionArea
			global::Gtk.HButtonBox w12 = this.ActionArea;
			w12.Name = "dialog1_ActionArea";
			w12.Spacing = 6;
			w12.BorderWidth = ((uint)(5));
			w12.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w13 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w12 [this.buttonCancel]));
			w13.Expand = false;
			w13.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			w12.Add (this.buttonOk);
			global::Gtk.ButtonBox.ButtonBoxChild w14 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w12 [this.buttonOk]));
			w14.Position = 1;
			w14.Expand = false;
			w14.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 722;
			this.DefaultHeight = 502;
			this.Hide ();
			this.buttonOk.Clicked += new global::System.EventHandler (this.OnButtonOkClicked);
		}
	}
}
