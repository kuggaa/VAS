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

using VAS.Core.Interfaces.MVVMC;

namespace VAS.Core.MVVMC
{

	/// <summary>
	/// A base class for UI bindings.
	/// </summary>
	public abstract class Binding : DisposableBase
	{
		IViewModel viewModel;
		bool binded;

		public IViewModel ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					UnbindViewModel ();
				}
				viewModel = value;
				if (viewModel != null) {
					BindViewModel ();
				}
				if (!binded) {
					BindView ();
					binded = true;
				}
			}
		}

		protected override void DisposeManagedResources ()
		{
			if (binded) {
				UnbindView ();
				UnbindViewModel ();
			}
		}

		/// <summary>
		/// Subclasses must implement this function to the bind the ViewModel.
		/// </summary>
		abstract protected void BindViewModel ();

		/// <summary>
		/// Subclasses must implement this function to the undbind the ViewModel.
		/// </summary>
		abstract protected void UnbindViewModel ();

		/// <summary>
		/// Subclasses must implement this function to the bind the view.
		/// </summary>
		abstract protected void BindView ();

		/// <summary>
		/// Subclasses must implement this function to the unbind the view.
		/// </summary>
		abstract protected void UnbindView ();
	}

}
