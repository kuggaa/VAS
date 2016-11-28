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
using VAS.Core.Interfaces.Drawing;
using VAS.Core.ViewModel;
using VAS.Core.MVVMC;

namespace VAS.Drawing.CanvasObjects.Timeline
{

	/// <summary>
	/// A label for the event types timeline row.
	/// </summary>
	[View ("EventTypeLabelView")]
	public class EventTypeLabelView : LabelView, ICanvasObjectView<EventTypeVM>
	{
		EventTypeVM viewModel;

		public override Color Color {
			get {
				return ViewModel.Color;
			}
		}

		public override string Name {
			get {
				return ViewModel.Name;
			}
		}

		public EventTypeVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (EventTypeVM)viewModel;
		}
	}
}
