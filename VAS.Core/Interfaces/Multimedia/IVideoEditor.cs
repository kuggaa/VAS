// IVideoEditor.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using VAS.Core.Common;
using VAS.Core.Handlers;

namespace VAS.Core.Interfaces.Multimedia
{
	public interface IVideoEditor
	{
		event ProgressHandler Progress;
		event ErrorHandler Error;

		EncodingSettings EncodingSettings {
			set;
		}

		string TempDir {
			set;
		}

		void AddSegment (string filePath, long start, long duration, double rate, string title, bool hasAudio, Area roi);

		void AddImageSegment (string filePath, long start, long duration, string title, Area roi);

		/// <summary>
		/// Sets the watermark on rendered videos. X, Y and Height are relative values, normalized from 0 to 1,
		/// so that the watermark is correctly scalled independently of the output size.
		/// </summary>
		/// <param name="image">The Image used as watermak.</param>
		/// <param name="x">The relative x offset.</param>
		/// <param name="y">The relative y offset.</param>
		/// <param name="height">The relative Height.</param>
		void SetWatermark (Image image, double x, double y, double height);

		void ClearList ();

		void Start ();

		void Cancel ();

	}
}
