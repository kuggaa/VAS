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
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.MVVMC;

namespace VAS.Core.Common
{
	[Serializable]
	/// <summary>
	/// A job running in the background.
	/// </summary>
	public class Job : BindableBase
	{
		public Job (EncodingSettings encSettings)
		{
			EncodingSettings = encSettings;
			State = JobState.None;
			Progress = -1;
		}

		/// <summary>
		/// Gets the name of the job.
		/// </summary>
		/// <value>The name.</value>
		public virtual string Name {
			get {
				return System.IO.Path.GetFileName (EncodingSettings.OutputFile);
			}
		}

		/// <summary>
		/// Gets or sets the state of the job.
		/// </summary>
		/// <value>The state.</value>
		public JobState State {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the progress of the job with values from 0 to 1.
		/// </summary>
		/// <value>The progress.</value>
		public double Progress {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the settings used to perform the job.
		/// </summary>
		/// <value>The encoding settings.</value>
		// FIXME: Move to a base RenderingJob when we support other kind of background jobs.
		public EncodingSettings EncodingSettings {
			get;
			set;
		}
	}

	/// <summary>
	/// A job for rendering a <see cref="Playlist"/>.
	/// </summary>
	public class EditionJob : Job
	{
		public EditionJob (Playlist playlist, EncodingSettings encSettings) : base (encSettings)
		{
			Playlist = Cloner.Clone (playlist);
		}

		/// <summary>
		/// The playlist the render.
		/// </summary>
		/// <value>The playlist.</value>
		public Playlist Playlist {
			get;
			protected set;
		}

		public override string Name {
			get {
				return Playlist?.Name ?? base.Name;
			}
		}
	}

	/// <summary>
	/// A job for convert and join one or serveral files into a new file.
	/// </summary>
	public class ConversionJob : Job
	{
		public ConversionJob (List<MediaFile> files, EncodingSettings encSettings) : base (encSettings)
		{
			InputFiles = files;
		}

		/// <summary>
		/// Gets or sets the list of files to render.
		/// </summary>
		/// <value>The input files.</value>
		public List<MediaFile> InputFiles {
			get;
			protected set;
		}
	}
}

