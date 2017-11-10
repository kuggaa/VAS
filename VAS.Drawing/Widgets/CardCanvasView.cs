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
using System;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.MVVMC;

namespace VAS.Drawing.Widgets
{
	/// <summary>
	/// Common Card Canvas Drawing/Render implementation, all CardCanvasViews should inherit from CardCanvasView and set different
	/// fields to automatically draw common things
	/// </summary>
	public abstract class CardCanvasView<TViewModel> : Canvas, ICanvasView<TViewModel>
		where TViewModel : IViewModel
    {
		protected static ISurface defaultBackground;
		protected static ISurface calendarIcon;

		protected const float ALPHA_BACKGROUND = 0.8f;
		protected const float ALPHA_EXTRA_INFO = 0.5f;
		protected const int EXTRA_INFO_ICONS_SIZE = 16;
		protected const int CARD_ROUND_RADIUS = 4;

		//FontSizes
		protected const int TITLE_FONT_SIZE = 14;
		protected const int SUBTITLE_FONT_SIZE = 12;
		protected const int EXTRA_INFO_FONT_SIZE = 10;

		protected Area cardDetailArea = new Area (0, 0, 320, 192);
		protected Area titleArea = new Area (4, 115, 312, 19);
		protected Area subtitleArea = new Area (4, 138, 312, 14);
		protected Area extraInfoArea = new Area (0, 160, 320, 32);
		protected Area calendarArea = new Area (4, 168, EXTRA_INFO_ICONS_SIZE, EXTRA_INFO_ICONS_SIZE);
		protected Area dateArea = new Area (24, 171, 70, 11);

		protected Color textColor;
		protected Color extraInfoColor;

		protected abstract string Title { get; }
		protected abstract string SubTitle { get; }
		protected abstract DateTime CreationDate { get; }

		TViewModel viewModel;

		static CardCanvasView() {
			calendarIcon = App.Current.DrawingToolkit.CreateSurfaceFromIcon (StyleConf.CalendarIcon);
			defaultBackground = App.Current.DrawingToolkit.CreateSurfaceFromResource (StyleConf.DefaultCardBackground);
		}

		public CardCanvasView ()
		{
			textColor = App.Current.Style.TextContrastBase;
			BackgroundColor = App.Current.Style.ThemeContrastBase;
			BackgroundColor.SetAlpha (ALPHA_BACKGROUND);
			extraInfoColor = App.Current.Style.ThemeContrastBase;
			extraInfoColor.SetAlpha (ALPHA_EXTRA_INFO);
		}

		public override void Draw (IContext context, Area area)
		{
			tk.Context = context;
			tk.Begin();
			tk.LineWidth = 0;
			tk.ClipRoundRectangle (cardDetailArea.Start, cardDetailArea.Width, cardDetailArea.Height, CARD_ROUND_RADIUS);
			tk.Clear (textColor);
			DrawBackgroundImage();
			DrawCardBackground();
			DrawContent();
			DrawTitle();
			DrawSubtitle();
			DrawExtraInformation();
			tk.End();
		}

		public TViewModel ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				widget?.ReDraw ();
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (TViewModel)viewModel;
		}

		protected abstract void DrawContent();

		protected virtual void DrawBackgroundImage ()
		{
			tk.DrawSurface (cardDetailArea.Start, cardDetailArea.Width, cardDetailArea.Height,
			               defaultBackground, ScaleMode.Fill);
		}

		protected virtual void DrawExtraInformation ()
		{
			tk.FillColor = extraInfoColor;
			tk.StrokeColor = extraInfoColor;
			tk.DrawRectangle (extraInfoArea.Start,extraInfoArea.Width, extraInfoArea.Height);
			DrawCreationDate ();
		}

		void DrawCardBackground ()
		{
			tk.FillColor = BackgroundColor;
			tk.StrokeColor = BackgroundColor;
			tk.DrawRectangle(cardDetailArea.Start, cardDetailArea.Width, cardDetailArea.Height);
		}

		void DrawTitle ()
		{
			tk.FillColor = textColor;
			tk.StrokeColor = textColor;
			tk.FontAlignment = FontAlignment.Left;
			tk.FontWeight = FontWeight.Bold;
			tk.FontSize = TITLE_FONT_SIZE;
			tk.DrawText(titleArea.Start, titleArea.Width, titleArea.Height, Title);
		}

		void DrawSubtitle ()
		{
			tk.FillColor = textColor;
			tk.StrokeColor = textColor;
			tk.FontSize = SUBTITLE_FONT_SIZE;
			tk.FontWeight = FontWeight.Light;
			tk.DrawText(subtitleArea.Start, subtitleArea.Width, subtitleArea.Height, SubTitle);
		}

		void DrawCreationDate ()
		{
			tk.FillColor = textColor;
			tk.StrokeColor = textColor;
			tk.DrawSurface (calendarArea.Start, calendarArea.Width, calendarArea.Height,
							calendarIcon, ScaleMode.AspectFit, true);
			tk.FontSize = EXTRA_INFO_FONT_SIZE;
			tk.FontWeight = FontWeight.Light;
			tk.DrawText (dateArea.Start, dateArea.Width, dateArea.Height, CreationDate.ToString ("dd/MM/yyyy"));
		}
	}
}
