//
//  Copyright (C) 2018 Fluendo S.A.
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
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Interfaces.Services;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Services.Service
{
	// FIXME: Make it a real reference
	[Controller ("LiveProjectAnalysis")]
	public class VideoRecoderService : DisposableBase, IVideoRecorderService
	{
		public Image CurrentFrame => throw new NotImplementedException ();

		public bool Started => throw new NotImplementedException ();

		public Task Start ()
		{
			return AsyncHelpers.Return ();
		}
		public Task Stop ()
		{
			return AsyncHelpers.Return ();
		}

		public void SetViewModel (IViewModel viewModel)
		{
		}

		public void SetDefaultCallbacks (VideoRecorderVM recorder)
		{
			recorder.StartRecordingCommand.SetCallback (o => StartRecording ((bool)o));
			recorder.StopRecordingCommand.SetCallback (StopRecording);
			recorder.PauseClockCommand.SetCallback (PauseClock);
			recorder.ResumeClockCommand.SetCallback (ResumeClock);

		}

		public void Close ()
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<KeyAction> GetDefaultKeyActions ()
		{
			throw new NotImplementedException ();
		}

		public void PauseClock ()
		{
			throw new NotImplementedException ();
		}

		public void ResumeClock ()
		{
			throw new NotImplementedException ();
		}

		public void Run ()
		{
			throw new NotImplementedException ();
		}

		public void StartRecording (bool newPeriod = true)
		{
			throw new NotImplementedException ();
		}

		public void StopRecording ()
		{
			throw new NotImplementedException ();
		}
	}
}
