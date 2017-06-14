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

namespace VAS.Core.Common
{
	[Serializable]
	public struct EncodingSettings
	{
		public EncodingSettings (VideoStandard videoStandard, EncodingProfile encodingProfile,
								 EncodingQuality encodingQuality, uint fr_n, uint fr_d,
								 string outputFile, bool enableAudio, bool enableTitle,
								 uint titleSize, Watermark watermark)
		{
			VideoStandard = videoStandard;
			EncodingProfile = encodingProfile;
			EncodingQuality = encodingQuality;
			Framerate_n = fr_n;
			Framerate_d = fr_d;
			OutputFile = outputFile;
			TitleSize = titleSize;
			EnableAudio = enableAudio;
			EnableTitle = enableTitle;
			Watermark = watermark;
		}

		public VideoStandard VideoStandard;
		public EncodingProfile EncodingProfile;
		public EncodingQuality EncodingQuality;
		public uint Framerate_n;
		public uint Framerate_d;
		public string OutputFile;
		public uint TitleSize;
		public bool EnableAudio;
		public bool EnableTitle;
		public Watermark Watermark;



		public static EncodingSettings DefaultRenderingSettings (string outputFilepath)
		{
			return new EncodingSettings (App.Current.Config.RenderVideoStandard,
				App.Current.Config.RenderEncodingProfile,
				App.Current.Config.RenderEncodingQuality,
				App.Current.Config.FPS_N, App.Current.Config.FPS_D,
				outputFilepath,
				App.Current.Config.EnableAudio, App.Current.Config.OverlayTitle, 20,
				null);
		}

		/// <summary>
		/// Returns the video bitrate using the Kush Gauge equation in kbps.
		/// </summary>
		public uint VideoBitrate {
			get {
				float fps = (float)Framerate_n / Framerate_d;
				float motionFactor = EncodingQuality.VideoQuality / 1000;
				return (uint)(VideoStandard.Width * VideoStandard.Height * fps * 0.07 * motionFactor / 1000);
			}
		}

		/// <summary>
		/// Returns the audio bitrate.
		/// </summary>
		public uint AudioBitrate {
			get {
				return EncodingQuality.AudioQuality;
			}
		}
	}
}

