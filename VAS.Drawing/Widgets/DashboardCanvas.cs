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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;
using VAS.Drawing.CanvasObjects.Dashboard;
using VASDrawing = VAS.Drawing;

namespace VAS.Drawing.Widgets
{
	public class DashboardCanvas : SelectionCanvas, IView<DashboardVM>
	{
		public event ButtonSelectedHandler EditButtonTagsEvent;
		public event ActionLinksSelectedHandler ActionLinksSelectedEvent;
		public event ActionLinkCreatedHandler ActionLinkCreatedEvent;
		public event ShowDashboardMenuHandler ShowMenuEvent;
		public event NewEventHandler NewTagEvent;

		protected int templateWidth, templateHeight;
		protected ActionLinkView movingLink;
		protected LinkAnchorView destAnchor;
		protected Dictionary<DashboardButtonVM, DashboardButtonView> buttonsDict;
		protected Dictionary<ActionLinkVM, ActionLinkView> linksDict;

		DashboardVM viewModel;

		public DashboardCanvas (IWidget widget) : base (widget)
		{
			Accuracy = 5;
			BackgroundColor = App.Current.Style.PaletteBackground;
			buttonsDict = new Dictionary<DashboardButtonVM, DashboardButtonView> ();
			linksDict = new Dictionary<ActionLinkVM, ActionLinkView> ();
		}

		public DashboardCanvas () : this (null)
		{
		}

		public DashboardVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandlePropertyChanged;
					viewModel.ViewModels.CollectionChanged -= HandleViewModelsCollectionChanged;
				}
				ClearCanvas ();
				viewModel = value;
				if (viewModel != null) {
					FillCanvas ();
					viewModel.PropertyChanged += HandlePropertyChanged;
					viewModel.ViewModels.CollectionChanged += HandleViewModelsCollectionChanged;
					ViewModel.Sync ();
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (DashboardVM)viewModel;
		}

		public void Click (DashboardButton b, TagVM tag = null)
		{
			DashboardButtonView co = Objects.OfType<DashboardButtonView> ().FirstOrDefault (o => o.Button == b);
			if (tag != null && co is AnalysisEventButtonView) {
				(co as AnalysisEventButtonView).ClickTag (tag);
			} else {
				co.Click ();
			}
		}

		public void RedrawButton (DashboardButton b)
		{
			DashboardButtonView co = Objects.OfType<DashboardButtonView> ().FirstOrDefault (o => o.Button == b);
			if (co != null) {
				co.ReDraw ();
			}
		}

		public void Refresh (DashboardButton b = null)
		{
			DashboardButtonView to;

			if (ViewModel == null) {
				return;
			}
			ClearCanvas ();
			FillCanvas ();
			to = Objects.OfType<DashboardButtonView> ().
				FirstOrDefault (o => o.Button == b);
			if (to != null) {
				UpdateSelection (new Selection (to, SelectionPosition.All, 0));
			}
		}

		public override void SetWidget (IWidget newWidget)
		{
			base.SetWidget (newWidget);
			HandleSizeChangedEvent ();
		}

		protected override void ShowMenu (Point coords)
		{
			List<DashboardButtonVM> buttons;
			List<ActionLinkVM> links;

			if (ShowMenuEvent == null || Selections.Count == 0)
				return;

			buttons = Selections.Where (s => s.Drawable is DashboardButtonView).
								Select (s => (s.Drawable as DashboardButtonView).ButtonVM).ToList ();
			links = Selections.Where (s => s.Drawable is ActionLinkView).
				Select (s => (s.Drawable as ActionLinkView).Link).ToList ();
			ShowMenuEvent (buttons, links);
		}

