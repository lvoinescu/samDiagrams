/*
 *   SamDiagrams - diagram component for .NET
 *   Copyright (C) 2011  Lucian Voinescu
 *
 *   This file is part of SamDiagrams
 *
 *   SamDiagrams is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Lesser General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   SamDiagrams is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Lesser General Public License for more details.
 *
 *   You should have received a copy of the GNU Lesser General Public License
 *   along with SamDiagrams. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using SamDiagrams.DiagramItem.NodeEditor;
using SamDiagrams.Drawers;
using SamDiagrams.Drawers.Links;
using SamDiagrams.Drawings;
using SamDiagrams.Drawings.Link;
using SamDiagrams.Drawings.Selection;
using SamDiagrams.Events;
using SamDiagrams.Linking;
using SamDiagrams.Model;
using SamDiagrams.Model.Link;

namespace SamDiagrams
{
	/// <summary>
	/// Description of DiagramContainer.
	/// </summary>
	public class DiagramContainer : UserControl
	{

		public event DiagramItemClickHandler DiagramItemClick;
		public event ZoomFactorChangedHandler ZoomFactorChanged;
		public event DrawingResizedHandler DrawingResized;
		
		
		private List<StructureDrawing> selectedItems;
		List<ILinkable> structures;
		private int zoomFactor;
		private float scaleFactor = 1;
		private int gridSize = 16;
		private bool snapObjectsToGrid = false;
		private int drawableHeight, drawableWidth;

		private bool autoSizeItem = true;
		private Rectangle invalidatedRegion = new Rectangle();
		private Region oldIvalidatedRegion = new Region();
		private NodeTextEditor nodeEditor;
		private ContainerDrawer containerDrawer;
		
		public NodeTextEditor NodeEditor {
			get { return nodeEditor; }
		}


		public bool AutoSizeItem {
			get { return autoSizeItem; }
			set {
				autoSizeItem = value;
				foreach (StructureDrawing drawer in containerDrawer.Drawings)
					if (drawer is StructureDrawing)
						((StructureDrawing)drawer).AutoSizeContent();
			}
		}

		public ContainerDrawer ContainerDrawer {
			get {
				return containerDrawer;
			}
		}
 
		public int DrawableWidth {
			get { return drawableWidth; }
			set {
				drawableWidth = value;
				SetScrollBarValues();
			}
		}

		public int DrawableHeight {
			get { return drawableHeight; }
			set {
				drawableHeight = value;
				SetScrollBarValues();
			}
		}

		HScrollBar hScrollBar = new HScrollBar();

		public HScrollBar HScrollBar {
			get {
				return hScrollBar;
			}
			set {
				hScrollBar = value;
			}
		}

		VScrollBar vScrollBar = new VScrollBar();

		public VScrollBar VScrollBar {
			get {
				return vScrollBar;
			}
			set {
				vScrollBar = value;
			}
		}
		public bool SnapObjectsToGrid {
			get { return snapObjectsToGrid; }
			set { snapObjectsToGrid = value; }
		}
		public int ZoomFactor {
			get { return zoomFactor; }
			set {
				zoomFactor = value;
				scaleFactor = (float)zoomFactor / 100;
				OnZoomChanged();
			}
		}



		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<ILinkable> DiagramItems {
			get { return structures; }
			set { structures = value; }
		}


		public DiagramContainer()
		{
			zoomFactor = 100;
			structures = new List<ILinkable>();
			selectedItems = new List<StructureDrawing>();
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
			ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);
			hScrollBar = new HScrollBar();
			hScrollBar.Dock = DockStyle.Bottom;
			vScrollBar = new VScrollBar();
			vScrollBar.Dock = DockStyle.Right;
			vScrollBar.Scroll += new ScrollEventHandler(vScrollBar1_Scroll);
			hScrollBar.Scroll += new ScrollEventHandler(hScrollBar1_Scroll);
			this.Resize += new EventHandler(DiagramContainer_Resize);
			this.Controls.Add(hScrollBar);
			this.Controls.Add(vScrollBar);
			nodeEditor = new NodeTextEditor(this);
			containerDrawer = new ContainerDrawer(this);
			this.Invalidate();
		}

 

		protected override void InitLayout()
		{
			base.InitLayout();
			this.drawableHeight = this.Height;
			this.drawableWidth = this.Width;
			SetScrollBarValues();
		}

		void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
		{
			this.Invalidate();
		}

		void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
		{
			this.Invalidate();
		}

		public void AddStructure(Structure structure, Color color)
		{
			StructureDrawing structureDrawing = new StructureDrawing(structure, color);
			structure.Drawing = structureDrawing;
			structureDrawing.Invalidated = true;
			
			
			
			SelectableDrawing selectableDrawing = new SelectableDrawing(structureDrawing);
			selectableDrawing.DrawingResized += new DrawingResizedHandler(OnDrawingResized);
			containerDrawer.Drawings.Insert(0, selectableDrawing);
			structures.Add(structure);
			structureDrawing.AutoSizeContent();
		}
		
		public void AddItem(Item item, IDrawing drawing, bool selectable, bool movable)
		{
			IDrawing drawingToAdd;
			if (selectable) {
				drawingToAdd = new SelectableDrawing(drawing);

			} else {
				drawingToAdd = drawing;
			}
			item.Drawing = drawing;
			containerDrawer.Drawings.Add(drawingToAdd);
		}
		
		public void AddDrawing(IDrawing drawing, bool selectable)
		{
			IDrawing drawingToAdd;
			if (selectable) {
				drawingToAdd = new SelectableDrawing(drawing);

			} else {
				drawingToAdd = drawing;
			}
			containerDrawer.Drawings.Add(drawingToAdd);
		}
		
		public void AddLink(ILinkable source, ILinkable destination)
		{
			Link link = new Link(source, destination, Color.Black);
			ContainerDrawer.LinkOrchestrator.AddLink(link);
		}
		
		public void AddLinkDrawing(ILinkableDrawing source, ILinkableDrawing destination)
		{
			LinkDrawing linkDrawing = new LinkDrawing(source, destination, 1.0f, 3.0f, LinkStyle.StreightLines);
			ContainerDrawer.LinkOrchestrator.AddLinkDrawing(linkDrawing);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			try {
				containerDrawer.Draw(scaleFactor, e.Graphics);
				base.OnPaint(e);
			} catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		void OnDrawingResized(object sender, DrawingResizedEventArgs e)
		{
			if (DrawingResized != null) {
				DrawingResized(this, e);
			}
		}
		protected void OnZoomChanged()
		{
			SetScrollBarValues();
			if (ZoomFactorChanged != null)
				this.ZoomFactorChanged(this, new ZoomFactorChangedArg(zoomFactor));
			invalidatedRegion = new Rectangle(this.Location, this.Size);
		}

		

		
		private bool checkEditorInsideMouseDown(MouseEventArgs e, double scaleFactor)
		{
			if (nodeEditor.Visible) {
				Point p = new Point((int)(e.Location.X / scaleFactor), (int)(e.Location.Y / scaleFactor));
				p.Offset(hScrollBar.Value, vScrollBar.Value);
				if (!nodeEditor.Bounds.Contains(p)) {
					nodeEditor.Visible = false;
					nodeEditor.currentNode.StructureDrawing.Invalidated = true;
					return true;
				}
				nodeEditor.onMouseDown(nodeEditor, e, scaleFactor);
			}
			return false;
		}
		

 

		private void textEditor_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter) {
 
			}
		}

		private void textEditor_LostFocus(object sender, EventArgs e)
		{


		}

		private void InvalidateTransform(Region region)
		{
			Matrix mx = new Matrix();
			mx.Scale(scaleFactor, scaleFactor);
			mx.Translate(-hScrollBar.Value, -vScrollBar.Value);
			region.Transform(mx);
			Invalidate(region);
		}


		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// DiagramContainer
			// 
			this.Size = new System.Drawing.Size(1000, 800);
			this.Resize += new System.EventHandler(this.DiagramContainer_Resize);
			this.ResumeLayout(false);

		}

		private void DiagramContainer_Resize(object sender, EventArgs e)
		{
			SetScrollBarValues();
		}

		private void SetScrollBarValues()
		{

			if (scaleFactor * drawableWidth > Width) {
				hScrollBar.Minimum = 0;
				hScrollBar.Maximum = (int)(scaleFactor * drawableWidth - Width);
				hScrollBar.SmallChange = (int)(scaleFactor * drawableWidth / 20);
				hScrollBar.LargeChange = (int)(scaleFactor * drawableWidth / 10);
				if (this.vScrollBar.Visible) { //step 2
					this.hScrollBar.Maximum += this.vScrollBar.Width;
				}
				hScrollBar.Maximum += this.hScrollBar.LargeChange;//step 3
				hScrollBar.Visible = true;
			} else {
				drawableWidth = Width;
				hScrollBar.Visible = false;
				hScrollBar.Value = 0;
			}

			if (scaleFactor * drawableHeight > Height) {
				vScrollBar.Minimum = 0;
				vScrollBar.Maximum = (int)((scaleFactor * drawableHeight - Height));
				vScrollBar.SmallChange = (int)(scaleFactor * drawableHeight / 20);
				vScrollBar.LargeChange = (int)(scaleFactor * drawableHeight / 10);
				if (this.hScrollBar.Visible) { //step 2
					this.vScrollBar.Maximum += this.hScrollBar.Height;
				}
				vScrollBar.Maximum += this.vScrollBar.LargeChange;//step 3
				vScrollBar.Visible = true;
			} else {
				drawableHeight = Height;
				vScrollBar.Visible = false;
				vScrollBar.Maximum = 0;
			}
		}
		
		
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			const int WM_KEYDOWN = 0x100;
			const int WM_SYSKEYDOWN = 0x104;
			
			if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN)) {
				switch (keyData) {
					case Keys.Down:
						this.Parent.Text = "Down Arrow Captured";
						break;
						
					case Keys.Up:
						this.Parent.Text = "Up Arrow Captured";
						break;
						
					case Keys.Tab:
						this.Parent.Text = "Tab Key Captured";
						break;
						
					case Keys.Control | Keys.M:
						this.Parent.Text = "<CTRL> + M Captured";
						break;
						
					case Keys.Alt | Keys.Z:
						this.Parent.Text = "<ALT> + Z Captured";
						break;
				}
			}
			if (nodeEditor.Visible)
				nodeEditor.OnKeyDown(new KeyEventArgs(keyData));
			return base.ProcessCmdKey(ref msg, keyData);
		}
		
	}
}
