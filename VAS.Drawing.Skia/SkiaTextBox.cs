//
//  Copyright (C) 2017 Fluendo S.A.
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
namespace VAS.Drawing.Skia
{
	public static class SkiaTextBox
	{

		public static void Draw (string text, double x, double y, double width, double height, SKCanvas canvas, SKPaint paint)
		{
			double textY = 0, textX = 0;

			switch (paint.TextAlign) {
			case SKTextAlign.Center:
				textX = x + width / 2;
				break;
			case SKTextAlign.Left:
				textX = x;
				break;
			case SKTextAlign.Right:
				textX = x + width;
				break;
			}

			var lines = BreakLines (text, paint, width);

			var metrics = paint.FontMetrics;
			var lineHeight = metrics.Bottom - metrics.Top;

			float textHeight = lines.Count * lineHeight - metrics.Leading;

			if (textHeight > height) {
				textY = y - metrics.Top;
			} else {
				textY = y - metrics.Top + (height - textHeight) / 2;
			}

			for (int i = 0; i < lines.Count; i++) {
				canvas.DrawText (lines [i], (float)textX, (float)textY, paint);
				textY += lineHeight;
				if (textY + metrics.Descent > y + height) {
					break;
				}
			}
		}

		static List<string> BreakLines (string text, SKPaint paint, double width)
		{
			List<string> lines = new List<string> ();

			string remainingText = text.Trim ();

			do {
				int idx = LineBreak (remainingText, paint, width);
				if (idx == 0) {
					break;
				}
				var lastLine = remainingText.Substring (0, idx).Trim ();
				lines.Add (lastLine);
				remainingText = remainingText.Substring (idx).Trim ();
			} while (!string.IsNullOrEmpty (remainingText));
			return lines;
		}

		static int LineBreak (string text, SKPaint paint, double width)
		{
			int idx = 0, last = 0;
			int lengthBreak = (int)paint.BreakText (text, (float)width);
			// FIXME: There seems to be a bug in Skia and BreakText mesasures text width in a wrong way
			// leading to lines that exceeds the box width. This can be proven comparing the measured width
			// from BreakText and paint.MesasureText, tha later calculating it correctly.

			while (idx < text.Length) {
				int next = text.IndexOfAny (new char [] { ' ', '\n' }, idx);
				if (next == -1) {
					if (idx == 0) {
						// Word is too long, we will have to break it
						return lengthBreak;
					} else {
						// Ellipsize if it's the last line
						if (lengthBreak == text.Length
						// || text.IndexOfAny (new char [] { ' ', '\n' }, lengthBreak + 1) == -1
						) {
							return lengthBreak;
						}
						// Split at the last word;
						return last;
					}
				}
				if (text [idx] == '\n') {
					return idx;
				}
				if (idx > lengthBreak) {
					return last;
				}
				last = next;
				idx = next + 1;
			}
			return last;
		}
	}
}