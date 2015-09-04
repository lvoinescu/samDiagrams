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
using SamDiagrams.Drawers.Links;
using SamDiagrams.Drawings;
using SamDiagrams.Drawings.Geometry;
using SamDiagrams.Drawings.Link;
using SamDiagrams.Drawings.Selection;

namespace SamDiagrams.Drawers
{
	/// <summary>
	/// Description of InvalidationStrategy.
	/// </summary>
	public class InvalidationStrategy
	{
		
		readonly ContainerDrawer containerDrawer;
		private Rectangle previouslyInvalidatedRectangle;
		
		public InvalidationStrategy(ContainerDrawer container)
		{
			this.containerDrawer = container;
			this.containerDrawer.ActionListener.ItemsMoved += new ItemsMovedHandler(OnItemsMoved);
			this.containerDrawer.ActionListener.SelectionChanged +=	new SelectedItemsChangedHandler(OnSelectionChanged);
			this.containerDrawer.LinkOrchestrator.linkStrategy.LinkDirectionChangedEvent +=
				new LinkDirectionChangedHandler(OnLinkDirectionChanged);
			this.containerDrawer.DiagramContainer.Paint += new PaintEventHandler(OnPaint);
		}

		/// <summary>
		/// This method handles drawing invalidation that is not triggered by
		/// selections or movement; for instance: control resize, form overlapping, etc.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnPaint(object sender, PaintEventArgs e)
		{
			foreach (IDrawing drawing in containerDrawer.Drawings) {
				if (!drawing.Invalidated && e.ClipRectangle.IntersectsWith(drawing.Bounds))
					drawing.Invalidated = true;
			}
		}

		void OnMouseDown(object sender, MouseEventArgs e)
		{
			foreach (IDrawing drawer in containerDrawer.Drawings) {
				drawer.Invalidated = true;
			}
		}

		void OnSelectionChanged(object sender, SelectedItemsChangedArgs e)
		{
			List<IDrawing> invalidatedGroup = new List<IDrawing>();
			foreach (SelectableDrawing selectableDrawing in e.SelectedDrawings) {
				invalidatedGroup.Add(selectableDrawing.Drawing);
			}
			
			foreach (SelectableDrawing selectableDrawing in e.PreviouslySelectedDrawings) {
				invalidatedGroup.Add(selectableDrawing.Drawing);
			}
			InvalidateDrawingGroup(invalidatedGroup);
		}

		void OnItemsMoved(object sender, ItemsMovedEventArg e)
		{
			InvalidateDrawingGroup(e.Items);
		}

		void OnLinkDirectionChanged(object sender, LinkDirectionChangedArg e)
		{
			InvalidateLinkDrawing((LinkDrawing)e.Link);
		}
		
		void InvalidateDrawingGroup(List<IDrawing> drawings)
		{
			if (drawings.Count < 1)
				return;
			
			InflatableRectangle newRectangleToInvalidate = new InflatableRectangle(drawings[0].InvalidatedRegion);
			foreach (IDrawing drawing in drawings) {
				drawing.Invalidated = true;
				newRectangleToInvalidate.Inflate(drawing.InvalidatedRegion);
			}
			
			Rectangle auxRectangle = newRectangleToInvalidate.Bounds;

			newRectangleToInvalidate.Inflate(previouslyInvalidatedRectangle);
			invalidateOverlappingDrawings(newRectangleToInvalidate.Bounds);
			previouslyInvalidatedRectangle = auxRectangle;
			
			containerDrawer.DiagramContainer.Invalidate(newRectangleToInvalidate.Bounds);

		}
		
		void InvalidateDrawing(IDrawing drawing)
		{
			drawing.Invalidated = true;
			InflatableRectangle newRectangleToInvalidate = new InflatableRectangle(drawing.InvalidatedRegion);
			Rectangle auxRectangle = newRectangleToInvalidate.Bounds;
			invalidateOverlappingDrawings(previouslyInvalidatedRectangle);
			newRectangleToInvalidate.Inflate(previouslyInvalidatedRectangle);
			previouslyInvalidatedRectangle = auxRectangle;
			invalidateOverlappingDrawings(newRectangleToInvalidate.Bounds);
			
			containerDrawer.DiagramContainer.Invalidate(newRectangleToInvalidate.Bounds);
		}
		
		void InvalidateLinkDrawing(LinkDrawing linkDrawing)
		{
			InflatableRectangle newRectangleToInvalidate = new InflatableRectangle(linkDrawing.Bounds);
			newRectangleToInvalidate.Inflate(getLinkInvalidatedRegion(linkDrawing));
			newRectangleToInvalidate.Inflate(
				getStructureInvalidatedRegion(linkDrawing.SourceDrawing));
			newRectangleToInvalidate.Inflate(
				getStructureInvalidatedRegion(linkDrawing.DestinationDrawing));
			Rectangle auxRectangle = newRectangleToInvalidate.Bounds;
			invalidateOverlappingDrawings(previouslyInvalidatedRectangle);
			newRectangleToInvalidate.Inflate(previouslyInvalidatedRectangle);
			invalidateOverlappingDrawings(newRectangleToInvalidate.Bounds);
			previouslyInvalidatedRectangle = auxRectangle;
			
			containerDrawer.DiagramContainer.Invalidate(newRectangleToInvalidate.Bounds);
			previouslyInvalidatedRectangle = newRectangleToInvalidate.Bounds;
			//containerDrawer.DiagramContainer.Invalidate();
		}
		
		Rectangle getStructureInvalidatedRegion(IDrawing targetDrawing)
		{
			targetDrawing.Invalidated = true;
			InflatableRectangle rectangle = new InflatableRectangle(targetDrawing.InvalidatedRegion);

			for (int i = 0; i < containerDrawer.Drawings.Count; i++) {
				IDrawing drawing = containerDrawer.Drawings[i];
				if (drawing.Bounds.IntersectsWith(rectangle.Bounds) && drawing.Invalidated == false) {
					drawing.Invalidated = true;
					rectangle.Inflate(drawing.Bounds);
				}
			}
			return rectangle.Bounds;
		}
		
		
		Rectangle getLinkInvalidatedRegion(LinkDrawing linkDrawing)
		{
			InflatableRectangle rectangle = new InflatableRectangle(linkDrawing.Bounds);
			linkDrawing.Invalidated = true;
			ILinkableDrawing sourceDrawing = linkDrawing.SourceDrawing;
			ILinkableDrawing destinationDrawing = linkDrawing.DestinationDrawing;
			sourceDrawing.Invalidated = true;
			destinationDrawing.Invalidated = true;
			rectangle.Inflate(sourceDrawing.InvalidatedRegion);
			rectangle.Inflate(sourceDrawing.InvalidatedRegion);
			
 
			
			foreach (IDrawing drawing in containerDrawer.Drawings) {
				if (drawing.Bounds.IntersectsWith(rectangle.Bounds)) {
					drawing.Invalidated = true;
					rectangle.Inflate(drawing.Bounds);
				}
			}
			return rectangle.Bounds;
		}

		void invalidateOverlappingDrawings(Rectangle rectangle)
		{
			foreach (IDrawing drawing in containerDrawer.Drawings) {
				if (!drawing.Invalidated && drawing.Bounds.IntersectsWith(rectangle)) {
					drawing.Invalidated = true;
				}
			}
		}
		
	}
}
