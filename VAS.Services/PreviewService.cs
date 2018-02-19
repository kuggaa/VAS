//
//  Copyright (C) 2018 
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
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store.Templates;
using VAS.Drawing.Widgets;

namespace VAS.Services
{
	/// <summary>
	/// Preview service that takes a snapshoot of a layout given a specified model
	/// </summary>
	public abstract class PreviewService : IPreviewService
	{
		/// <summary>
		/// Creates the preview.
		/// </summary>
		/// <returns>The preview.</returns>
		/// <param name="template">Template.</param>
		public Image CreatePreview (ITemplate template)
		{
			Team team = template as Team;
			if (team != null) {
				return CreatePreview (team);
			}

			Dashboard dashboard = template as Dashboard;
			if (dashboard != null) {
				return CreatePreview (dashboard);
			}

			return null;
		}

		protected Image CreatePreview (Dashboard dashboard) {
			// instantiate the view and the viewmodel and call the create internal preview
			return null;
		}

		protected virtual Image CreatePreview (Team team) {
			return null;
		}

		protected void CreateInternalPreview (ICanvasView view, IViewModel vm, double width, double height) {
			NoWindowWidget widget = new NoWindowWidget ();
			view.SetWidget (widget);
			view.SetViewModel (vm);

			// set the widget sizes after the view model has been set
			widget.Width = width;
			widget.Height = height;
		}
	}
}
