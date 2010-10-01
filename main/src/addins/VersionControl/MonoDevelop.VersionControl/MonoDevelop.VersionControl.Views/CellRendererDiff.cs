
using System;
using Gtk;
using Gdk;
using MonoDevelop.Ide;

namespace MonoDevelop.VersionControl.Views
{
	class CellRendererDiff: Gtk.CellRendererText
	{
		Pango.Layout layout;
		Pango.FontDescription font;
		bool diffMode;
		int width, height, lineHeight;
		string[] lines;		
		int selectedLine = -1;
		TreePath selctedPath;
		TreePath path;
		
		public CellRendererDiff()
		{
			font = Pango.FontDescription.FromString (DesktopService.DefaultMonospaceFont);
		}
		
		void DisposeLayout ()
		{
			if (layout != null) {
				layout.Dispose ();
				layout = null;
			}
		}
		
		bool isDisposed = false;
		protected override void OnDestroyed ()
		{
			isDisposed = true;
			DisposeLayout ();
			if (font != null) {
				font.Dispose ();
				font = null;
			}
			base.OnDestroyed ();
		}
		
		public void Reset ()
		{
		}

		public void InitCell (Widget container, bool diffMode, string[] lines, TreePath path)
		{
			if (isDisposed)
				return;
			if (lines == null)
				throw new ArgumentNullException ("lines");
			this.lines = lines;
			this.diffMode = diffMode;
			this.path = path;
			
			if (diffMode) {
				if (lines != null && lines.Length > 0) {
					int maxlen = -1;
					int maxlin = -1;
					for (int n=0; n<lines.Length; n++) {
						string line = lines [n];
						if (line == null)
							throw new Exception ("Line " + n + " from diff was null.");
						if (line.Length > maxlen) {
							maxlen = lines [n].Length;
							maxlin = n;
						}
					}
					DisposeLayout ();
					layout = CreateLayout (container, lines [maxlin]);
					layout.GetPixelSize (out width, out lineHeight);
					height = lineHeight * lines.Length;
				}
				else
					width = height = 0;
			}
			else {
				DisposeLayout ();
				layout = CreateLayout (container, string.Join (Environment.NewLine, lines));
				layout.GetPixelSize (out width, out height);
			}
		}
		
		Pango.Layout CreateLayout (Widget container, string text)
		{
			Pango.Layout layout = new Pango.Layout (container.PangoContext);
			layout.SingleParagraphMode = false;
			if (diffMode) {
				layout.FontDescription = font;
				layout.SetText (text);
			}
			else
				layout.SetMarkup (text);
			return layout;
		}
		
		const int leftSpace = 16;
		public bool DrawLeft { get; set; }
		
		protected override void Render (Drawable window, Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, CellRendererState flags)
		{
			if (isDisposed)
				return;
			if (diffMode) {
				
				if (path.Equals (selctedPath)) {
					selectedLine = -1;
					selctedPath = null;
				}
				
				int w, maxy;
				window.GetSize (out w, out maxy);
				if (DrawLeft) {
					cell_area.Width += cell_area.X - leftSpace;
					cell_area.X = leftSpace;
				}
				var treeview = widget as FileTreeView;
				var p = treeview != null? treeview.CursorLocation : null;
				
				int recty = cell_area.Y;
				int recth = cell_area.Height - 1;
				if (recty < 0) {
					recth += recty + 1;
					recty = -1;
				}
				if (recth > maxy + 2)
					recth = maxy + 2;
				
				window.DrawRectangle (widget.Style.BaseGC (Gtk.StateType.Normal), true, cell_area.X, recty, cell_area.Width - 1, recth);

				Gdk.GC normalGC = widget.Style.TextGC (StateType.Normal);
				Gdk.GC removedGC = new Gdk.GC (window);
				removedGC.Copy (normalGC);
				removedGC.RgbFgColor = new Color (255, 0, 0);
				Gdk.GC addedGC = new Gdk.GC (window);
				addedGC.Copy (normalGC);
				addedGC.RgbFgColor = new Color (0, 0, 255);
				Gdk.GC infoGC = new Gdk.GC (window);
				infoGC.Copy (normalGC);
				infoGC.RgbFgColor = new Color (0xa5, 0x2a, 0x2a);
				
				int y = cell_area.Y + 2;
				int cline = 1;
				bool inHeader = true;
				
				for (int n=0; n<lines.Length; n++, y += lineHeight) {
					
					string line = lines [n];
					if (line.Length == 0)
						continue;
					
					char tag = line [0];

					// Keep track of the real file line
					int thisLine = cline;
					if (tag == '@') {
						int l = ParseCurrentLine (line);
						if (l != -1) cline = thisLine = l;
						inHeader = false;
					} else if (tag != '-' && !inHeader)
						cline++;
					
					if (y + lineHeight < 0)
						continue;
					if (y > maxy)
						break;
					
					Gdk.GC gc;
					switch (tag) {
						case '-': gc = removedGC; break;
						case '+': gc = addedGC; break;
						case '@': gc = infoGC; break;
						default: gc = normalGC; break;
					}

					if (p.HasValue && p.Value.X >= cell_area.X && p.Value.X <= cell_area.Right && p.Value.Y >= y && p.Value.Y < y + lineHeight) {
						window.DrawRectangle (widget.Style.BaseGC (Gtk.StateType.Prelight), true, cell_area.X, y, cell_area.Width - 1, lineHeight);
						selectedLine = thisLine;
						selctedPath = path;
					}
					
					layout.SetText (line);
					window.DrawLayout (gc, cell_area.X + 2, y, layout);
				}
				window.DrawRectangle (widget.Style.DarkGC (Gtk.StateType.Prelight), false, cell_area.X, recty, cell_area.Width - 1, recth);
				removedGC.Dispose ();
				addedGC.Dispose ();
				infoGC.Dispose ();
			} else {
				int y = cell_area.Y + (cell_area.Height - height)/2;
				window.DrawLayout (widget.Style.TextGC (GetState(flags)), cell_area.X, y, layout);
			}
		}
		
		public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int c_width, out int c_height)
		{
			x_offset = y_offset = 0;
			c_width = width;
			c_height = height;
			
			if (diffMode) {
				// Add some spacing for the margin
				c_width += 4;
				c_height += 4;
			}
		}
		
		StateType GetState (CellRendererState flags)
		{
			if ((flags & CellRendererState.Selected) != 0)
				return StateType.Selected;
			else
				return StateType.Normal;
		}
		
		int ParseCurrentLine (string line)
		{
			int i = line.IndexOf ('+');
			if (i == -1) return -1;
			i++;
			int j = line.IndexOf (',', i);
			if (j == -1) return -1;
			int cline;
			if (!int.TryParse (line.Substring (i, j - i), out cline))
			    return -1;
			return cline;
		}
		
		public int GetSelectedLine (TreePath cpath)
		{
			if (cpath.Equals (selctedPath))
				return selectedLine;
			else
				return -1;
		}
	}
}
