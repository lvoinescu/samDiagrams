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
using SamDiagrams.Drawers.Links;
using SamDiagrams.Drawings.Link;
using SamDiagrams.Model;

namespace SamDiagrams.Drawings
{
	/// <summary>
	/// Description of BaseLinkableDrawing.
	/// </summary>
	public abstract class BaseLinkableDrawing : BaseDrawing, ILinkableDrawing
	{
		private List<LinkDrawing> drawingLinks;
		private Item item;
		
		protected BaseLinkableDrawing(Item item)
			: base()
		{
			this.item = item;
			this.drawingLinks = new List<LinkDrawing>();
		}
		
		
		public List<LinkDrawing> DrawingLinks {
			get {
				return this.drawingLinks;
			}
		}
		
		public Item Item {
			get {
				return this.item;
			}
		}

		public abstract Rectangle InvalidatedRegion {
			get;
		}
		
	}
}
