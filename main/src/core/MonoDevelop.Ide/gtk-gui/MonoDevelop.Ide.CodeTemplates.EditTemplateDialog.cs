
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.Ide.CodeTemplates
{
	public partial class EditTemplateDialog
	{
		private global::Gtk.HPaned hpaned1;
		private global::Gtk.VBox vbox2;
		private global::Gtk.Table table2;
		private global::Gtk.HBox hbox1;
		private global::Gtk.Entry entryDescription;
		private global::Gtk.Label label2;
		private global::Gtk.ComboBoxEntry comboboxentryMime;
		private global::Gtk.HBox hbox2;
		private global::Gtk.Entry entryShortcut1;
		private global::Gtk.Label label5;
		private global::Gtk.ComboBoxEntry comboboxentryGroups;
		private global::Gtk.HBox hbox5;
		private global::Gtk.CheckButton checkbuttonExpansion;
		private global::Gtk.CheckButton checkbuttonSurroundWith;
		private global::Gtk.Label label1;
		private global::Gtk.Label label3;
		private global::Gtk.VBox vbox3;
		private global::Gtk.HBox hbox3;
		private global::Gtk.Label label6;
		private global::Gtk.Fixed fixed1;
		private global::Gtk.CheckButton checkbuttonWhiteSpaces;
		private global::Gtk.ScrolledWindow scrolledwindow1;
		private global::Gtk.VBox vbox4;
		private global::Gtk.ComboBox comboboxVariables;
		private global::Gtk.ScrolledWindow scrolledwindow2;
		private global::Gtk.TreeView treeviewVariable;
		private global::Gtk.Button buttonCancel;
		private global::Gtk.Button buttonOk;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.Ide.CodeTemplates.EditTemplateDialog
			this.Name = "MonoDevelop.Ide.CodeTemplates.EditTemplateDialog";
			this.Title = "";
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.BorderWidth = ((uint)(6));
			this.SkipPagerHint = true;
			this.SkipTaskbarHint = true;
			// Internal child MonoDevelop.Ide.CodeTemplates.EditTemplateDialog.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.Spacing = 6;
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hpaned1 = new global::Gtk.HPaned ();
			this.hpaned1.CanFocus = true;
			this.hpaned1.Name = "hpaned1";
			this.hpaned1.Position = 555;
			// Container child hpaned1.Gtk.Paned+PanedChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			this.vbox2.BorderWidth = ((uint)(6));
			// Container child vbox2.Gtk.Box+BoxChild
			this.table2 = new global::Gtk.Table (((uint)(3)), ((uint)(2)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(6));
			this.table2.ColumnSpacing = ((uint)(6));
			// Container child table2.Gtk.Table+TableChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.entryDescription = new global::Gtk.Entry ();
			this.entryDescription.CanFocus = true;
			this.entryDescription.Name = "entryDescription";
			this.entryDescription.IsEditable = true;
			this.entryDescription.InvisibleChar = '●';
			this.hbox1.Add (this.entryDescription);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.entryDescription]));
			w2.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.Xalign = 0F;
			this.label2.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("_Mime:");
			this.label2.UseUnderline = true;
			this.hbox1.Add (this.label2);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.label2]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.comboboxentryMime = global::Gtk.ComboBoxEntry.NewText ();
			this.comboboxentryMime.Name = "comboboxentryMime";
			this.hbox1.Add (this.comboboxentryMime);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.comboboxentryMime]));
			w4.Position = 2;
			w4.Expand = false;
			w4.Fill = false;
			this.table2.Add (this.hbox1);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table2 [this.hbox1]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.entryShortcut1 = new global::Gtk.Entry ();
			this.entryShortcut1.CanFocus = true;
			this.entryShortcut1.Name = "entryShortcut1";
			this.entryShortcut1.Text = global::MonoDevelop.Core.GettextCatalog.GetString (" ");
			this.entryShortcut1.IsEditable = true;
			this.entryShortcut1.InvisibleChar = '●';
			this.hbox2.Add (this.entryShortcut1);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.entryShortcut1]));
			w6.Position = 0;
			// Container child hbox2.Gtk.Box+BoxChild
			this.label5 = new global::Gtk.Label ();
			this.label5.Name = "label5";
			this.label5.Xalign = 0F;
			this.label5.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("_Group:");
			this.label5.UseUnderline = true;
			this.hbox2.Add (this.label5);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.label5]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.comboboxentryGroups = global::Gtk.ComboBoxEntry.NewText ();
			this.comboboxentryGroups.Name = "comboboxentryGroups";
			this.hbox2.Add (this.comboboxentryGroups);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.comboboxentryGroups]));
			w8.Position = 2;
			w8.Expand = false;
			w8.Fill = false;
			this.table2.Add (this.hbox2);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table2 [this.hbox2]));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.hbox5 = new global::Gtk.HBox ();
			this.hbox5.Name = "hbox5";
			this.hbox5.Spacing = 6;
			// Container child hbox5.Gtk.Box+BoxChild
			this.checkbuttonExpansion = new global::Gtk.CheckButton ();
			this.checkbuttonExpansion.CanFocus = true;
			this.checkbuttonExpansion.Name = "checkbuttonExpansion";
			this.checkbuttonExpansion.Label = global::MonoDevelop.Core.GettextCatalog.GetString ("Is _expandable template");
			this.checkbuttonExpansion.DrawIndicator = true;
			this.checkbuttonExpansion.UseUnderline = true;
			this.hbox5.Add (this.checkbuttonExpansion);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.checkbuttonExpansion]));
			w10.Position = 0;
			w10.Expand = false;
			// Container child hbox5.Gtk.Box+BoxChild
			this.checkbuttonSurroundWith = new global::Gtk.CheckButton ();
			this.checkbuttonSurroundWith.CanFocus = true;
			this.checkbuttonSurroundWith.Name = "checkbuttonSurroundWith";
			this.checkbuttonSurroundWith.Label = global::MonoDevelop.Core.GettextCatalog.GetString ("Is _surround with template");
			this.checkbuttonSurroundWith.DrawIndicator = true;
			this.checkbuttonSurroundWith.UseUnderline = true;
			this.hbox5.Add (this.checkbuttonSurroundWith);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hbox5 [this.checkbuttonSurroundWith]));
			w11.Position = 1;
			this.table2.Add (this.hbox5);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table2 [this.hbox5]));
			w12.TopAttach = ((uint)(2));
			w12.BottomAttach = ((uint)(3));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.Xalign = 0F;
			this.label1.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("_Shortcut:");
			this.label1.UseUnderline = true;
			this.table2.Add (this.label1);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table2 [this.label1]));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label ();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("_Description:");
			this.label3.UseUnderline = true;
			this.table2.Add (this.label3);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table2 [this.label3]));
			w14.TopAttach = ((uint)(1));
			w14.BottomAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add (this.table2);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.table2]));
			w15.Position = 0;
			w15.Expand = false;
			w15.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.vbox3 = new global::Gtk.VBox ();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			this.vbox3.BorderWidth = ((uint)(6));
			// Container child vbox3.Gtk.Box+BoxChild
			this.hbox3 = new global::Gtk.HBox ();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.label6 = new global::Gtk.Label ();
			this.label6.Name = "label6";
			this.label6.Xalign = 0F;
			this.label6.LabelProp = global::MonoDevelop.Core.GettextCatalog.GetString ("Template Text:");
			this.hbox3.Add (this.label6);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.label6]));
			w16.Position = 0;
			w16.Expand = false;
			w16.Fill = false;
			// Container child hbox3.Gtk.Box+BoxChild
			this.fixed1 = new global::Gtk.Fixed ();
			this.fixed1.Name = "fixed1";
			this.fixed1.HasWindow = false;
			this.hbox3.Add (this.fixed1);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.fixed1]));
			w17.Position = 1;
			// Container child hbox3.Gtk.Box+BoxChild
			this.checkbuttonWhiteSpaces = new global::Gtk.CheckButton ();
			this.checkbuttonWhiteSpaces.CanFocus = true;
			this.checkbuttonWhiteSpaces.Name = "checkbuttonWhiteSpaces";
			this.checkbuttonWhiteSpaces.Label = global::MonoDevelop.Core.GettextCatalog.GetString ("S_how whitespaces");
			this.checkbuttonWhiteSpaces.DrawIndicator = true;
			this.checkbuttonWhiteSpaces.UseUnderline = true;
			this.hbox3.Add (this.checkbuttonWhiteSpaces);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.checkbuttonWhiteSpaces]));
			w18.Position = 2;
			w18.Expand = false;
			this.vbox3.Add (this.hbox3);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.hbox3]));
			w19.Position = 0;
			w19.Expand = false;
			w19.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow1.CanFocus = true;
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			this.vbox3.Add (this.scrolledwindow1);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.scrolledwindow1]));
			w20.Position = 1;
			this.vbox2.Add (this.vbox3);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.vbox3]));
			w21.Position = 1;
			this.hpaned1.Add (this.vbox2);
			global::Gtk.Paned.PanedChild w22 = ((global::Gtk.Paned.PanedChild)(this.hpaned1 [this.vbox2]));
			w22.Resize = false;
			// Container child hpaned1.Gtk.Paned+PanedChild
			this.vbox4 = new global::Gtk.VBox ();
			this.vbox4.Name = "vbox4";
			this.vbox4.Spacing = 6;
			// Container child vbox4.Gtk.Box+BoxChild
			this.comboboxVariables = global::Gtk.ComboBox.NewText ();
			this.comboboxVariables.Name = "comboboxVariables";
			this.vbox4.Add (this.comboboxVariables);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.comboboxVariables]));
			w23.Position = 0;
			w23.Expand = false;
			w23.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.scrolledwindow2 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow2.CanFocus = true;
			this.scrolledwindow2.Name = "scrolledwindow2";
			this.scrolledwindow2.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow2.Gtk.Container+ContainerChild
			this.treeviewVariable = new global::Gtk.TreeView ();
			this.treeviewVariable.CanFocus = true;
			this.treeviewVariable.Name = "treeviewVariable";
			this.scrolledwindow2.Add (this.treeviewVariable);
			this.vbox4.Add (this.scrolledwindow2);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.scrolledwindow2]));
			w25.Position = 1;
			this.hpaned1.Add (this.vbox4);
			w1.Add (this.hpaned1);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(w1 [this.hpaned1]));
			w27.Position = 0;
			// Internal child MonoDevelop.Ide.CodeTemplates.EditTemplateDialog.ActionArea
			global::Gtk.HButtonBox w28 = this.ActionArea;
			w28.Name = "dialog1_ActionArea";
			w28.Spacing = 6;
			w28.BorderWidth = ((uint)(5));
			w28.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w29 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w28 [this.buttonCancel]));
			w29.Expand = false;
			w29.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w30 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w28 [this.buttonOk]));
			w30.Position = 1;
			w30.Expand = false;
			w30.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 859;
			this.DefaultHeight = 494;
			this.Hide ();
		}
	}
}
