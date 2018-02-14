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
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.ViewModel;

namespace VAS.Tests.Core.ViewModel
{
	[TestFixture]
	public class TestEventButtonVM
	{

		[Test]
		public void SetCurrenTime_TimerStartedAndNewCurrentTimeOlderThanStart_TimerCancelled ()
		{
			EventButtonVM button = new EventButtonVM {
				Model = new AnalysisEventButton (),
				Mode = DashboardMode.Code,
				TagMode = TagMode.Free
			};
			button.CurrentTime = new Time { TotalSeconds = 4999 };
			button.Click ();
			button.CurrentTime = new Time { TotalSeconds = 5000 };

			button.CurrentTime = new Time { TotalSeconds = 3000 };

			Assert.IsNull (button.ButtonTime);
			Assert.IsNull (button.RecordingStart);
		}
	}
}
