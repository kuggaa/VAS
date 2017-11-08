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
using System.Runtime.Serialization;
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.MVVMC;
using VAS.Core.Serialization;

namespace VAS
{
	[Serializable]
	public abstract class Config : BindableBase
	{
		public void Save ()
		{
			try {
				if (AutoSaveConfig)
					Serializer.Instance.Save (this, App.Current.ConfigFile);
			} catch (Exception ex) {
				Log.Error ("Error saving config");
				Log.Exception (ex);
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public override bool IsChanged {
			get {
				return base.IsChanged;
			}
			set {
				base.IsChanged = value;
				Save ();
			}
		}

		public string Lang {
			get;
			set;
		} = null;

		public VideoStandard CaptureVideoStandard {
			get;
			set;
		} = VideoStandards.P480_16_9.Clone ();

		public EncodingProfile CaptureEncodingProfile {
			get;
			set;
		} = EncodingProfiles.MP4.Clone ();

		public EncodingQuality CaptureEncodingQuality {
			get;
			set;
		} = EncodingQualities.Medium.Clone ();

		public bool AutoSave {
			get;
			set;
		} = false;

		public bool AutoRenderPlaysInLive {
			get;
			set;
		} = false;

		public string AutoRenderDir {
			get;
			set;
		} = null;

		public string LastDir {
			get;
			set;
		} = null;

		public string LastRenderDir {
			get;
			set;
		} = null;

		public bool ReviewPlaysInSameWindow {
			get;
			set;
		} = true;

		public string DefaultTemplate {
			get;
			set;
		} = null;

		public ProjectSortMethod ProjectSortMethod {
			get;
			set;
		} = ProjectSortMethod.Date;

		public Version IgnoreUpdaterVersion {
			get;
			set;
		} = null;

		public VideoStandard RenderVideoStandard {
			get;
			set;
		} = VideoStandards.P720_16_9.Clone ();

		public EncodingProfile RenderEncodingProfile {
			get;
			set;
		} = EncodingProfiles.MP4.Clone ();

		public EncodingQuality RenderEncodingQuality {
			get;
			set;
		} = EncodingQualities.High.Clone ();

		public bool OverlayTitle {
			get;
			set;
		} = true;

		public bool EnableAudio {
			get;
			set;
		} = false;

		public bool AddWatermark {
			get;
			set;
		} = true;

		/// <summary>
		/// Gets or sets the watermark.
		/// </summary>
		/// <value>The watermark.</value>
		public Image Watermark {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the name of the watermark. It's typically the filename.
		/// </summary>
		/// <value>The name of the watermark.</value>
		public string WatermarkName {
			get;
			set;
		} = "Watermark";

		public uint FPS_N {
			get;
			set;
		} = 25;

		public uint FPS_D {
			get;
			set;
		} = 1;

		public bool FastTagging {
			get;
			set;
		} = false;

		public string CurrentDatabase {
			get;
			set;
		}

		public string LicenseCode {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the key configs list
		/// </summary>
		/// <value>The key configurations</value>
		public List<KeyConfig> KeyConfigs {
			get;
			set;
		} = new List<KeyConfig> ();

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		bool AutoSaveConfig {
			get; set;
		} = true;

		/// <summary>
		/// Handles the on deserializing. When deserialization starts, auto save is deactivated.
		/// </summary>
		/// <param name="context">Context.</param>
		[OnDeserializing]
		internal void HandleOnDeserializing (StreamingContext context)
		{
			AutoSaveConfig = false;
		}

		/// <summary>
		/// Handles the on deserialized. When deserializations ends, auto save is activated.
		/// </summary>
		/// <param name="context">Context.</param>
		[OnDeserialized ()]
		internal void HandleOnDeserialized (StreamingContext context)
		{
			AutoSaveConfig = true;
		}
	}
}
