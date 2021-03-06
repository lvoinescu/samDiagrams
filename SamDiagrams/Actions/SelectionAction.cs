﻿/*
 * Created by SharpDevelop.
 * User: Sam
 * Date: 8/5/2015
 * Time: 10:56 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SamDiagrams.Drawings;
using SamDiagrams.Drawings.Selection;

namespace SamDiagrams.Actions
{
	/// <summary>
	/// Description of MouseHandler.
	/// </summary>
	public class SelectionAction : MouseAction
	{
		public event SelectedItemsChangedHandler SelectedItemsChanged;
		DiagramContainer container;
		private List<SelectableDrawing> selectedDrawings;
		private List<SelectableDrawing> removedFromSelection;
		
		public SelectionAction(DiagramContainer container)
		{
			this.container = container;
			selectedDrawings = new List<SelectableDrawing>();
			removedFromSelection = new List<SelectableDrawing>();
			
		}

		public void OnMouseDown(object sender, MouseEventArgs e)
		{
			bool drawingFound = false;
			float scaleFactor = (float)container.ZoomFactor / 100;
			
			Point scaledMouseLocation = new Point((int)((double)e.Location.X / scaleFactor), (int)((double)e.Location.Y / scaleFactor));
			scaledMouseLocation.Offset(container.HScrollBar.Value, container.VScrollBar.Value);
			
			for (int i = 0; i < container.ContainerDrawer.Drawings.Count && !drawingFound; i++) {
				SelectableDrawing selectableDrawing = container.ContainerDrawer.Drawings[i] as SelectableDrawing;

				if (selectableDrawing != null) {
					if (selectableDrawing.Bounds.Contains(scaledMouseLocation)) {
						drawingFound = true;
						if (!selectedDrawings.Contains(selectableDrawing)) {
							if (Control.ModifierKeys != Keys.Control) {
								clearSelections();
								bringToFront(selectableDrawing);
								toggleSelection(selectableDrawing);
							} else {
								bringToFront(selectableDrawing);
								addSelected(selectableDrawing);
							}
							if (SelectedItemsChanged != null)
								SelectedItemsChanged(this, new SelectedItemsChangedArgs(selectedDrawings, removedFromSelection));
						}

						break;
					} 
				}
			}
			
			if (!drawingFound) {
				clearSelections();
				if (SelectedItemsChanged != null)
					SelectedItemsChanged(this, new SelectedItemsChangedArgs(selectedDrawings, removedFromSelection));
			}
		}

		private void clearSelections()
		{
			removedFromSelection = new List<SelectableDrawing>();
			removedFromSelection.AddRange(selectedDrawings);
			foreach (SelectableDrawing drawing in selectedDrawings) {
				drawing.Selected = false;
			}
			selectedDrawings.Clear();
		}
		
		private void toggleSelection(SelectableDrawing selectableDrawing)
		{
			if (!selectedDrawings.Contains(selectableDrawing)) {
				addSelected(selectableDrawing);
			} else {
				removeSelected(selectableDrawing);
			}
		}
		
		private void addSelected(SelectableDrawing selectedDrawing)
		{
			if (!selectedDrawings.Contains(selectedDrawing)) {
				selectedDrawing.Selected = true;
				selectedDrawings.Add(selectedDrawing);
				container.ContainerDrawer.SelectedDrawing.Add(selectedDrawing);
			}
		}
		
		private void removeSelected(SelectableDrawing selectedDrawing)
		{
			if (selectedDrawings.Contains(selectedDrawing)) {
				selectedDrawing.Selected = false;
				selectedDrawings.Remove(selectedDrawing);
				container.ContainerDrawer.SelectedDrawing.Remove(selectedDrawing);
				removedFromSelection.Remove(selectedDrawing);
			}
		}
		
		private void bringToFront(SelectableDrawing drawing)
		{
			container.ContainerDrawer.Drawings.Remove(drawing);
			container.ContainerDrawer.Drawings.Add(drawing);
		}
		
		public void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
		}

		public void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
		}

	}
}
