//
//  Copyright (C) 2016 Fluendo S.A.
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
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;
namespace VAS.Core.ViewModel
{
	/// <summary>
	/// A ViewModel for <see cref="MediaFile"/>
	/// </summary>
	public class MediaFileVM : ViewModelBase<MediaFile>
	{
		/// <summary>
		/// Path of the media file.
		/// </summary>
		/// <value>The file path.</value>
		public string FilePath {
			get {
				return Model.FilePath;
			}
		}

		/// <summary>
		/// Duration of the file.
		/// </summary>
		/// <value>The duration.</value>
		public Time Duration {
			get {
				return Model.Duration;
			}
		}

		/// <summary>
		/// Gets the container of the media file.
		/// </summary>
		/// <value>The container.</value>
		public string Container {
			get {
				return Model.Container;
			}
		}

		/// <summary>
		/// Gets the video codec.
		/// </summary>
		/// <value>The video codec.</value>
		public string VideoCodec {
			get {
				return Model.VideoCodec;
			}
		}

		/// <summary>
		/// Gets the audio codec.
		/// </summary>
		/// <value>The audio codec.</value>
		public string AudioCodec {
			get {
				return Model.AudioCodec;
			}
		}

		/// <summary>
		/// Gets the width of the video.
		/// </summary>
		/// <value>The width of the video.</value>
		public uint VideoWidth {
			get {
				return Model.VideoWidth;
			}
		}

		/// <summary>
		/// Gets the height of the video.
		/// </summary>
		/// <value>The height of the video.</value>
		public uint VideoHeight {
			get {
				return Model.VideoHeight;
			}
		}

		/// <summary>
		/// Gets the framerate of the video.
		/// </summary>
		/// <value>The fps.</value>
		public ushort Fps {
			get {
				return Model.Fps;
			}
		}

		/// <summary>
		/// Gets the par of the video.
		/// </summary>
		/// <value>The par.</value>
		public double Par {
			get {
				return Model.Par;
			}
		}

		/// <summary>
		/// Gets the preview image of the file.
		/// </summary>
		/// <value>The preview.</value>
		public Image Preview {
			get {
				return Model.Preview;
			}
		}

		/// <summary>
		/// Gets or sets the offset of this file.
		/// </summary>
		/// <value>The offset.</value>
		public Time Offset {
			get {
				return Model.Offset;
			}
			set {
				Model.Offset = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the media file.
		/// </summary>
		/// <value>The name.</value>
		public string Name {
			get {
				return Model.Name;
			}
			set {
				Model.Name = value;
			}
		}

		/// <summary>
		/// Gets the short description for the media file.
		/// </summary>
		/// <value>The short description.</value>
		public string ShortDescription {
			get {
				return Model.ShortDescription;
			}
		}
	}
}