		protected override Selection GetSelection (Point coords, bool inMotion = false, bool skipSelected = false)
		{
			Selection sel = null;
			Selection selected = null;

			/* Regular GetSelection */
			if (!ViewModel.ShowLinks)
				return base.GetSelection (coords, inMotion, skipSelected);

			/* With ShowLinks, only links and anchor can be selected */
			if (Selections.Count > 0) {
				selected = Selections.LastOrDefault ();
			}

			foreach (ICanvasSelectableObject co in Objects) {
				sel = co.GetSelection (coords, Accuracy, inMotion);
				if (sel == null || sel.Drawable is DashboardButtonView)
					continue;
				if (skipSelected && selected != null && sel.Drawable == selected.Drawable)
					continue;
				break;
			}
			return sel;
		}

		protected override void SelectionMoved (Selection sel)
		{
			if (sel.Drawable is DashboardButtonView) {
				HandleSizeChangedEvent ();
			} else if (sel.Drawable is ActionLinkView) {
				ActionLinkView link = sel.Drawable as ActionLinkView;
				LinkAnchorView anchor = null;
				Selection destSel;

				destSel = GetSelection (MoveStart, true, true);
				if (destSel != null && destSel.Drawable is LinkAnchorView) {
					anchor = destSel.Drawable as LinkAnchorView;
				}
				/* Toggled highlited state */
				if (anchor != destAnchor) {
					if (destAnchor != null) {
						destAnchor.Highlighted = false;
					}
					/* Only highlight valid targets */
					if (link.CanLink (anchor)) {
						anchor.Highlighted = true;
						destAnchor = anchor;
					} else {
						destAnchor = null;
					}
				}
			}
			base.SelectionMoved (sel);
		}

		protected override void SelectionChanged (List<Selection> sel)
		{
			if (sel.Count == 0) {
				ViewModel.Selection.Clear ();
				return;
			}
			if (sel [0].Drawable is DashboardButtonView) {
				ViewModel.Selection.Reset (sel.Select (s => (s.Drawable as DashboardButtonView).ButtonVM));
			} else if (sel [0].Drawable is ActionLinkView) {
				List<ActionLinkVM> links;

				links = sel.Select (s => (s.Drawable as ActionLinkView).Link).ToList ();
				if (ViewModel.Mode == DashboardMode.Edit) {
					if (ActionLinksSelectedEvent != null) {
						ActionLinksSelectedEvent (links);
					}
				}
			}
			base.SelectionChanged (sel);
		}

		protected override void StartMove (Selection sel)
		{
			if (sel != null && sel.Drawable is LinkAnchorView) {
				LinkAnchorView anchor = sel.Drawable as LinkAnchorView;
				ActionLinkVM link = new ActionLinkVM {
					Model = new ActionLink { SourceTags = new RangeObservableCollection<Tag> (anchor.Tags.Select (t => t.Model)) },
					SourceButton = anchor.Button.ButtonVM
				};
				movingLink = new ActionLinkView (anchor, null, link);
				AddObject (movingLink);
				ClearSelection ();
				UpdateSelection (new Selection (movingLink, SelectionPosition.LineStop, 0), false);
			}
			base.StartMove (sel);
		}

		protected override void StopMove (bool moved)
		{
			Selection sel = Selections.FirstOrDefault ();

			if (movingLink != null) {
				if (destAnchor != null) {
					ActionLinkVM link = movingLink.Link;
					link.DestinationButton = destAnchor.Button.ButtonVM;
					link.DestinationTags.ViewModels.AddRange (destAnchor.Tags);
					link.SourceButton.ActionLinks.ViewModels.Add (link);
					movingLink.Destination = destAnchor;
					destAnchor.Highlighted = false;
					if (ActionLinkCreatedEvent != null) {
						ActionLinkCreatedEvent (link);
					}
					linksDict.Add (link, movingLink);
				} else {
					RemoveObject (movingLink);
					widget.ReDraw ();
				}
				ClearSelection ();
				movingLink = null;
				destAnchor = null;
				return;
			}

			if (sel != null && moved) {
				if (sel.Drawable is DashboardButtonView) {
					/* Round the position of the button to match a corner in the grid */
					int i = VASDrawing.Constants.CATEGORY_TPL_GRID;
					DashboardButton tb = (sel.Drawable as DashboardButtonView).Button;
					tb.Position.X = VASDrawing.Utils.Round (tb.Position.X, i);
					tb.Position.Y = VASDrawing.Utils.Round (tb.Position.Y, i);
					tb.Width = (int)VASDrawing.Utils.Round (tb.Width, i);
					tb.Height = (int)VASDrawing.Utils.Round (tb.Height, i);
					(sel.Drawable as DashboardButtonView).ResetDrawArea ();
					widget.ReDraw ();
				}
			}
			base.StopMove (moved);
		}

