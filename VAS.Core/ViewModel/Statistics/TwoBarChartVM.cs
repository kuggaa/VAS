//
//  Copyright (C) 2017 FLUENDO S.A.
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
using System.Collections.Generic;
using System.ComponentModel;
using VAS.Core.MVVMC;

namespace VAS.Core.ViewModel.Statistics
{
	/// <summary>
	/// View model of a graphic with two horizontal series
	/// </summary>
	public sealed class TwoBarChartVM : BarChartVM
	{
		TwoBarTextStyle leftTextStyle;
		TwoBarTextStyle rightTextStyle;
		string leftDisplayText;
		string rightDisplayText;
		double referenceNumber;
		double total;
		bool isRightEmptySerie;

		public TwoBarChartVM (SeriesVM leftSerie, SeriesVM rightSerie = null, int total = 0)
		{
			LeftSerie = leftSerie;
			isRightEmptySerie = rightSerie == null;
			RightSerie = rightSerie ?? new SeriesVM ("empty", App.Current.Style.ThemeBase);
			Series.ViewModels.AddRange (new List<SeriesVM> { LeftSerie, RightSerie });
			leftTextStyle = rightTextStyle = TwoBarTextStyle.Hidden;
			TotalElements = ReferenceNumber = total;
			UpdateDisplayTexts (TwoBarSide.Left);
			UpdateDisplayTexts (TwoBarSide.Right);
		}

		/// <summary>
		/// Left serie in the chart
		/// </summary>
		/// <value>The left serie.</value>
		public SeriesVM LeftSerie { get; private set; }

		/// <summary>
		/// Right serie in the chart which can be optional
		/// </summary>
		/// <value>The right serie.</value>
		public SeriesVM RightSerie { get; private set; }

		/// <summary>
		/// Gets or sets the left text style.
		/// </summary>
		/// <value>The left text style.</value>
		public TwoBarTextStyle LeftTextStyle {
			get { return leftTextStyle; }
			set {
				leftTextStyle = value;
				UpdateDisplayTexts (TwoBarSide.Left);
			}
		}

		/// <summary>
		/// Gets or sets the right text style.
		/// </summary>
		/// <value>The right text style.</value>
		public TwoBarTextStyle RightTextStyle {
			get { return rightTextStyle; }
			set {
				rightTextStyle = value;
				UpdateDisplayTexts (TwoBarSide.Right);
			}
		}

		/// <summary>
		/// Total number of elements in series
		/// </summary>
		/// <value>The reference number.</value>
		public double TotalElements {
			get => total;
			set {
				total = value;
				SyncEmptySerie ();
			}
		}

		/// <summary>
		/// Number to refer the elements in a serie
		/// </summary>
		/// <value>The reference number.</value>
		public double ReferenceNumber {
			get { return referenceNumber; }
			set {
				referenceNumber = value;
				UpdateDisplayTexts (TwoBarSide.Left);
				UpdateDisplayTexts (TwoBarSide.Right);
			}
		}

		/// <summary>
		/// Left diapl
		/// </summary>
		/// <value>The dispaly text.</value>
		public string LeftDisplayText {
			get {
				return leftDisplayText;
			}
		}

		/// <summary>
		/// Left diapl
		/// </summary>
		/// <value>The dispaly text.</value>
		public string RightDisplayText {
			get {
				return rightDisplayText;
			}
		}

		void UpdateDisplayTexts (TwoBarSide side)
		{
			switch (side) {
			case TwoBarSide.Left:
				InternalUpdate (ref leftDisplayText, LeftSerie, ref leftTextStyle);
				break;
			case TwoBarSide.Right:
				InternalUpdate (ref rightDisplayText, RightSerie, ref rightTextStyle);
				break;
			}
		}

		void InternalUpdate (ref string text, SeriesVM serie, ref TwoBarTextStyle style)
		{
			double refValue = 0;
			if (ReferenceNumber > 0.1f) {
				refValue = LeftSerie.Elements / ReferenceNumber;
			}

			switch (style) {
			case TwoBarTextStyle.NormalValue:
				text = serie.Elements.ToString ();
				break;
			case TwoBarTextStyle.PercentageRef:
				text = string.Format ("{0} ({1:0.0%})", serie.Elements, refValue);
				break;
			case TwoBarTextStyle.TotalRef:
				text = string.Format (" {0} / {1}", serie.Elements, ReferenceNumber);
				break;
			default:
				text = string.Empty;
				break;
			}
		}

		protected override void ForwardPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (NeedsSync (e.PropertyName, nameof (LeftSerie.Elements), sender, LeftSerie)) {
				SyncEmptySerie ();
			}

			base.ForwardPropertyChanged (sender, e);
		}

		void SyncEmptySerie () {
			if (isRightEmptySerie) {
				RightSerie.Elements = (int)TotalElements - LeftSerie.Elements;
			}
		}
	}

	/// <summary>
	/// Style of the text for the two bars chart
	/// </summary>
	public enum TwoBarTextStyle
	{
		/// <summary>
		/// An additional value is displayed with the reference to a given number --> x (y%)
		/// </summary>
		PercentageRef,

		/// <summary>
		/// An additional value is displayed with the reference to a given number --> x/y
		/// </summary>
		TotalRef,

		/// <summary>
		/// Only the value is displayed
		/// </summary>
		NormalValue,

		/// <summary>
		/// Display no value
		/// </summary>
		Hidden
	}

	/// <summary>
	/// Defines the sides of the chart
	/// </summary>
	enum TwoBarSide
	{
		/// <summary>
		/// Left side
		/// </summary>
		Left,

		/// <summary>
		/// Right side
		/// </summary>
		Right
	}
}
