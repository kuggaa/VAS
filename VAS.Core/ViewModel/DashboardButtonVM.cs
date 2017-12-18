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
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;

namespace VAS.Core.ViewModel
{
	/// <summary>
	/// ViewModel for <see cref="DashboardButton"/>.
	/// </summary>
	public class DashboardButtonVM : ViewModelBase<DashboardButton>
	{
		public DashboardButtonVM ()
		{
			ActionLinks = new CollectionViewModel<ActionLink, ActionLinkVM> ();
			HotKey = new HotKeyVM ();
		}

		/// <summary>
		/// Gets or sets the model (DashboardButton).
		/// </summary>
		/// <value>The model.</value>
		[PropertyChanged.DoNotCheckEquality]
		public override DashboardButton Model {
			get {
				return base.Model;
			}
			set {
				ActionLinks.Model = value?.ActionLinks;
				HotKey.Model = value?.HotKey;
				base.Model = value;
			}
		}

		/// <summary>
		/// Gets the DashboardButtonView.
		/// </summary>
		/// <value>The view.</value>
		public virtual string View {
			get;
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public virtual string Name {
			get {
				return Model.Name;
			}
			set {
				Model.Name = value;
			}
		}

		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		/// <value>The position.</value>
		public Point Position {
			get {
				return Model.Position;
			}
			set {
				Model.Position = value;
			}
		}

		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>The width.</value>
		public int Width {
			get {
				return Model.Width;
			}
			set {
				Model.Width = value;
			}
		}

		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		/// <value>The height.</value>
		public int Height {
			get {
				return Model.Height;
			}
			set {
				Model.Height = value;
			}
		}

		/// <summary>
		/// Gets or sets the color of the background.
		/// </summary>
		/// <value>The color of the background.</value>
		public Color BackgroundColor {
			get {
				return Model.BackgroundColor;
			}
			set {
				Model.BackgroundColor = value;
			}
		}

		/// <summary>
		/// Gets or sets the color of the text.
		/// </summary>
		/// <value>The color of the text.</value>
		public Color TextColor {
			get {
				return Model.TextColor;
			}
			set {
				Model.TextColor = value;
			}
		}

		/// <summary>
		/// Gets or sets the hot key.
		/// </summary>
		/// <value>The hot key.</value>
		public virtual HotKeyVM HotKey {
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the background image.
		/// </summary>
		/// <value>The background image.</value>
		public virtual Image BackgroundImage {
			get {
				return Model.BackgroundImage;
			}
			set {
				Model.BackgroundImage = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating to showing or not the hotkey.
		/// </summary>
		/// <value><c>true</c> if show hotkey; otherwise, <c>false</c>.</value>
		public bool ShowHotkey {
			get {
				return Model.ShowHotkey;
			}
			set {
				Model.ShowHotkey = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating to showing or not the setting icon.
		/// </summary>
		/// <value><c>true</c> if show setting icon; otherwise, <c>false</c>.</value>
		public bool ShowSettingIcon {
			get {
				return Model.ShowSettingIcon;
			}
			set {
				Model.ShowSettingIcon = value;
			}
		}

		/// <summary>
		/// A list with all the outgoing links of this button
		/// </summary>
		public CollectionViewModel<ActionLink, ActionLinkVM> ActionLinks {
			get;
			set;
		}

		/// <summary>
		/// Gets the LightColor.
		/// </summary>
		/// <value>The color of the light.</value>
		public Color LightColor {
			get {
				return Model.LightColor;
			}
		}

		/// <summary>
		/// Gets the DarkColor.
		/// </summary>
		/// <value>The color of the dark.</value>
		public Color DarkColor {
			get {
				return Model.DarkColor;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating to showing or not the icon.
		/// </summary>
		/// <value><c>true</c> if show icon; otherwise, <c>false</c>.</value>
		public bool ShowIcon {
			get {
				return Model.ShowIcon;
			}
			set {
				Model.ShowIcon = value;
			}
		}
		/// <summary>
		/// Gets or sets the dashboard mode.
		/// </summary>
		/// <value>The mode.</value>
		[PropertyChanged.DoNotNotify]
		public DashboardMode Mode {
			get;
			set;
		}
	}
}
