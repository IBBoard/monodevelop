
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.Ide.Gui.Dialogs
{
	internal partial class AddinLoadErrorDialog
	{
		private global::Gtk.HBox hbox1;
		private global::Gtk.Image image1;
		private global::Gtk.VBox vbox4;
		private global::Gtk.Label label4;
		private global::Gtk.ScrolledWindow scrolledwindow1;
		private global::Gtk.TreeView errorTree;
		private global::Gtk.Label labelContinue;
		private global::Gtk.Label labelFatal;
		private global::Gtk.Label labelWarning;
		private global::Gtk.Button noButton;
		private global::Gtk.Button yesButton;
		private global::Gtk.Button closeButton;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.Ide.Gui.Dialogs.AddinLoadErrorDialog
			this.Name = "MonoDevelop.Ide.Gui.Dialogs.AddinLoadErrorDialog";
			this.Title = "MonoDevelop";
			this.TypeHint = ((global::Gdk.WindowTypeHint)(1));
			this.BorderWidth = ((uint)(6));
			this.DefaultHeight = 350;
			// Internal child MonoDevelop.Ide.Gui.Dialogs.AddinLoadErrorDialog.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog-vbox1";
			w1.Spacing = 6;
			w1.BorderWidth = ((uint)(2));
			// Container child dialog-vbox1.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 12;
			this.hbox1.BorderWidth = ((uint)(6));
			// Container child hbox1.Gtk.Box+BoxChild
			this.image1 = new global::Gtk.Image ();
			this.image1.Name = "image1";
			this.image1.Xalign = 0F;
			this.image1.Yalign = 0F;
			this.image1.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-dialog-error", global::Gtk.IconSize.Dialog);
			this.hbox1.Add (this.image1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.image1]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.vbox4 = new global::Gtk.VBox ();
			this.vbox4.Name = "vbox4";
			this.vbox4.Spacing = 6;
			// Container child vbox4.Gtk.Box+BoxChild
			this.label4 = new global::Gtk.Label ();
			this.label4.Name = "label4";
			this.label4.Xalign = 0F;
			this.label4.Yalign = 0F;
			this.label4.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("The following add-ins could not be started:");
			this.vbox4.Add (this.label4);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.label4]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow1.Gtk.Container+ContainerChild
			this.errorTree = new global::Gtk.TreeView ();
			this.errorTree.Name = "errorTree";
			this.errorTree.HeadersVisible = false;
			this.scrolledwindow1.Add (this.errorTree);
			this.vbox4.Add (this.scrolledwindow1);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.scrolledwindow1]));
			w5.Position = 1;
			// Container child vbox4.Gtk.Box+BoxChild
			this.labelContinue = new global::Gtk.Label ();
			this.labelContinue.WidthRequest = 479;
			this.labelContinue.Name = "labelContinue";
			this.labelContinue.Xalign = 0F;
			this.labelContinue.Yalign = 0F;
			this.labelContinue.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("You can start MonoDevelop without these add-ins, but the functionality they provide will be missing. Do you wish to continue?");
			this.labelContinue.Wrap = true;
			this.vbox4.Add (this.labelContinue);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.labelContinue]));
			w6.Position = 2;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.labelFatal = new global::Gtk.Label ();
			this.labelFatal.Name = "labelFatal";
			this.labelFatal.Xalign = 0F;
			this.labelFatal.Yalign = 0F;
			this.labelFatal.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("MonoDevelop cannot start because a fatal error has been detected.");
			this.vbox4.Add (this.labelFatal);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.labelFatal]));
			w7.Position = 3;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.labelWarning = new global::Gtk.Label ();
			this.labelWarning.Name = "labelWarning";
			this.labelWarning.Xalign = 0F;
			this.labelWarning.Yalign = 0F;
			this.labelWarning.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("MonoDevelop can run without these add-ins, but the functionality they provide will be missing.");
			this.labelWarning.Wrap = true;
			this.vbox4.Add (this.labelWarning);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.labelWarning]));
			w8.Position = 4;
			w8.Expand = false;
			w8.Fill = false;
			this.hbox1.Add (this.vbox4);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.vbox4]));
			w9.Position = 1;
			w1.Add (this.hbox1);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(w1 [this.hbox1]));
			w10.Position = 0;
			// Internal child MonoDevelop.Ide.Gui.Dialogs.AddinLoadErrorDialog.ActionArea
			global::Gtk.HButtonBox w11 = this.ActionArea;
			w11.Name = "GtkDialog_ActionArea";
			w11.Spacing = 6;
			w11.BorderWidth = ((uint)(5));
			w11.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child GtkDialog_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.noButton = new global::Gtk.Button ();
			this.noButton.CanFocus = true;
			this.noButton.Name = "noButton";
			this.noButton.UseStock = true;
			this.noButton.UseUnderline = true;
			this.noButton.Label = "gtk-no";
			this.AddActionWidget (this.noButton, -9);
			global::Gtk.ButtonBox.ButtonBoxChild w12 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w11 [this.noButton]));
			w12.Expand = false;
			w12.Fill = false;
			// Container child GtkDialog_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.yesButton = new global::Gtk.Button ();
			this.yesButton.CanFocus = true;
			this.yesButton.Name = "yesButton";
			this.yesButton.UseStock = true;
			this.yesButton.UseUnderline = true;
			this.yesButton.Label = "gtk-yes";
			this.AddActionWidget (this.yesButton, -8);
			global::Gtk.ButtonBox.ButtonBoxChild w13 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w11 [this.yesButton]));
			w13.Position = 1;
			w13.Expand = false;
			w13.Fill = false;
			// Container child GtkDialog_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.closeButton = new global::Gtk.Button ();
			this.closeButton.CanFocus = true;
			this.closeButton.Name = "closeButton";
			this.closeButton.UseStock = true;
			this.closeButton.UseUnderline = true;
			this.closeButton.Label = "gtk-close";
			this.AddActionWidget (this.closeButton, -7);
			global::Gtk.ButtonBox.ButtonBoxChild w14 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w11 [this.closeButton]));
			w14.Position = 2;
			w14.Expand = false;
			w14.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 575;
			this.labelFatal.Hide ();
			this.labelWarning.Hide ();
			this.closeButton.Hide ();
			this.Hide ();
		}
	}
}
