// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Collections.Generic;

namespace VAS.Core.Multimedia
{
	[Serializable]
	public class VideoStandard
	{
		public string Name;
		public uint Height;
		public uint Width;

		public VideoStandard ()
		{
		}

		public VideoStandard (string name, uint width, uint height)
		{
			Name = name;
			Height = height;
			Width = width;
		}

		public override bool Equals (object obj)
		{
			VideoStandard vstd;
			if (!(obj is VideoStandard))
				return false;
			vstd = (VideoStandard)obj;
			return vstd.Name == Name && vstd.Height == Height && vstd.Width == Width;
		}

		public override int GetHashCode ()
		{
			return String.Format ("{0}-{1}-{2}", Name, Width, Height).GetHashCode ();
		}
	}

	public class VideoStandards
	{
		public static VideoStandard Original = new VideoStandard (Catalog.GetString ("Keep original size"), 0, 0);
		public static VideoStandard P240 = new VideoStandard ("240p", 320, 240);
		public static VideoStandard P480 = new VideoStandard ("480p", 640, 480);
		public static VideoStandard P576 = new VideoStandard ("576p", 720, 576);
		public static VideoStandard P720 = new VideoStandard ("720p", 1280, 720);
		public static VideoStandard P1080 = new VideoStandard ("1080p", 1920, 1080);
		public static VideoStandard P240_4_3 = new VideoStandard ("240p (4:3)", 320, 240);
		public static VideoStandard P240_16_9 = new VideoStandard ("240p (16:9)", 426, 240);
		public static VideoStandard P480_4_3 = new VideoStandard ("480p (4:3)", 640, 480);
		public static VideoStandard P480_16_9 = new VideoStandard ("480p (16:9)", 854, 480);
		public static VideoStandard P720_4_3 = new VideoStandard ("720p (4:3)", 960, 720);
		public static VideoStandard P720_16_9 = new VideoStandard ("720p (16:9)", 1280, 720);
		public static VideoStandard P1080_4_3 = new VideoStandard ("1080p (4:3)", 1440, 1080);
		public static VideoStandard P1080_16_9 = new VideoStandard ("1080p (16:9)", 1920, 1080);

		public static List<VideoStandard> Rendering {
			get {
				List<VideoStandard> list = new List<VideoStandard> ();
				list.Add (P480_16_9);
				list.Add (P720_16_9);
				list.Add (P1080_16_9);
				return list;
			}
		}

		public static List<VideoStandard> Capture {
			get {
				List<VideoStandard> list = new List<VideoStandard> ();
				list.Add (P480);
				list.Add (P576);
				list.Add (P720);
				list.Add (P1080);
				return list;
			}
		}

		public static VideoStandard [] Transcode {
			get {
				return new VideoStandard [] {
					VideoStandards.P1080,
					VideoStandards.P720,
					VideoStandards.P480,
				};
			}
		}
	}

}

