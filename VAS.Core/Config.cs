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
using System.IO;
using Newtonsoft.Json;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Serialization;

namespace VAS
{
	public class Config
	{
		public static string homeDirectory = ".";
		public static string baseDirectory = ".";
		public static string configDirectory = ".";
		public static string dataDir = ".";
		
		/* State */
		public static EventsBroker EventsBrokerBase;

		protected static StyleConf style;
		protected static ConfigState state;

		public static void Init ()
		{
			/* NOTE
			*  All derived Configs should set the following:
			*  
			*  Config.baseDirectory
			*  Config.configDirectory
			*  Config.dataDir
			*  Config.homeDirectory
			*/
		}

		public static void LoadState (ConfigState newState)
		{
			state = newState;
		}

		public static void Save ()
		{
			try {
				Serializer.Instance.Save (state, Config.ConfigFile);
			} catch (Exception ex) {
				Log.Error ("Error saving config");
				Log.Exception (ex);
			}
		}

		public static StyleConf Style {
			get {
				if (style == null) {
					style = new StyleConf ();
				}
				return style;
			}
			set {
				style = value;
			}
		}

		public static string ConfigFile {
			get {
				string filename = Constants.SOFTWARE_NAME.ToLower () + "-1.0.config";
				return Path.Combine (Config.ConfigDir, filename);
			}
		}

		public static string HomeDir {
			get {
				return homeDirectory;
			}
		}

		public static string BaseDir {
			set {
				baseDirectory = value;
			}
		}

		public static string ConfigDir {
			set {
				configDirectory = value;
			}
			get {
				return configDirectory;
			}
		}

		public static string TemplatesDir {
			get {
				return Path.Combine (DBDir, "templates");
			}
		}

		public static string PlayListDir {
			get {
				return Path.Combine (homeDirectory, "playlists");
			}
		}

		public static string SnapshotsDir {
			get {
				return Path.Combine (homeDirectory, "snapshots");
			}
		}

		public static string VideosDir {
			get {
				return Path.Combine (homeDirectory, "videos");
			}
		}

		public static string TempVideosDir {
			get {
				return Path.Combine (configDirectory, "temp");
			}
		}

		public static string DBDir {
			get {
				return Path.Combine (homeDirectory, "db");
			}
		}

		public static string RelativeToPrefix (string relativePath)
		{
			return Path.Combine (baseDirectory, relativePath);
		}

		#region Properties

		static public Version Version {
			get;
			set;
		}

		static public string BuildVersion {
			get;
			set;
		}

		static public Image FieldBackground {
			get {
				return Resources.LoadImage (Constants.FIELD_BACKGROUND);
			}
		}

		static public Image HalfFieldBackground {
			get {
				return Resources.LoadImage (Constants.HALF_FIELD_BACKGROUND);
			}
		}

		static public Image HHalfFieldBackground {
			get {
				return Resources.LoadImage (Constants.HHALF_FIELD_BACKGROUND);
			}
		}

		static public Image GoalBackground {
			get {
				return Resources.LoadImage (Constants.GOAL_BACKGROUND);
			}
		}

		public static string CurrentDatabase {
			get {
				return state.currentDatabase;
			}
			set {
				state.currentDatabase = value;
				Save ();
			}
		}

		public static VideoStandard RenderVideoStandard {
			get {
				return state.renderVideoStandard;
			}
			set {
				state.renderVideoStandard = value;
				Save ();
			}
		}

		public static EncodingProfile RenderEncodingProfile {
			get {
				return state.renderEncodingProfile;
			}
			set {
				state.renderEncodingProfile = value;
				Save ();
		
			}
		}

		public static EncodingQuality RenderEncodingQuality {
			get {
				return state.renderEncodingQuality;
			}
			set {
				state.renderEncodingQuality = value;
				Save ();
			}
		}

		public static bool OverlayTitle {
			get {
				return state.overlayTitle;
			}
			set {
				state.overlayTitle = value;
				Save ();
			}
		}

		public static bool EnableAudio {
			get {
				return state.enableAudio;
			}
			set {
				state.enableAudio = value;
				Save ();
			}
		}

		public static uint FPS_N {
			get {
				return state.fps_n;
			}
			set {
				state.fps_n = value;
				Save ();
			}
		}

		public static uint FPS_D {
			get {
				return state.fps_d;
			}
			set {
				state.fps_d = value;
				Save ();
			}
		}

		#endregion

	}

	[Serializable]
	[JsonConverter (typeof(VASConverter))]
	public class ConfigState
	{
		public bool fastTagging;
		public bool autoSave;
		public string currentDatabase;
		public string lang;
		public uint fps_n;
		public uint fps_d;
		public VideoStandard captureVideoStandard;
		public VideoStandard renderVideoStandard;
		public EncodingProfile captureEncodingProfile;
		public EncodingProfile renderEncodingProfile;
		public EncodingQuality captureEncodingQuality;
		public EncodingQuality renderEncodingQuality;
		public bool overlayTitle;
		public bool enableAudio;
		public bool autorender;
		public string autorenderDir;
		public string lastRenderDir;
		public string lastDir;
		public bool reviewPlaysInSameWindow;
		public string defaultTemplate;
		public ProjectSortMethod projectSortMethod;
		public Version ignoreUpdaterVersion;

		public ConfigState ()
		{
			/* Set default values */
			fastTagging = false;
			currentDatabase = Constants.DEFAULT_DB_NAME;
			lang = null;
			autoSave = false;
			captureVideoStandard = VideoStandards.P480_16_9.Clone ();
			captureEncodingProfile = EncodingProfiles.MP4.Clone ();
			captureEncodingQuality = EncodingQualities.Medium.Clone ();
			renderVideoStandard = VideoStandards.P720_16_9.Clone ();
			renderEncodingProfile = EncodingProfiles.MP4.Clone ();
			renderEncodingQuality = EncodingQualities.High.Clone ();
			overlayTitle = true;
			enableAudio = false;
			fps_n = 25;
			fps_d = 1;
			autorender = false;
			autorenderDir = null;
			lastRenderDir = null;
			lastDir = null;
			reviewPlaysInSameWindow = true;
			defaultTemplate = null;
			projectSortMethod = ProjectSortMethod.Date;
			ignoreUpdaterVersion = null;
		}
	}
}