		public override void Draw (IContext context, Area area)
		{
			tk.Context = context;
			DrawBackground ();
			Begin (context);
			if (ViewModel.Mode != DashboardMode.Code) {
				/* Draw grid */
				tk.LineWidth = 1;
				tk.StrokeColor = Color.Grey1;
				tk.FillColor = Color.Grey1;
				/* Vertical lines */
				for (int i = 0; i <= templateHeight; i += VASDrawing.Constants.CATEGORY_TPL_GRID) {
					tk.DrawLine (new Point (0, i), new Point (templateWidth, i));
				}
				/* Horizontal lines */
				for (int i = 0; i < templateWidth; i += VASDrawing.Constants.CATEGORY_TPL_GRID) {
					tk.DrawLine (new Point (i, 0), new Point (i, templateHeight));
				}
			}
			DrawObjects (area);
			End ();
		}

		protected virtual void ClearCanvas ()
		{
			ClearObjects ();
			foreach (var vm in buttonsDict.Keys) {
				vm.ActionLinks.ViewModels.CollectionChanged -= HandleActionLinksCollectionChanged;
			}
			buttonsDict.Clear ();
			linksDict.Clear ();
		}

		protected virtual void FillCanvas ()
		{
			AddButtonsWithActionLinks (ViewModel.ViewModels);
			HandleSizeChangedEvent ();
		}

		protected override void HandleSizeChangedEvent ()
		{
			if (ViewModel == null || widget == null) {
				return;
			}

			base.HandleSizeChangedEvent ();

			FitMode prevFitMode = ViewModel.FitMode;
			templateHeight = ViewModel.CanvasHeight + 10;
			templateWidth = ViewModel.CanvasWidth + 10;
			/* When going from Original to Fill or Fit, we can't know the new 
			 * size of the shrinked object until we have a resize */
			if (ViewModel.FitMode == FitMode.Original) {
				widget.Width = templateWidth;
				widget.Height = templateHeight;
				ScaleX = ScaleY = 1;
				Translation = new Point (0, 0);
			} else if (ViewModel.FitMode == FitMode.Fill) {
				ScaleX = (double)widget.Width / templateWidth;
				ScaleY = (double)widget.Height / templateHeight;
				Translation = new Point (0, 0);
			} else if (ViewModel.FitMode == FitMode.Fit) {
				double scaleX, scaleY;
				Point translation;
				Image.ScaleFactor (templateWidth, templateHeight,
					(int)widget.Width, (int)widget.Height, ScaleMode.AspectFit,
					out scaleX, out scaleY, out translation);
				ScaleX = scaleX;
				ScaleY = scaleY;
				Translation = translation;
			}

			widget.ReDraw ();
		}

		void AddButton (DashboardButtonVM vm)
		{
			IView view = App.Current.ViewLocator.Retrieve (vm.View);
			view.SetViewModel (vm);
			var viewButton = view as DashboardButtonView;
			if (viewButton is AnalysisEventButtonView) {
				((AnalysisEventButtonView)viewButton).EditButtonTagsEvent += (t) => {
					if (EditButtonTagsEvent != null)
						EditButtonTagsEvent (t);
				};
			}
			viewButton.ShowLinks = ViewModel.ShowLinks;
			AddObject (viewButton);
			buttonsDict.Add (vm, viewButton);
			vm.ActionLinks.ViewModels.CollectionChanged += HandleActionLinksCollectionChanged;
		}

