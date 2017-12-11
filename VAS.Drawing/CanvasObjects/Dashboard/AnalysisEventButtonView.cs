//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.ComponentModel;
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;

namespace VAS.Drawing.CanvasObjects.Dashboard
{
	/// <summary>
	/// Class for the AnalysisEventButton View
	/// </summary>
	[View ("AnalysisEventButtonView")]
	public class AnalysisEventButtonView : TimedTaggerButtonView, ICanvasObjectView<AnalysisEventButtonVM>
	{
		public event ButtonSelectedHandler EditButtonTagsEvent;

		static Image iconImage;
		protected static Image recImage;
		static Image editImage;
		protected static Image cancelImage;
		static Image applyImage;
		protected Dictionary<Rectangle, object> rects, buttonsRects;
		Dictionary<string, List<TagVM>> tagsByGroup;
		bool emitEvent, delayEvent, editClicked;
		bool cancelClicked, applyClicked, moved;
		int nrows;
		const int TIMEOUT_MS = 800;
		System.Threading.Timer timer;
		protected object cancelButton = new object ();
		object editbutton = new object ();
		object applyButton = new object ();
		protected Rectangle editRect, cancelRect, applyRect;
		double catWidth, heightPerRow;
		Dictionary<LinkAnchorView, TagVM> subcatAnchors;

		public AnalysisEventButtonView () : base ()
		{
			rects = new Dictionary<Rectangle, object> ();
			buttonsRects = new Dictionary<Rectangle, object> ();
			cancelRect = new Rectangle (new Point (0, 0), 0, 0);
			editRect = new Rectangle (new Point (0, 0), 0, 0);
			applyRect = new Rectangle (new Point (0, 0), 0, 0);
			if (iconImage == null) {
				iconImage = App.Current.ResourcesLocator.LoadImage (StyleConf.ButtonEventIcon);
			}
			if (recImage == null) {
				recImage = App.Current.ResourcesLocator.LoadImage (StyleConf.RecordButton);
			}
			if (editImage == null) {
				editImage = App.Current.ResourcesLocator.LoadImage (StyleConf.EditButton);
			}
			if (cancelImage == null) {
				cancelImage = App.Current.ResourcesLocator.LoadImage (StyleConf.CancelButton);
			}
			if (applyImage == null) {
				applyImage = App.Current.ResourcesLocator.LoadImage (StyleConf.ApplyButton);
			}
			MinWidth = 100;
			MinHeight = HeaderHeight * 2;
			subcatAnchors = new Dictionary<LinkAnchorView, TagVM> ();
		}

		protected override void DisposeManagedResources ()
		{
			base.DisposeManagedResources ();
			if (timer != null) {
				timer.Dispose ();
				timer = null;
			}
			foreach (LinkAnchorView anchor in subcatAnchors.Keys.ToList ()) {
				RemoveAnchor (anchor);
			}
		}

		public AnalysisEventButtonVM ViewModel {
			get {
				return ButtonVM as AnalysisEventButtonVM;
			}
			set {
				if (ButtonVM != null) {
					ButtonVM.PropertyChanged -= HandleViewModelPropertyChanged;
				}
				ButtonVM = value;

				if (value != null) {
					foreach (TagVM tag in value.Tags.ViewModels) {
						AddSubcatAnchor (tag, new Point (0, 0), 100, HeaderHeight);
					}
					value.PropertyChanged += HandleViewModelPropertyChanged;
				}

			}
		}

