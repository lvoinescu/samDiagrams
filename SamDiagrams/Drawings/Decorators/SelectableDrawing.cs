﻿/*
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
using System.Drawing;
using System.Windows.Forms;
using SamDiagrams.Drawings.Geometry;
using SamDiagrams.Events;
using SamDiagrams.Model;

namespace SamDiagrams.Drawings.Selection
{
	/// <summary>
	/// A Decorator that handles drawing the selection border
	/// </summary>
	public class SelectableDrawing : IDrawing, IClickable, IResizable
	{
		public event DrawingResizedHandler DrawingResized;


		readonly IDrawing drawing;
		SelectionBorder selectionBorder;
		Point initialSelectedLocation;
		
		public IDrawing Drawing {
			get {
				return drawing;
			}
		}

		public Item Item {
			get {
				return this.drawing.Item;
			}
		}
		
		public List<IDrawing> Components {
			get {
				return drawing.Components;
			}
		}
		
		public Color Color {
			get {
				return drawing.Color;
			}
			set {
				drawing.Color = value;
			}
		}

		public Point Location {
			get {
				return selectionBorder.Bounds.Location;
			}
			set {
				Point p = new Point(value.X, value.Y);
				int dx = (selectionBorder.Bounds.Width - this.drawing.Bounds.Width) / 2;
				int dy = (selectionBorder.Bounds.Height - this.drawing.Bounds.Height) / 2;
				p.Offset(dx, dy);
				drawing.Location = p;
			}
		}
		
		public Size Size {
			get {
				return selectionBorder.Bounds.Size;
			}
		}
		
		public bool Invalidated {
			get {
				return drawing.Invalidated;
			}
			set {
				drawing.Invalidated = value;
			}
		}

		public Rectangle Bounds {
			get {
				return selectionBorder.Bounds;
			}
		}

		public bool Movable {
			get {
				return drawing.Movable;
			}
			set {
				drawing.Movable = value;
			}
		}
		
		public Rectangle InvalidatedRegion {
			get {
				MergeableRectangle rectangle = new MergeableRectangle(selectionBorder.Bounds);
				rectangle.Merge(this.drawing.InvalidatedRegion);
				rectangle.Inflate(2);
				return rectangle.Bounds;
			}
		}
		
		public bool Selected {
			get {
				return this.drawing.Selected;
			}
			set {
				this.drawing.Selected = value;
			}
		}
		
		public Point InitialSelectedLocation {
			get {
				return initialSelectedLocation;
			}
			set {
				this.initialSelectedLocation = value;
			}
		}
		
		public SelectableDrawing(IDrawing drawing)
		{
			this.drawing = drawing;
			this.selectionBorder = new SelectionBorder(drawing);
			if (drawing is IResizable) {
				(drawing as IResizable).DrawingResized += new DrawingResizedHandler(OnDrawingResized);
			}
		}
		
		public void Draw(Graphics graphics)
		{
			this.drawing.Draw(graphics);
			if (drawing.Selected) {
				this.selectionBorder.Draw(graphics);
			}
		}

		public void OnInsideClick(MouseEventArgs e)
		{
			if (drawing is IClickable) {
				Point drawingPointClick = new Point(e.X + Location.X - drawing.Location.X,
					                          e.Y + Location.Y - drawing.Location.Y);
				(drawing as IClickable).OnInsideClick(new MouseEventArgs(e.Button, e.Clicks, 
					drawingPointClick.X, drawingPointClick.Y, e.Delta));
			}
		}
		
		void OnDrawingResized(object sender, SamDiagrams.Events.DrawingResizedEventArgs e)
		{
			if (DrawingResized != null) {
				
				Rectangle previousSizeRectangle = new Rectangle(e.PreviousBounds.Location, e.PreviousBounds.Size);
				previousSizeRectangle.Inflate(SelectionBorder.CORNER_SQUARE_SIZE, SelectionBorder.CORNER_SQUARE_SIZE);
				
				Rectangle newSizeRectangle = new Rectangle(drawing.Location, e.Drawing.Size);
				newSizeRectangle.Inflate(SelectionBorder.CORNER_SQUARE_SIZE, SelectionBorder.CORNER_SQUARE_SIZE);
				
				DrawingResized(this, new DrawingResizedEventArgs(this, previousSizeRectangle, newSizeRectangle));
			}
		}
		
		public override String ToString()
		{
			return "SelectableDrawing; " + drawing.ToString();
		}
		
	}
}
