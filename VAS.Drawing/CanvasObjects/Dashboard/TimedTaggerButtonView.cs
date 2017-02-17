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

using System.ComponentModel;
using VAS.Core.Common;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Dashboard
{

	/// <summary>
	/// Class for the TimedTaggerButton View
	/// </summary>
	public class TimedTaggerButtonView : DashboardButtonView
	{

		public TimedDashboardButtonVM TimedButtonVM {
			get {
				return ButtonVM as TimedDashboardButtonVM;
			}
			set {
				ButtonVM = value;
			}
		}

		protected bool Recording {
			get;
			set;
		}

		public override void ClickReleased ()
		{
			if (TimedButtonVM.TagMode == TagMode.Predefined) {
				Active = !Active;
				EmitClickEvent ();
			} else if (!Recording) {
				StartRecording ();
			} else {
				EmitClickEvent ();
				Clear ();
			}
		}

		protected void StartRecording ()
		{
			Recording = true;
			if (TimedButtonVM.RecordingStart == null) {
				TimedButtonVM.RecordingStart = TimedButtonVM.CurrentTime;
			}
			Active = true;
			ReDraw ();
		}

		protected virtual void Clear ()
		{
			Recording = false;
			TimedButtonVM.RecordingStart = null;
			Active = false;
		}

		protected override void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (TimedButtonVM.NeedsSync (e, nameof (TimedButtonVM.ButtonTime))) {
				ReDraw ();
			}
		}
	}
}