		void AddActionLinks (DashboardButtonView buttonObject)
		{
			foreach (ActionLinkVM link in buttonObject.ButtonVM.ActionLinks) {
				LinkAnchorView sourceAnchor, destAnchor;
				ActionLinkView linkObject;

				sourceAnchor = buttonObject.GetAnchor (link.SourceTags.ViewModels);
				try {
					var but = buttonsDict [link.DestinationButton];
					destAnchor = buttonsDict [link.DestinationButton].GetAnchor (link.DestinationTags.ViewModels);
				} catch {
					Log.Error ("Skipping link with invalid destination tags");
					continue;
				}
				linkObject = new ActionLinkView (sourceAnchor, destAnchor, link);
				link.SourceButton = buttonObject.ButtonVM;
				linkObject.Visible = ViewModel.ShowLinks;
				AddObject (linkObject);
				linksDict.Add (link, linkObject);
			}
		}

		void AddButtonsWithActionLinks (IEnumerable<DashboardButtonVM> buttons)
		{
			Objects.RemoveAll (o => o is ActionLinkView);
			linksDict.Clear ();
			foreach (var button in buttons) {
				AddButton (button);
			}
			foreach (var button in buttonsDict.Values) {
				AddActionLinks (button);
			}
		}

		void RemoveButton (DashboardButtonVM vm)
		{
			RemoveObject (buttonsDict [vm]);
			buttonsDict.Remove (vm);
			vm.ActionLinks.ViewModels.CollectionChanged -= HandleActionLinksCollectionChanged;
		}

		void UpdateMode ()
		{
			ObjectsCanMove = ViewModel.Mode == DashboardMode.Edit;
			ClearSelection ();
		}

		void UpdateShowLinks ()
		{
			foreach (DashboardButtonView to in Objects.OfType<DashboardButtonView> ()) {
				to.ShowLinks = ViewModel.ShowLinks;
				to.ResetDrawArea ();
			}
			foreach (ActionLinkView ao in Objects.OfType<ActionLinkView> ()) {
				ao.Visible = ViewModel.ShowLinks;
				ao.ResetDrawArea ();
			}
			ClearSelection ();
			widget?.ReDraw ();
		}

		void SyncSelection ()
		{
			ClearSelection ();
			var selections = new List<Selection> ();
			foreach (var button in ViewModel.Selection) {
				var view = buttonsDict [button];
				view.Selected = true;
				selections.Add (new Selection (view, SelectionPosition.All));
			}
			Selections = selections;
		}

		void HandleViewModelsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				AddButtonsWithActionLinks (e.NewItems.OfType<DashboardButtonVM> ());
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (DashboardButtonVM viewModel in e.OldItems.OfType<DashboardButtonVM> ()) {
					RemoveButton (viewModel);
				}
				break;
			case NotifyCollectionChangedAction.Reset:
				ClearCanvas ();
				FillCanvas ();
				break;
			}
			HandleSizeChangedEvent ();
		}

		void HandleActionLinksCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Remove) {
				foreach (ActionLinkVM viewModel in e.OldItems.OfType<ActionLinkVM> ()) {
					RemoveObject (linksDict [viewModel]);
					linksDict.Remove (viewModel);
					widget.ReDraw ();
				}
			}
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (sender != ViewModel) {
				return;
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.Mode))) {
				UpdateMode ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.ShowLinks))) {
				UpdateShowLinks ();
			}
			if (ViewModel.NeedsSync (e, nameof (ViewModel.FitMode))) {
				HandleSizeChangedEvent ();
			}
			if (ViewModel.NeedsSync (e, $"Collection_{nameof (DashboardVM.Selection)}")) {
				SyncSelection ();
			}
		}
	}
}
