
// This file has been generated by the GUI designer. Do not modify.
namespace CBinding
{
	public partial class CodeGenerationPanel
	{
		private global::Gtk.Notebook notebook1;
		private global::Gtk.VBox vbox6;
		private global::Gtk.Table table1;
		private global::Gtk.Label label4;
		private global::Gtk.Label label5;
		private global::Gtk.Label label6;
		private global::Gtk.SpinButton optimizationSpinButton;
		private global::Gtk.ComboBox targetComboBox;
		private global::Gtk.VBox vbox1;
		private global::Gtk.RadioButton noWarningRadio;
		private global::Gtk.RadioButton normalWarningRadio;
		private global::Gtk.RadioButton allWarningRadio;
		private global::Gtk.CheckButton warningsAsErrorsCheckBox;
		private global::Gtk.HBox hbox1;
		private global::Gtk.Label label12;
		private global::Gtk.Entry defineSymbolsTextEntry;
		private global::Gtk.Frame frame2;
		private global::Gtk.Alignment GtkAlignment;
		private global::Gtk.Table table5;
		private global::Gtk.Label label11;
		private global::Gtk.Label label7;
		private global::Gtk.ScrolledWindow scrolledwindow4;
		private global::Gtk.TextView extraCompilerTextView;
		private global::Gtk.ScrolledWindow scrolledwindow5;
		private global::Gtk.TextView extraLinkerTextView;
		private global::Gtk.Label GtkLabel12;
		private global::Gtk.Label label1;
		private global::Gtk.Table table2;
		private global::Gtk.Button addLibButton;
		private global::Gtk.Label label8;
		private global::Gtk.Entry libAddEntry;
		private global::Gtk.ScrolledWindow scrolledwindow1;
		private global::Gtk.TreeView libTreeView;
		private global::Gtk.VBox vbox4;
		private global::Gtk.Button browseButton;
		private global::Gtk.Button removeLibButton;
		private global::Gtk.Label label2;
		private global::Gtk.VBox vbox7;
		private global::Gtk.Table table4;
		private global::Gtk.HBox hbox2;
		private global::Gtk.Entry libPathEntry;
		private global::Gtk.Button quickInsertLibButton;
		private global::Gtk.Label label10;
		private global::Gtk.Button libPathAddButton;
		private global::Gtk.ScrolledWindow scrolledwindow3;
		private global::Gtk.TreeView libPathTreeView;
		private global::Gtk.VBox vbox3;
		private global::Gtk.Button libPathBrowseButton;
		private global::Gtk.Button libPathRemoveButton;
		private global::Gtk.Table table3;
		private global::Gtk.HBox hbox3;
		private global::Gtk.Entry includePathEntry;
		private global::Gtk.Button quickInsertIncludeButton;
		private global::Gtk.Button includePathAddButton;
		private global::Gtk.Label label9;
		private global::Gtk.ScrolledWindow scrolledwindow2;
		private global::Gtk.TreeView includePathTreeView;
		private global::Gtk.VBox vbox5;
		private global::Gtk.Button includePathBrowseButton;
		private global::Gtk.Button includePathRemoveButton;
		private global::Gtk.Label label3;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget CBinding.CodeGenerationPanel
			global::Stetic.BinContainer.Attach (this);
			this.Name = "CBinding.CodeGenerationPanel";
			// Container child CBinding.CodeGenerationPanel.Gtk.Container+ContainerChild
			this.notebook1 = new global::Gtk.Notebook ();
			this.notebook1.CanFocus = true;
			this.notebook1.Name = "notebook1";
			this.notebook1.CurrentPage = 0;
			// Container child notebook1.Gtk.Notebook+NotebookChild
			this.vbox6 = new global::Gtk.VBox ();
			this.vbox6.Name = "vbox6";
			this.vbox6.Spacing = 3;
			// Container child vbox6.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table (((uint)(3)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(5));
			this.table1.ColumnSpacing = ((uint)(5));
			this.table1.BorderWidth = ((uint)(2));
			// Container child table1.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label ();
			this.label4.Name = "label4";
			this.label4.Xpad = 10;
			this.label4.Xalign = 0F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString ("Warning Level:");
			this.table1.Add (this.label4);
			global::Gtk.Table.TableChild w1 = ((global::Gtk.Table.TableChild)(this.table1 [this.label4]));
			w1.XOptions = ((global::Gtk.AttachOptions)(4));
			w1.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label ();
			this.label5.Name = "label5";
			this.label5.Xpad = 10;
			this.label5.Xalign = 0F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString ("Optimization Level:");
			this.table1.Add (this.label5);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1 [this.label5]));
			w2.TopAttach = ((uint)(1));
			w2.BottomAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label6 = new global::Gtk.Label ();
			this.label6.Name = "label6";
			this.label6.Xpad = 10;
			this.label6.Xalign = 0F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString ("Target:");
			this.table1.Add (this.label6);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1 [this.label6]));
			w3.TopAttach = ((uint)(2));
			w3.BottomAttach = ((uint)(3));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.optimizationSpinButton = new global::Gtk.SpinButton (0, 3, 1);
			this.optimizationSpinButton.CanFocus = true;
			this.optimizationSpinButton.Name = "optimizationSpinButton";
			this.optimizationSpinButton.Adjustment.PageIncrement = 10;
			this.optimizationSpinButton.ClimbRate = 1;
			this.optimizationSpinButton.Numeric = true;
			this.table1.Add (this.optimizationSpinButton);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1 [this.optimizationSpinButton]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.targetComboBox = global::Gtk.ComboBox.NewText ();
			this.targetComboBox.AppendText (global::Mono.Unix.Catalog.GetString ("Executable"));
			this.targetComboBox.AppendText (global::Mono.Unix.Catalog.GetString ("Static Library"));
			this.targetComboBox.AppendText (global::Mono.Unix.Catalog.GetString ("Shared Object"));
			this.targetComboBox.Name = "targetComboBox";
			this.table1.Add (this.targetComboBox);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1 [this.targetComboBox]));
			w5.TopAttach = ((uint)(2));
			w5.BottomAttach = ((uint)(3));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 1;
			// Container child vbox1.Gtk.Box+BoxChild
			this.noWarningRadio = new global::Gtk.RadioButton (global::Mono.Unix.Catalog.GetString ("no warnings"));
			this.noWarningRadio.CanFocus = true;
			this.noWarningRadio.Name = "noWarningRadio";
			this.noWarningRadio.DrawIndicator = true;
			this.noWarningRadio.UseUnderline = true;
			this.noWarningRadio.Group = new global::GLib.SList (global::System.IntPtr.Zero);
			this.vbox1.Add (this.noWarningRadio);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.noWarningRadio]));
			w6.Position = 0;
			w6.Expand = false;
			w6.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.normalWarningRadio = new global::Gtk.RadioButton (global::Mono.Unix.Catalog.GetString ("normal"));
			this.normalWarningRadio.CanFocus = true;
			this.normalWarningRadio.Name = "normalWarningRadio";
			this.normalWarningRadio.DrawIndicator = true;
			this.normalWarningRadio.UseUnderline = true;
			this.normalWarningRadio.Group = this.noWarningRadio.Group;
			this.vbox1.Add (this.normalWarningRadio);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.normalWarningRadio]));
			w7.Position = 1;
			w7.Expand = false;
			w7.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.allWarningRadio = new global::Gtk.RadioButton (global::Mono.Unix.Catalog.GetString ("all"));
			this.allWarningRadio.CanFocus = true;
			this.allWarningRadio.Name = "allWarningRadio";
			this.allWarningRadio.DrawIndicator = true;
			this.allWarningRadio.UseUnderline = true;
			this.allWarningRadio.Group = this.noWarningRadio.Group;
			this.vbox1.Add (this.allWarningRadio);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.allWarningRadio]));
			w8.Position = 2;
			w8.Expand = false;
			w8.Fill = false;
			// Container child vbox1.Gtk.Box+BoxChild
			this.warningsAsErrorsCheckBox = new global::Gtk.CheckButton ();
			this.warningsAsErrorsCheckBox.CanFocus = true;
			this.warningsAsErrorsCheckBox.Name = "warningsAsErrorsCheckBox";
			this.warningsAsErrorsCheckBox.Label = global::Mono.Unix.Catalog.GetString ("Treat warnings as errors");
			this.warningsAsErrorsCheckBox.DrawIndicator = true;
			this.warningsAsErrorsCheckBox.UseUnderline = true;
			this.vbox1.Add (this.warningsAsErrorsCheckBox);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.warningsAsErrorsCheckBox]));
			w9.Position = 3;
			w9.Expand = false;
			w9.Fill = false;
			this.table1.Add (this.vbox1);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1 [this.vbox1]));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox6.Add (this.table1);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox6 [this.table1]));
			w11.Position = 0;
			w11.Expand = false;
			w11.Fill = false;
			// Container child vbox6.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.label12 = new global::Gtk.Label ();
			this.label12.Name = "label12";
			this.label12.Xpad = 13;
			this.label12.Xalign = 0F;
			this.label12.LabelProp = global::Mono.Unix.Catalog.GetString ("Define Symbols:");
			this.hbox1.Add (this.label12);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.label12]));
			w12.Position = 0;
			w12.Expand = false;
			w12.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.defineSymbolsTextEntry = new global::Gtk.Entry ();
			this.defineSymbolsTextEntry.TooltipMarkup = "A space seperated list of symbols to define.";
			this.defineSymbolsTextEntry.CanFocus = true;
			this.defineSymbolsTextEntry.Name = "defineSymbolsTextEntry";
			this.defineSymbolsTextEntry.IsEditable = true;
			this.defineSymbolsTextEntry.InvisibleChar = '●';
			this.hbox1.Add (this.defineSymbolsTextEntry);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.defineSymbolsTextEntry]));
			w13.Position = 1;
			w13.Padding = ((uint)(14));
			this.vbox6.Add (this.hbox1);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox6 [this.hbox1]));
			w14.Position = 1;
			w14.Expand = false;
			w14.Fill = false;
			// Container child vbox6.Gtk.Box+BoxChild
			this.frame2 = new global::Gtk.Frame ();
			this.frame2.Name = "frame2";
			this.frame2.ShadowType = ((global::Gtk.ShadowType)(0));
			this.frame2.LabelYalign = 0F;
			// Container child frame2.Gtk.Container+ContainerChild
			this.GtkAlignment = new global::Gtk.Alignment (0F, 0F, 1F, 1F);
			this.GtkAlignment.Name = "GtkAlignment";
			this.GtkAlignment.LeftPadding = ((uint)(12));
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			this.table5 = new global::Gtk.Table (((uint)(2)), ((uint)(2)), false);
			this.table5.Name = "table5";
			this.table5.RowSpacing = ((uint)(6));
			this.table5.ColumnSpacing = ((uint)(9));
			this.table5.BorderWidth = ((uint)(6));
			// Container child table5.Gtk.Table+TableChild
			this.label11 = new global::Gtk.Label ();
			this.label11.Name = "label11";
			this.label11.Xalign = 0F;
			this.label11.LabelProp = global::Mono.Unix.Catalog.GetString ("Extra Linker Options");
			this.table5.Add (this.label11);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table5 [this.label11]));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table5.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label ();
			this.label7.Name = "label7";
			this.label7.Xalign = 0F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString ("Extra Compiler Options");
			this.table5.Add (this.label7);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table5 [this.label7]));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table5.Gtk.Table+TableChild
			this.scrolledwindow4 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow4.CanFocus = true;
			this.scrolledwindow4.Name = "scrolledwindow4";
			this.scrolledwindow4.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow4.Gtk.Container+ContainerChild
			this.extraCompilerTextView = new global::Gtk.TextView ();
			this.extraCompilerTextView.TooltipMarkup = "A newline seperated list of extra options to send to the compiler.\nOne option can be in more than one line.\nExample:\n\t`pkg-config\n\t--cflags\n\tcairo`";
			this.extraCompilerTextView.CanFocus = true;
			this.extraCompilerTextView.Name = "extraCompilerTextView";
			this.scrolledwindow4.Add (this.extraCompilerTextView);
			this.table5.Add (this.scrolledwindow4);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table5 [this.scrolledwindow4]));
			w18.TopAttach = ((uint)(1));
			w18.BottomAttach = ((uint)(2));
			// Container child table5.Gtk.Table+TableChild
			this.scrolledwindow5 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow5.CanFocus = true;
			this.scrolledwindow5.Name = "scrolledwindow5";
			this.scrolledwindow5.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow5.Gtk.Container+ContainerChild
			this.extraLinkerTextView = new global::Gtk.TextView ();
			this.extraLinkerTextView.TooltipMarkup = "A newline seperated list of extra options to send to the linker.\nOne option can be in more than one line.\nExample:\n\t`pkg-config\n\t--libs\n\tcairo`";
			this.extraLinkerTextView.CanFocus = true;
			this.extraLinkerTextView.Name = "extraLinkerTextView";
			this.scrolledwindow5.Add (this.extraLinkerTextView);
			this.table5.Add (this.scrolledwindow5);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table5 [this.scrolledwindow5]));
			w20.TopAttach = ((uint)(1));
			w20.BottomAttach = ((uint)(2));
			w20.LeftAttach = ((uint)(1));
			w20.RightAttach = ((uint)(2));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			this.GtkAlignment.Add (this.table5);
			this.frame2.Add (this.GtkAlignment);
			this.GtkLabel12 = new global::Gtk.Label ();
			this.GtkLabel12.Name = "GtkLabel12";
			this.GtkLabel12.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Extra Options</b>");
			this.GtkLabel12.UseMarkup = true;
			this.frame2.LabelWidget = this.GtkLabel12;
			this.vbox6.Add (this.frame2);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.vbox6 [this.frame2]));
			w23.Position = 2;
			this.notebook1.Add (this.vbox6);
			// Notebook tab
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Code Generation");
			this.notebook1.SetTabLabel (this.vbox6, this.label1);
			this.label1.ShowAll ();
			// Container child notebook1.Gtk.Notebook+NotebookChild
			this.table2 = new global::Gtk.Table (((uint)(2)), ((uint)(3)), false);
			this.table2.Name = "table2";
			this.table2.RowSpacing = ((uint)(10));
			this.table2.ColumnSpacing = ((uint)(10));
			this.table2.BorderWidth = ((uint)(3));
			// Container child table2.Gtk.Table+TableChild
			this.addLibButton = new global::Gtk.Button ();
			this.addLibButton.Sensitive = false;
			this.addLibButton.CanFocus = true;
			this.addLibButton.Name = "addLibButton";
			this.addLibButton.UseUnderline = true;
			this.addLibButton.Label = global::Mono.Unix.Catalog.GetString ("Add");
			this.table2.Add (this.addLibButton);
			global::Gtk.Table.TableChild w25 = ((global::Gtk.Table.TableChild)(this.table2 [this.addLibButton]));
			w25.LeftAttach = ((uint)(2));
			w25.RightAttach = ((uint)(3));
			w25.XOptions = ((global::Gtk.AttachOptions)(4));
			w25.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.label8 = new global::Gtk.Label ();
			this.label8.Name = "label8";
			this.label8.LabelProp = global::Mono.Unix.Catalog.GetString ("Library:");
			this.table2.Add (this.label8);
			global::Gtk.Table.TableChild w26 = ((global::Gtk.Table.TableChild)(this.table2 [this.label8]));
			w26.XOptions = ((global::Gtk.AttachOptions)(4));
			w26.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.libAddEntry = new global::Gtk.Entry ();
			this.libAddEntry.CanFocus = true;
			this.libAddEntry.Name = "libAddEntry";
			this.libAddEntry.IsEditable = true;
			this.libAddEntry.InvisibleChar = '●';
			this.table2.Add (this.libAddEntry);
			global::Gtk.Table.TableChild w27 = ((global::Gtk.Table.TableChild)(this.table2 [this.libAddEntry]));
			w27.LeftAttach = ((uint)(1));
			w27.RightAttach = ((uint)(2));
			w27.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table2.Gtk.Table+TableChild
			this.scrolledwindow1 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow1.CanFocus = true;
			this.scrolledwindow1.Name = "scrolledwindow1";
			this.scrolledwindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow1.Gtk.Container+ContainerChild
			this.libTreeView = new global::Gtk.TreeView ();
			this.libTreeView.CanFocus = true;
			this.libTreeView.Name = "libTreeView";
			this.scrolledwindow1.Add (this.libTreeView);
			this.table2.Add (this.scrolledwindow1);
			global::Gtk.Table.TableChild w29 = ((global::Gtk.Table.TableChild)(this.table2 [this.scrolledwindow1]));
			w29.TopAttach = ((uint)(1));
			w29.BottomAttach = ((uint)(2));
			w29.LeftAttach = ((uint)(1));
			w29.RightAttach = ((uint)(2));
			// Container child table2.Gtk.Table+TableChild
			this.vbox4 = new global::Gtk.VBox ();
			this.vbox4.Name = "vbox4";
			this.vbox4.Spacing = 6;
			// Container child vbox4.Gtk.Box+BoxChild
			this.browseButton = new global::Gtk.Button ();
			this.browseButton.CanFocus = true;
			this.browseButton.Name = "browseButton";
			this.browseButton.UseUnderline = true;
			this.browseButton.Label = global::Mono.Unix.Catalog.GetString ("Browse...");
			this.vbox4.Add (this.browseButton);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.browseButton]));
			w30.Position = 0;
			w30.Expand = false;
			w30.Fill = false;
			// Container child vbox4.Gtk.Box+BoxChild
			this.removeLibButton = new global::Gtk.Button ();
			this.removeLibButton.Sensitive = false;
			this.removeLibButton.CanFocus = true;
			this.removeLibButton.Name = "removeLibButton";
			this.removeLibButton.UseUnderline = true;
			this.removeLibButton.Label = global::Mono.Unix.Catalog.GetString ("Remove");
			this.vbox4.Add (this.removeLibButton);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.vbox4 [this.removeLibButton]));
			w31.Position = 1;
			w31.Expand = false;
			w31.Fill = false;
			this.table2.Add (this.vbox4);
			global::Gtk.Table.TableChild w32 = ((global::Gtk.Table.TableChild)(this.table2 [this.vbox4]));
			w32.TopAttach = ((uint)(1));
			w32.BottomAttach = ((uint)(2));
			w32.LeftAttach = ((uint)(2));
			w32.RightAttach = ((uint)(3));
			w32.XOptions = ((global::Gtk.AttachOptions)(4));
			this.notebook1.Add (this.table2);
			global::Gtk.Notebook.NotebookChild w33 = ((global::Gtk.Notebook.NotebookChild)(this.notebook1 [this.table2]));
			w33.Position = 1;
			// Notebook tab
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("Libraries");
			this.notebook1.SetTabLabel (this.table2, this.label2);
			this.label2.ShowAll ();
			// Container child notebook1.Gtk.Notebook+NotebookChild
			this.vbox7 = new global::Gtk.VBox ();
			this.vbox7.Name = "vbox7";
			this.vbox7.Spacing = 6;
			this.vbox7.BorderWidth = ((uint)(3));
			// Container child vbox7.Gtk.Box+BoxChild
			this.table4 = new global::Gtk.Table (((uint)(2)), ((uint)(3)), false);
			this.table4.Name = "table4";
			this.table4.RowSpacing = ((uint)(10));
			this.table4.ColumnSpacing = ((uint)(10));
			// Container child table4.Gtk.Table+TableChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.libPathEntry = new global::Gtk.Entry ();
			this.libPathEntry.CanFocus = true;
			this.libPathEntry.Name = "libPathEntry";
			this.libPathEntry.IsEditable = true;
			this.libPathEntry.InvisibleChar = '●';
			this.hbox2.Add (this.libPathEntry);
			global::Gtk.Box.BoxChild w34 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.libPathEntry]));
			w34.Position = 0;
			// Container child hbox2.Gtk.Box+BoxChild
			this.quickInsertLibButton = new global::Gtk.Button ();
			this.quickInsertLibButton.TooltipMarkup = "Insert a macro.";
			this.quickInsertLibButton.CanFocus = true;
			this.quickInsertLibButton.Name = "quickInsertLibButton";
			this.quickInsertLibButton.UseUnderline = true;
			this.quickInsertLibButton.Label = global::Mono.Unix.Catalog.GetString (">");
			this.hbox2.Add (this.quickInsertLibButton);
			global::Gtk.Box.BoxChild w35 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.quickInsertLibButton]));
			w35.Position = 1;
			w35.Expand = false;
			w35.Fill = false;
			this.table4.Add (this.hbox2);
			global::Gtk.Table.TableChild w36 = ((global::Gtk.Table.TableChild)(this.table4 [this.hbox2]));
			w36.LeftAttach = ((uint)(1));
			w36.RightAttach = ((uint)(2));
			w36.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table4.Gtk.Table+TableChild
			this.label10 = new global::Gtk.Label ();
			this.label10.Name = "label10";
			this.label10.LabelProp = global::Mono.Unix.Catalog.GetString ("Library:");
			this.table4.Add (this.label10);
			global::Gtk.Table.TableChild w37 = ((global::Gtk.Table.TableChild)(this.table4 [this.label10]));
			w37.XOptions = ((global::Gtk.AttachOptions)(4));
			w37.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table4.Gtk.Table+TableChild
			this.libPathAddButton = new global::Gtk.Button ();
			this.libPathAddButton.Sensitive = false;
			this.libPathAddButton.CanFocus = true;
			this.libPathAddButton.Name = "libPathAddButton";
			this.libPathAddButton.UseUnderline = true;
			this.libPathAddButton.Label = global::Mono.Unix.Catalog.GetString ("Add");
			this.table4.Add (this.libPathAddButton);
			global::Gtk.Table.TableChild w38 = ((global::Gtk.Table.TableChild)(this.table4 [this.libPathAddButton]));
			w38.LeftAttach = ((uint)(2));
			w38.RightAttach = ((uint)(3));
			w38.XOptions = ((global::Gtk.AttachOptions)(4));
			w38.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table4.Gtk.Table+TableChild
			this.scrolledwindow3 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow3.CanFocus = true;
			this.scrolledwindow3.Name = "scrolledwindow3";
			this.scrolledwindow3.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow3.Gtk.Container+ContainerChild
			this.libPathTreeView = new global::Gtk.TreeView ();
			this.libPathTreeView.CanFocus = true;
			this.libPathTreeView.Name = "libPathTreeView";
			this.scrolledwindow3.Add (this.libPathTreeView);
			this.table4.Add (this.scrolledwindow3);
			global::Gtk.Table.TableChild w40 = ((global::Gtk.Table.TableChild)(this.table4 [this.scrolledwindow3]));
			w40.TopAttach = ((uint)(1));
			w40.BottomAttach = ((uint)(2));
			w40.LeftAttach = ((uint)(1));
			w40.RightAttach = ((uint)(2));
			// Container child table4.Gtk.Table+TableChild
			this.vbox3 = new global::Gtk.VBox ();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.libPathBrowseButton = new global::Gtk.Button ();
			this.libPathBrowseButton.CanFocus = true;
			this.libPathBrowseButton.Name = "libPathBrowseButton";
			this.libPathBrowseButton.UseUnderline = true;
			this.libPathBrowseButton.Label = global::Mono.Unix.Catalog.GetString ("Browse...");
			this.vbox3.Add (this.libPathBrowseButton);
			global::Gtk.Box.BoxChild w41 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.libPathBrowseButton]));
			w41.Position = 0;
			w41.Expand = false;
			w41.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.libPathRemoveButton = new global::Gtk.Button ();
			this.libPathRemoveButton.Sensitive = false;
			this.libPathRemoveButton.CanFocus = true;
			this.libPathRemoveButton.Name = "libPathRemoveButton";
			this.libPathRemoveButton.UseUnderline = true;
			this.libPathRemoveButton.Label = global::Mono.Unix.Catalog.GetString ("Remove");
			this.vbox3.Add (this.libPathRemoveButton);
			global::Gtk.Box.BoxChild w42 = ((global::Gtk.Box.BoxChild)(this.vbox3 [this.libPathRemoveButton]));
			w42.Position = 1;
			w42.Expand = false;
			w42.Fill = false;
			this.table4.Add (this.vbox3);
			global::Gtk.Table.TableChild w43 = ((global::Gtk.Table.TableChild)(this.table4 [this.vbox3]));
			w43.TopAttach = ((uint)(1));
			w43.BottomAttach = ((uint)(2));
			w43.LeftAttach = ((uint)(2));
			w43.RightAttach = ((uint)(3));
			w43.XOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox7.Add (this.table4);
			global::Gtk.Box.BoxChild w44 = ((global::Gtk.Box.BoxChild)(this.vbox7 [this.table4]));
			w44.Position = 0;
			// Container child vbox7.Gtk.Box+BoxChild
			this.table3 = new global::Gtk.Table (((uint)(2)), ((uint)(3)), false);
			this.table3.Name = "table3";
			this.table3.RowSpacing = ((uint)(10));
			this.table3.ColumnSpacing = ((uint)(10));
			// Container child table3.Gtk.Table+TableChild
			this.hbox3 = new global::Gtk.HBox ();
			this.hbox3.Name = "hbox3";
			this.hbox3.Spacing = 6;
			// Container child hbox3.Gtk.Box+BoxChild
			this.includePathEntry = new global::Gtk.Entry ();
			this.includePathEntry.CanFocus = true;
			this.includePathEntry.Name = "includePathEntry";
			this.includePathEntry.IsEditable = true;
			this.includePathEntry.InvisibleChar = '●';
			this.hbox3.Add (this.includePathEntry);
			global::Gtk.Box.BoxChild w45 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.includePathEntry]));
			w45.Position = 0;
			// Container child hbox3.Gtk.Box+BoxChild
			this.quickInsertIncludeButton = new global::Gtk.Button ();
			this.quickInsertIncludeButton.TooltipMarkup = "Insert a macro.";
			this.quickInsertIncludeButton.CanFocus = true;
			this.quickInsertIncludeButton.Name = "quickInsertIncludeButton";
			this.quickInsertIncludeButton.UseUnderline = true;
			this.quickInsertIncludeButton.Label = global::Mono.Unix.Catalog.GetString (">");
			this.hbox3.Add (this.quickInsertIncludeButton);
			global::Gtk.Box.BoxChild w46 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.quickInsertIncludeButton]));
			w46.Position = 1;
			w46.Expand = false;
			w46.Fill = false;
			this.table3.Add (this.hbox3);
			global::Gtk.Table.TableChild w47 = ((global::Gtk.Table.TableChild)(this.table3 [this.hbox3]));
			w47.LeftAttach = ((uint)(1));
			w47.RightAttach = ((uint)(2));
			w47.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.includePathAddButton = new global::Gtk.Button ();
			this.includePathAddButton.Sensitive = false;
			this.includePathAddButton.CanFocus = true;
			this.includePathAddButton.Name = "includePathAddButton";
			this.includePathAddButton.UseUnderline = true;
			this.includePathAddButton.Label = global::Mono.Unix.Catalog.GetString ("Add");
			this.table3.Add (this.includePathAddButton);
			global::Gtk.Table.TableChild w48 = ((global::Gtk.Table.TableChild)(this.table3 [this.includePathAddButton]));
			w48.LeftAttach = ((uint)(2));
			w48.RightAttach = ((uint)(3));
			w48.XOptions = ((global::Gtk.AttachOptions)(4));
			w48.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.label9 = new global::Gtk.Label ();
			this.label9.Name = "label9";
			this.label9.LabelProp = global::Mono.Unix.Catalog.GetString ("Include:");
			this.table3.Add (this.label9);
			global::Gtk.Table.TableChild w49 = ((global::Gtk.Table.TableChild)(this.table3 [this.label9]));
			w49.XOptions = ((global::Gtk.AttachOptions)(4));
			w49.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table3.Gtk.Table+TableChild
			this.scrolledwindow2 = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow2.CanFocus = true;
			this.scrolledwindow2.Name = "scrolledwindow2";
			this.scrolledwindow2.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow2.Gtk.Container+ContainerChild
			this.includePathTreeView = new global::Gtk.TreeView ();
			this.includePathTreeView.CanFocus = true;
			this.includePathTreeView.Name = "includePathTreeView";
			this.scrolledwindow2.Add (this.includePathTreeView);
			this.table3.Add (this.scrolledwindow2);
			global::Gtk.Table.TableChild w51 = ((global::Gtk.Table.TableChild)(this.table3 [this.scrolledwindow2]));
			w51.TopAttach = ((uint)(1));
			w51.BottomAttach = ((uint)(2));
			w51.LeftAttach = ((uint)(1));
			w51.RightAttach = ((uint)(2));
			// Container child table3.Gtk.Table+TableChild
			this.vbox5 = new global::Gtk.VBox ();
			this.vbox5.Name = "vbox5";
			this.vbox5.Spacing = 6;
			// Container child vbox5.Gtk.Box+BoxChild
			this.includePathBrowseButton = new global::Gtk.Button ();
			this.includePathBrowseButton.CanFocus = true;
			this.includePathBrowseButton.Name = "includePathBrowseButton";
			this.includePathBrowseButton.UseUnderline = true;
			this.includePathBrowseButton.Label = global::Mono.Unix.Catalog.GetString ("Browse...");
			this.vbox5.Add (this.includePathBrowseButton);
			global::Gtk.Box.BoxChild w52 = ((global::Gtk.Box.BoxChild)(this.vbox5 [this.includePathBrowseButton]));
			w52.Position = 0;
			w52.Expand = false;
			w52.Fill = false;
			// Container child vbox5.Gtk.Box+BoxChild
			this.includePathRemoveButton = new global::Gtk.Button ();
			this.includePathRemoveButton.Sensitive = false;
			this.includePathRemoveButton.CanFocus = true;
			this.includePathRemoveButton.Name = "includePathRemoveButton";
			this.includePathRemoveButton.UseUnderline = true;
			this.includePathRemoveButton.Label = global::Mono.Unix.Catalog.GetString ("Remove");
			this.vbox5.Add (this.includePathRemoveButton);
			global::Gtk.Box.BoxChild w53 = ((global::Gtk.Box.BoxChild)(this.vbox5 [this.includePathRemoveButton]));
			w53.Position = 1;
			w53.Expand = false;
			w53.Fill = false;
			this.table3.Add (this.vbox5);
			global::Gtk.Table.TableChild w54 = ((global::Gtk.Table.TableChild)(this.table3 [this.vbox5]));
			w54.TopAttach = ((uint)(1));
			w54.BottomAttach = ((uint)(2));
			w54.LeftAttach = ((uint)(2));
			w54.RightAttach = ((uint)(3));
			w54.XOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox7.Add (this.table3);
			global::Gtk.Box.BoxChild w55 = ((global::Gtk.Box.BoxChild)(this.vbox7 [this.table3]));
			w55.Position = 1;
			this.notebook1.Add (this.vbox7);
			global::Gtk.Notebook.NotebookChild w56 = ((global::Gtk.Notebook.NotebookChild)(this.notebook1 [this.vbox7]));
			w56.Position = 2;
			// Notebook tab
			this.label3 = new global::Gtk.Label ();
			this.label3.Name = "label3";
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString ("Paths");
			this.notebook1.SetTabLabel (this.vbox7, this.label3);
			this.label3.ShowAll ();
			this.Add (this.notebook1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Show ();
			this.browseButton.Clicked += new global::System.EventHandler (this.OnBrowseButtonClick);
			this.removeLibButton.Clicked += new global::System.EventHandler (this.OnRemoveLibButtonClicked);
			this.removeLibButton.Clicked += new global::System.EventHandler (this.OnLibRemoved);
			this.libTreeView.CursorChanged += new global::System.EventHandler (this.OnLibTreeViewCursorChanged);
			this.libAddEntry.Changed += new global::System.EventHandler (this.OnLibAddEntryChanged);
			this.libAddEntry.Activated += new global::System.EventHandler (this.OnLibAddEntryActivated);
			this.addLibButton.Clicked += new global::System.EventHandler (this.OnLibAdded);
			this.libPathBrowseButton.Clicked += new global::System.EventHandler (this.OnLibPathBrowseButtonClick);
			this.libPathRemoveButton.Clicked += new global::System.EventHandler (this.OnLibPathRemoveButtonClicked);
			this.libPathRemoveButton.Clicked += new global::System.EventHandler (this.OnLibPathRemoved);
			this.libPathTreeView.CursorChanged += new global::System.EventHandler (this.OnLibPathTreeViewCursorChanged);
			this.libPathAddButton.Clicked += new global::System.EventHandler (this.OnLibPathAdded);
			this.libPathEntry.Changed += new global::System.EventHandler (this.OnLibPathEntryChanged);
			this.libPathEntry.Activated += new global::System.EventHandler (this.OnLibPathEntryActivated);
			this.includePathBrowseButton.Clicked += new global::System.EventHandler (this.OnIncludePathBrowseButtonClick);
			this.includePathRemoveButton.Clicked += new global::System.EventHandler (this.OnIncludePathRemoveButtonClicked);
			this.includePathRemoveButton.Clicked += new global::System.EventHandler (this.OnIncludePathRemoved);
			this.includePathTreeView.CursorChanged += new global::System.EventHandler (this.OnIncludePathTreeViewCursorChanged);
			this.includePathAddButton.Clicked += new global::System.EventHandler (this.OnIncludePathAdded);
			this.includePathEntry.Changed += new global::System.EventHandler (this.OnIncludePathEntryChanged);
			this.includePathEntry.Activated += new global::System.EventHandler (this.OnIncludePathEntryActivated);
		}
	}
}
