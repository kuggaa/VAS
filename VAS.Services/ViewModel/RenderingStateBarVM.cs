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
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;

namespace VAS.Services.ViewModel
{
	public class RenderingStateBarVM : BindableBase, IViewModel, IRenderingStateBar
	{
		/// <summary>
		/// Cancel event handler
		/// </summary>
		public event EventHandler Cancel;
		/// <summary>
		/// Manage Jobs event handler
		/// </summary>
		public event EventHandler ManageJobs;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:VAS.Services.ViewModel.RenderingStateBarVM"/> job running.
		/// </summary>
		/// <value><c>true</c> if job running; otherwise, <c>false</c>.</value>
		public bool JobRunning { get; set; }

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>The text.</value>
		public string Text { get; set; }

		/// <summary>
		/// Gets or sets the progress text.
		/// </summary>
		/// <value>The progress text.</value>
		public string ProgressText { get; set; }

		/// <summary>
		/// Gets or sets the fractionin the progress bar.
		/// </summary>
		/// <value>The fraction.</value>
		public double Fraction { get; set; }

		#region commands
		/// <summary>
		/// Commands that cancel jobs.
		/// </summary>
		public void CommandCancelJob ()
		{
			if (Cancel != null) {
				Cancel (this, null);
			}
		}

		/// <summary>
		/// Commands that manage jobs.
		/// </summary>
		public void CommandManageJob ()
		{
			if (ManageJobs != null) {
				ManageJobs (this, null);
			}
		}
		#endregion
	}
}