		void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (ViewModel.IsCategoryClicked)) {
				if (ViewModel.IsCategoryClicked) {
					CategoryClicked (null);
				} else {
					EmitCreateEvent ();
				}
			}
		}

		public override Image Icon {
			get {
				if (Button.ShowIcon)
					return iconImage;
				else
					return null;
			}
		}

		public override Color BackgroundColor {
			get {
				return Button.BackgroundColor;
			}
		}

		// FIXME: View accessing the Model/ <value>The button.</value>
		new AnalysisEventButton Button {
			get {
				return ViewModel.TypedModel;
			}
		}

		bool ShowApplyButton {
			get {
				return ShowTags && tagsByGroup.Count > 1
											  && ViewModel.TagMode == TagMode.Predefined
											  && ButtonVM.Mode != DashboardMode.Edit;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:VAS.Drawing.CanvasObjects.Dashboard.AnalysisEventButtonView"/>
		/// show tags.
		/// </summary>
		/// <value><c>true</c> if show tags; otherwise, <c>false</c>.</value>
		protected bool ShowTags {
			get {
				return (ViewModel.ShowSubcategories || ShowLinks) && Button.AnalysisEventType.Tags.Count != 0;
			}
		}

		int HeaderHeight {
			get {
				return StyleConf.ButtonHeaderHeight + 5;
			}
		}

		int HeaderTextOffset {
			get {
				return StyleConf.ButtonHeaderWidth + 5;
			}
		}

		double HeaderTextWidth {
			get {
				if (ViewModel.TagMode == TagMode.Free) {
					return Width - HeaderTextOffset - StyleConf.ButtonRecWidth;
				} else {
					return Width - HeaderTextOffset;
				}
			}
		}

		void UpdateRows ()
		{
			/* Header */
			int tagsPerRow = Math.Max (1, ViewModel.TagsPerRow);
			nrows = 0;

			foreach (List<TagVM> tags in tagsByGroup.Values) {
				nrows += (int)Math.Ceiling ((float)tags.Count / tagsPerRow);
			}
		}

		/// <summary>
		/// Sets the view model.
		/// </summary>
		/// <param name="viewModel">View model.</param>
		public void SetViewModel (object viewModel)
		{
			ViewModel = (AnalysisEventButtonVM)viewModel;
		}

		public void ClickTag (TagVM tag)
		{
			SelectedTags.Add (tag);
			ReDraw ();
		}

		override protected void Clear ()
		{
			base.Clear ();
			emitEvent = false;
			cancelClicked = false;
			SelectedTags.Clear ();
		}

		void RemoveAnchor (LinkAnchorView anchor)
		{
			anchor.Dispose ();
			subcatAnchors.Remove (anchor);
		}

		void EmitCreateEvent ()
		{
			ViewModel.Click ();
			Clear ();
		}

		void TimerCallback (Object state)
		{
			App.Current.DrawingToolkit.Invoke (delegate {
				EmitCreateEvent ();
			});
		}

		void DelayTagClicked ()
		{
			if (tagsByGroup.Keys.Count == 1 || ButtonVM.Mode == DashboardMode.Edit) {
				TimerCallback (null);
				return;
			}
			//if (timer == null) {
			//	timer = new System.Threading.Timer (TimerCallback, null, TIMEOUT_MS, 0);
			//} else {
			//	timer.Change (TIMEOUT_MS, 0);
			//} 
		}

		void CategoryClicked (AnalysisEventButton category)
		{
			if (ViewModel.TagMode == TagMode.Predefined) {
				emitEvent = true;
				Active = true;
			} else if (ViewModel.TagMode == TagMode.Free) {
				if (!Recording) {
					StartRecording ();
				} else {
					emitEvent = true;
				}
			}
		}

		void TagClicked (TagVM tag)
		{
			if (SelectedTags.Contains (tag)) {
				SelectedTags.Remove (tag);
			} else {
				SelectedTags.RemoveAll (t => t.Group == tag.Group);
				SelectedTags.Add (tag);
			}
			if (ViewModel.TagMode == TagMode.Free) {
				StartRecording ();
			} else {
				Active = true;
				delayEvent = true;
			}
			ReDraw ();
		}

		void UpdateGroups ()
		{
			tagsByGroup = ViewModel.TagsByGroup;
		}

		public RangeObservableCollection<TagVM> SelectedTags {
			get {
				return ViewModel.SelectedTags;
			}
		}

		void AddSubcatAnchor (TagVM tag, Point point, double width, double height)
		{
			LinkAnchorView anchor;

			if (subcatAnchors.ContainsValue (tag)) {
				anchor = subcatAnchors.GetKeyByValue(tag);
				anchor.RelativePosition = point;
			} else {
				anchor = new LinkAnchorView (this, new List<TagVM> { tag }, point);
				anchor.RedrawEvent += (co, area) => {
					EmitRedrawEvent (anchor, area);
				};
				subcatAnchors.Add (anchor, tag);
			}
			anchor.Width = width;
			anchor.Height = height;
		}

		bool CheckRect (Point p, Rectangle rect, object obj)
		{
			Selection subsel;

			if (obj == null) {
				return false;
			}
			subsel = rect.GetSelection (p, 0);
			if (subsel != null) {
				if (obj is AnalysisEventButton) {
					CategoryClicked (Button);
				} else if (obj is TagVM) {
					TagClicked (obj as TagVM);
				} else if (obj == cancelButton) {
					cancelClicked = true;
				} else if (obj == editbutton) {
					editClicked = true;
				} else if (obj == applyButton) {
					applyClicked = true;
				}
				return true;
			}
			return false;
		}

		public override Selection GetSelection (Point p, double precision, bool inMotion = false)
		{
			if (ShowLinks) {
				Selection sel = anchor.GetSelection (p, precision, inMotion);
				if (sel != null)
					return sel;
				foreach (LinkAnchorView subcatAnchor in subcatAnchors.Keys) {
					sel = subcatAnchor.GetSelection (p, precision, inMotion);
					if (sel != null)
						return sel;
				}
			}
			return base.GetSelection (p, precision, inMotion);
		}

		/// <summary>
		/// Gets the anchor.
		/// </summary>
		/// <returns>The anchor.</returns>
		/// <param name="sourceTags">Source tags.</param>
		public override LinkAnchorView GetAnchor (IList<TagVM> sourceTags)
		{
			/* Only one tag is supported for now */
			if (sourceTags == null || sourceTags.Count == 0) {
				return base.GetAnchor (sourceTags);
			} else {
				return subcatAnchors.GetKeyByValue (sourceTags [0]);
			}
		}

		public override void ClickPressed (Point p, ButtonModifier modif, Selection selection)
		{
			if (ButtonVM.Mode == DashboardMode.Edit || Button.ShowSettingIcon) {
				editClicked = CheckRect (p, editRect, editbutton);
				if (editClicked || ButtonVM.Mode == DashboardMode.Edit)
					return;
			}

			foreach (Rectangle rect in buttonsRects.Keys) {
				if (CheckRect (p, rect, buttonsRects [rect])) {
					return;
				}
			}
			foreach (Rectangle rect in rects.Keys) {
				if (CheckRect (p, rect, rects [rect])) {
					return;
				}
			}
		}

		public override void ClickReleased ()
		{
			if (editClicked && !moved && EditButtonTagsEvent != null) {
				EditButtonTagsEvent (Button);
			} else if (cancelClicked) {
				Clear ();
			} else if (emitEvent) {
				EmitCreateEvent ();
			} else if (delayEvent) {
				if (SelectedTags.Count == tagsByGroup.Count) {
					EmitCreateEvent ();
				} else {
					DelayTagClicked ();
				}
			} else if (applyClicked) {
				if (SelectedTags.Count > 1) {
					EmitCreateEvent ();
				}
			}
			emitEvent = delayEvent = moved = editClicked = applyClicked = false;
		}

		void DrawTagsGroup (IDrawingToolkit tk, List<TagVM> tags, ref double yptr)
		{
			double rowwidth;
			Point start;
			int tagsPerRow, row = 0;

			tagsPerRow = Math.Max (1, ViewModel.TagsPerRow);
			rowwidth = catWidth / tagsPerRow;

			start = new Point (Position.X, Position.Y + HeaderHeight);
			tk.FontSize = 12;
			tk.FontWeight = FontWeight.Light;

			/* Draw tags */
			for (int i = 0; i < tags.Count; i++) {
				Point pos;
				int col;
				TagVM tag;

				row = i / tagsPerRow;
				col = i % tagsPerRow;
				pos = new Point (start.X + col * rowwidth,
					start.Y + yptr + row * heightPerRow);
				tag = tags [i];

				AddSubcatAnchor (tag, new Point (pos.X - Position.X, pos.Y - Position.Y),
					rowwidth, heightPerRow);
				if (!ShowTags) {
					continue;
				}

				tk.StrokeColor = Button.DarkColor;
				tk.LineWidth = 1;
				/* Draw last vertical line when the last row is not fully filled*/
				if (col == 0) {
					if (i + tagsPerRow > tags.Count) {
						tk.LineStyle = LineStyle.Dashed;
						var st = new Point (pos.X + rowwidth * ((i + 1) % tagsPerRow), pos.Y);
						tk.DrawLine (st, new Point (st.X, st.Y + heightPerRow));
						tk.LineStyle = LineStyle.Normal;
					}
				}

				if (col == 0) {
					if (row != 0) {
						tk.LineStyle = LineStyle.Dashed;
					}
					/* Horizontal line */
					tk.DrawLine (pos, new Point (pos.X + catWidth, pos.Y));
					tk.LineStyle = LineStyle.Normal;
				} else {
					/* Vertical line */
					tk.LineStyle = LineStyle.Dashed;
					tk.DrawLine (pos, new Point (pos.X, pos.Y + heightPerRow));
					tk.LineStyle = LineStyle.Normal;
				}
				tk.StrokeColor = Button.TextColor;
				tk.DrawText (pos, rowwidth, heightPerRow, tag.Value);
				rects.Add (new Rectangle (pos, rowwidth, heightPerRow), tag);
			}
			yptr += heightPerRow * (row + 1);
		}

		/// <summary>
		/// Draws the header.
		/// </summary>
		/// <param name="tk">Tk.</param>
		protected virtual void DrawHeader (IDrawingToolkit tk)
		{
			Color textColor;
			Point pos;
			double width, height;
			int fontSize;
			bool ellipsize = true;

			if (Active) {
				textColor = BackgroundColor;
			} else {
				textColor = TextColor;
			}

			width = HeaderTextWidth;
			height = HeaderHeight;
			pos = new Point (Position.X + HeaderTextOffset, Position.Y);
			fontSize = StyleConf.ButtonHeaderFontSize;

			if (ShowTags) {
				rects.Add (new Rectangle (Position, Width, HeaderHeight), Button);
				if (Recording) {
					/* Draw Timer instead */
					return;
				}
			} else {
				if (!Recording && Icon != null) {
					width = Width;
					height = Height - HeaderHeight;
					pos = new Point (Position.X, Position.Y + HeaderHeight);
					fontSize = StyleConf.ButtonNameFontSize;
					ellipsize = false;
				} else {
					width = HeaderTextWidth;
					height = HeaderHeight;
					pos = new Point (Position.X + 5, Position.Y + 5);
					fontSize = StyleConf.ButtonHeaderFontSize;
				}
				rects.Add (new Rectangle (Position, Width, Height), Button);
			}
			tk.FontSize = fontSize;
			tk.StrokeColor = BackgroundColor;
			tk.StrokeColor = textColor;
			tk.FontWeight = FontWeight.Light;
			tk.DrawText (pos, width, height, ViewModel.Name, false, ellipsize);
		}

		void DrawEditButton (IDrawingToolkit tk)
		{
			Point pos;
			Color c;
			double width, height;

			if ((ButtonVM.Mode != DashboardMode.Edit || ShowLinks || !ViewModel.ShowSubcategories) && !Button.ShowSettingIcon) {
				return;
			}

			c = App.Current.Style.PaletteBackgroundDark;
			width = StyleConf.ButtonRecWidth;
			height = HeaderHeight;
			pos = new Point (Position.X + Width - StyleConf.ButtonRecWidth,
				Position.Y + Height - height);
			tk.LineWidth = 0;
			tk.FillColor = new Color (c.R, c.G, c.B, 200);
			tk.StrokeColor = BackgroundColor;
			tk.DrawRectangle (pos, width, height);
			tk.StrokeColor = Color.Green1;
			tk.FillColor = Color.Green1;
			tk.FontSize = StyleConf.ButtonButtonsFontSize;
			editRect.Update (pos, width, height);
			buttonsRects [editRect] = editbutton;
			pos = new Point (pos.X, pos.Y + 5);
			tk.DrawImage (pos, width, height - 10, editImage, ScaleMode.AspectFit, true);
		}

		void DrawSelectedTags (IDrawingToolkit tk)
		{
			if (ButtonVM.Mode == DashboardMode.Edit) {
				return;
			}
			foreach (Rectangle r in rects.Keys) {
				object obj = rects [r];
				if (obj is TagVM && SelectedTags.Contains (obj as TagVM)) {
					tk.LineWidth = 0;
					tk.FontWeight = FontWeight.Light;
					tk.FillColor = TextColor;
					tk.FontSize = 12;
					tk.DrawRectangle (new Point (r.TopLeft.X, r.TopLeft.Y), r.Width, r.Height);
					tk.StrokeColor = BackgroundColor;
					tk.DrawText (new Point (r.TopLeft.X, r.TopLeft.Y), r.Width, r.Height,
						(obj as TagVM).Value);
				}
			}
		}

		protected virtual void DrawRecordTime (IDrawingToolkit tk)
		{
			if (Recording && ButtonVM.Mode != DashboardMode.Edit && ViewModel.ButtonTime != null) {
				if (ShowTags) {
					tk.FontSize = 12;
					tk.FontWeight = FontWeight.Normal;
					tk.StrokeColor = BackgroundColor;
					tk.DrawText (new Point (Position.X + HeaderTextOffset, Position.Y),
						HeaderTextWidth, HeaderHeight,
								 ViewModel.ButtonTime.ToSecondsString ());
				} else {
					tk.FontSize = 24;
					tk.FontWeight = FontWeight.Bold;
					tk.StrokeColor = BackgroundColor;
					tk.DrawText (new Point (Position.X, Position.Y + HeaderHeight),
						Width, Height - HeaderHeight,
								 ViewModel.ButtonTime.ToSecondsString ());
				}
			}
		}

		void DrawApplyButton (IDrawingToolkit tk)
		{
			Point pos;
			double width, height;

			if (!ShowApplyButton || SelectedTags.Count == 0) {
				rects [applyRect] = null;
				return;
			}

			pos = new Point (Position.X + Width - StyleConf.ButtonRecWidth,
				Position.Y);
			width = StyleConf.ButtonRecWidth;
			height = HeaderHeight;
			tk.FillColor = App.Current.Style.PaletteBackgroundDark;
			tk.LineWidth = 0;
			tk.DrawRectangle (pos, width, height);
			tk.StrokeColor = Color.Green1;
			tk.FillColor = Color.Green1;
			tk.FontSize = 12;
			applyRect.Update (pos, width, height);
			buttonsRects [applyRect] = applyButton;
			pos = new Point (pos.X, pos.Y + 5);
			tk.DrawImage (pos, width, height - 10, applyImage, ScaleMode.AspectFit, true);
		}

		protected virtual void DrawRecordButton (IDrawingToolkit tk)
		{
			Point pos, bpos;
			double width, height;

			if (ViewModel.TagMode != TagMode.Free || ShowLinks) {
				return;
			}

			pos = new Point (Position.X + Width - StyleConf.ButtonRecWidth,
				Position.Y);
			bpos = new Point (pos.X, pos.Y + 5);

			width = StyleConf.ButtonRecWidth;
			height = HeaderHeight;
			tk.FontSize = StyleConf.ButtonButtonsFontSize;
			if (!Recording) {
				tk.FillColor = App.Current.Style.PaletteBackgroundDark;
				tk.StrokeColor = BackgroundColor;
				tk.LineWidth = StyleConf.ButtonLineWidth;
				tk.DrawRectangle (pos, width, height);
				tk.StrokeColor = Color.Red1;
				tk.FillColor = Color.Red1;
				tk.DrawImage (bpos, width, height - 10, recImage, ScaleMode.AspectFit, true);
			} else {
				tk.FillColor = tk.StrokeColor = BackgroundColor;
				tk.DrawRectangle (pos, width, height);
				tk.StrokeColor = TextColor;
				tk.FillColor = TextColor;
				tk.DrawImage (bpos, width, height - 10, cancelImage, ScaleMode.AspectFit, true);
				cancelRect.Update (pos, width, height);
				buttonsRects [cancelRect] = cancelButton;
			}
		}

		void DrawAnchors (IDrawingToolkit tk)
		{
			if (!ShowLinks)
				return;

			anchor.Height = HeaderHeight;
			DrawAnchor (tk, null);
			foreach (LinkAnchorView a in subcatAnchors.Keys) {
				a.Draw (tk, null);
			}
		}

		/// <summary>
		/// Draws the button.
		/// </summary>
		/// <param name="tk">Tk.</param>
		protected override void DrawButton (IDrawingToolkit tk)
		{
			if (!ShowTags) {
				base.DrawButton (tk);
			} else {
				tk.FillColor = BackgroundColor;
				tk.StrokeColor = TextColor;
				tk.LineWidth = 0;
				tk.DrawRectangle (Position, Width, Height);
				if (Active) {
					tk.FillColor = TextColor;
					tk.DrawRectangle (Position, Width, HeaderHeight);
				}
				if (Icon != null) {
					if (Active) {
						tk.FillColor = BackgroundColor;
					} else {
						tk.FillColor = TextColor;
					}
					tk.DrawImage (new Point (Position.X + 5, Position.Y + 5),
						StyleConf.ButtonHeaderWidth, StyleConf.ButtonHeaderHeight, Icon, ScaleMode.AspectFit, true);
				}
			}
		}

		void DrawBackbuffer (IDrawingToolkit tk)
		{
			Point pos;
			double yptr = 0;

			rects.Clear ();
			buttonsRects.Clear ();
			UpdateGroups ();
			UpdateRows ();
			heightPerRow = (Height - HeaderHeight) / nrows;
			catWidth = Width;
			pos = Position;

			tk.Begin ();
			if (UseBackBufferSurface) {
				tk.TranslateAndScale (new Point (-Position.X, -Position.Y),
					new Point (1, 1));
			}
			tk.FontWeight = FontWeight.Bold;

			/* Draw Rectangle */
			DrawButton (tk);
			DrawImage (tk);
			DrawHeader (tk);
			DrawRecordButton (tk);
			DrawHotkey (tk);

			foreach (List<TagVM> tags in tagsByGroup.Values) {
				DrawTagsGroup (tk, tags, ref yptr);
			}

			/* Remove anchor object that where not reused
				 * eg: after removinga a subcategory tag */
			foreach (var tagEntry in subcatAnchors.ToList ()) {
				if (!ViewModel.Tags.Contains (tagEntry.Value)) {
					RemoveAnchor (tagEntry.Key);
				}
			}
			if (!ShowLinks) {
				DrawEditButton (tk);
			}

			tk.End ();
		}

		void CreateBackBufferSurface ()
		{
			IDrawingToolkit tk = App.Current.DrawingToolkit;

			ResetBackbuffer ();

			backBufferSurface = tk.CreateSurface ((int)Width, (int)Height);
			using (IContext c = backBufferSurface.Context) {
				tk.Context = c;
				DrawBackbuffer (tk);
			}
		}

		public override void Move (Selection s, Point p, Point start)
		{
			base.Move (s, p, start);
			moved = true;
			SelectedTags.Clear ();
			switch (s.Position) {
			case SelectionPosition.Right:
			case SelectionPosition.Bottom:
			case SelectionPosition.BottomRight:
				CreateBackBufferSurface ();
				break;
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			IContext ctx = tk.Context;
			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			}
			if (UseBackBufferSurface && backBufferSurface == null) {
				CreateBackBufferSurface ();
			}
			tk.Context = ctx;
			tk.Begin ();

			if (UseBackBufferSurface) {
				tk.DrawSurface (backBufferSurface, Position);
			} else {
				DrawBackbuffer (tk);
			}

			DrawSelectedTags (tk);
			DrawRecordTime (tk);
			DrawApplyButton (tk);
			DrawSelectionArea (tk);
			DrawAnchors (tk);
			tk.End ();
		}

		protected override void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			base.HandlePropertyChanged (sender, e);
			if (sender == ViewModel && (
				TimedButtonVM.NeedsSync (e, nameof (ViewModel.TagsPerRow)) ||
				TimedButtonVM.NeedsSync (e, nameof (ViewModel.ShowSubcategories)))) {
				ReDraw ();
			}
		}
	}
}

