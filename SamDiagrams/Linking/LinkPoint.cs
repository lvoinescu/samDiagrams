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
using System.Drawing;
using SamDiagrams.Drawers.Links;
using SamDiagrams.Linking;

namespace SamDiagrams
{
	/// <summary>
	/// A simple class representing the end point of a link described
	/// by a cartesian pair (x, y).
	/// </summary>
	public class LinkPoint
	{
		private int x = 0;
		private int y = 0;
		protected LinkDrawing linkDrawing;
		
		public LinkDrawing Link {
			get { return linkDrawing; }
			set { linkDrawing = value; }
		}
		
		public int X {
			get { return x; }
			set { x = value; }
		}

		public int Y {
			get { return y; }
			set { y = value; }
		}

		public LinkPoint(LinkDrawing linkDrawing)
		{
			this.linkDrawing = linkDrawing;
		}

		public Point Location{
			get {
				return new Point(x, y);
			}
		}

		public static void Swap(LinkPoint p1, LinkPoint p2)
		{
			int t = 0;
			t = p1.X;
			p1.X = p2.X;
			p2.X = t;

			t = p1.Y;
			p1.Y = p2.Y;
			p2.y = t;
		}

	}
}
