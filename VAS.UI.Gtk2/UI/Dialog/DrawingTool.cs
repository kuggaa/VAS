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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Gtk;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;
using VAS.Drawing.Cairo;
using VAS.Drawing.Widgets;
using VAS.Services.State;
using VAS.Services.ViewModel;
using VAS.UI.Component;
using VAS.UI.Helpers;
using VAS.UI.Helpers.Bindings;
using Color = VAS.Core.Common.Color;
using Drawable = VAS.Core.Store.Drawables.Drawable;
using Image = VAS.Core.Common.Image;
using Misc = VAS.UI.Helpers.Misc;

namespace VAS.UI.Dialog
{
	[ViewAttribute (DrawingToolState.NAME)]
	public abstract partial class DrawingTool : Gtk.Dialog, IPanel<DrawingToolVM>
	{
		const int MOVE_OFFSET = 5;
		const int TOOL_HEIGHT = 24;
		const double ZOOM_STEP = 0.2;
		const double ZOOM_PAGE = 0.2;

		readonly Blackboard blackboard;
		TimelineEventVM playVM;
		FrameDrawing drawing;
		CameraConfig camConfig;
		Drawable selectedDrawable;
		protected Gtk.Dialog playerDialog;
		protected Text playerText;
		double scaleFactor;
		bool ignoreChanges;
		DrawingToolVM viewModel;
		BindingContext ctx;
		protected Dictionary<RadioButton, DrawTool> buttonToDrawTool;
		Dictionary<DrawTool, ToolSettingBase> toolSettings;

		public DrawingTool ()
		{
			this.Build ();
			blackboard = new Blackboard (new WidgetWrapper (drawingarea));
			blackboard.ConfigureObjectEvent += HandleConfigureObjectEvent;
			blackboard.ShowMenuEvent += HandleShowMenuEvent;
			blackboard.DrawableChangedEvent += HandleDrawableChangedEvent;
			blackboard.DrawToolChanged += HandleDrawToolChangedEvent;
			blackboard.RegionOfInterestChanged += HandleRegionOfInterestChanged;

			selectbutton.Active = true;

			buttonToDrawTool = new Dictionary<RadioButton, DrawTool> () {
				{selectbutton, DrawTool.Selection},
				{eraserbutton,DrawTool.Eraser},
				{penbutton,DrawTool.Pen},
				{textbutton,DrawTool.Text},
				{linebutton,DrawTool.Line},
				{crossbutton,DrawTool.Cross},
				{rectanglebutton,DrawTool.Rectangle},
				{ellipsebutton,DrawTool.Ellipse},
				{rectanglefilledbutton,DrawTool.RectangleArea},
				{ellipsefilledbutton,DrawTool.CircleArea},
				{numberbutton,DrawTool.Counter},
				{anglebutton,DrawTool.Angle},
				{playerbutton,DrawTool.Player},
				{zoombutton,DrawTool.Zoom},
			};

			foreach (var button in buttonToDrawTool.Keys) {
				button.Name = "DrawingToolButton-" + button.Name;
				button.Toggled += HandleToolClicked;
			}
			
			CreateToolSettings ();
			UpdateSettingsVisibility (DrawTool.Selection);

			selectbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-select", 20);
			eraserbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-eraser", 20);
			penbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-pencil", 20);
			textbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-text", 20);
			linebuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-arrow", 20);
			crossbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-mark", 20);
			rectanglebuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-square", 20);
			ellipsebuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-circle", 20);
			rectanglefilledbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-square-fill", 20);
			ellipsefilledbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-circle-fill", 20);
			playerbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-person", 20);
			numberbuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-counter", 20);
			anglebuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-angle", 20);
			zoombuttonimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-search", 20);
			zoomoutimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-zoom-out", 14);
			zoominimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-zoom-in", 14);

			// Force tooltips to be translatable as there seems to be a bug in stetic 
			// code generation for translatable tooltips.
			selectbutton.TooltipMarkup = Catalog.GetString ("Selection tool");
			eraserbutton.TooltipMarkup = Catalog.GetString ("Eraser tool");
			penbutton.TooltipMarkup = Catalog.GetString ("Pencil tool");
			textbutton.TooltipMarkup = Catalog.GetString ("Text tool");
			linebutton.TooltipMarkup = Catalog.GetString ("Line tool");
			crossbutton.TooltipMarkup = Catalog.GetString ("Cross tool");
			rectanglebutton.TooltipMarkup = Catalog.GetString ("Rectangle tool");
			ellipsebutton.TooltipMarkup = Catalog.GetString ("Ellipse tool");
			rectanglefilledbutton.TooltipMarkup = Catalog.GetString ("Rectangle area tool");
			ellipsefilledbutton.TooltipMarkup = Catalog.GetString ("Ellipsis area tool");
			playerbutton.TooltipMarkup = Catalog.GetString ("Player tool");
			numberbutton.TooltipMarkup = Catalog.GetString ("Index tool");
			anglebutton.TooltipMarkup = Catalog.GetString ("Angle tool");
			stylecombobox.TooltipMarkup = Catalog.GetString ("Change the line style");
			typecombobox.TooltipMarkup = Catalog.GetString ("Change the line style");
			clearbutton.TooltipMarkup = Catalog.GetString ("Clear all drawings");
			zoombutton.TooltipMarkup = Catalog.GetString ("Zoom tool. Click to zoom in, Alt+Shift to zoom out");

			FillLineStyle ();
			FillLineType ();

			colorbutton.ColorSet += HandleColorSet;
			colorbutton.Color = Misc.ToGdkColor (Color.Red1);
			colorbutton.UseAlpha = true;
			colorbutton.Alpha = Color.FloatToUShort (0.8f);
			blackboard.Color = Color.Red1;
			blackboard.Color.SetAlpha (0.8f);
			textcolorbutton.ColorSet += HandleTextColorSet;
			textcolorbutton.Color = Misc.ToGdkColor (Color.White);
			blackboard.TextColor = Color.White;
			backgroundcolorbutton.UseAlpha = true;
			backgroundcolorbutton.Alpha = 0;
			backgroundcolorbutton.ColorSet += HandleBackgroundColorSet;
			backgroundcolorbutton.Color = Misc.ToGdkColor (Color.Green1);
			blackboard.TextBackgroundColor = Misc.ToLgmColor (backgroundcolorbutton.Color,
			   backgroundcolorbutton.Alpha);
			textspinbutton.Value = 12;
			textspinbutton.ValueChanged += (sender, e) => UpdateTextSize ();
			linesizespinbutton.ValueChanged += (sender, e) => UpdateLineWidth ();
			linesizespinbutton.Value = 2;

			zoomscale.CanFocus = false;
			hscrollbar.ValueChanged += HandleScrollValueChanged;
			wscrollbar.ValueChanged += HandleScrollValueChanged;
			hscrollbar.Visible = wscrollbar.Visible = false;


			Misc.SetFocus (this, false);

			var saveToFileCommand = new AsyncCommand (SaveToFile);
			saveToFileCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("vas-save", App.Current.Style.IconSmallWidth);
			saveToFileCommand.Text = Catalog.GetString ("Save to File");
			savebutton.BindManually (saveToFileCommand);

			var saveToProjectCommand = new AsyncCommand (SaveToProject);
			saveToProjectCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("vas-save", App.Current.Style.IconSmallWidth);
			saveToProjectCommand.Text = Catalog.GetString ("Save to Project");
			savetoprojectbutton.BindManually (saveToProjectCommand);

			var clearCommand = new Command (Clear);
			clearCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("vas-delete", App.Current.Style.IconSmallWidth);
			clearbutton.BindManually (clearCommand);

			Bind ();
		}

		public DrawingTool (Window parent) : this ()
		{
			TransientFor = parent;
		}

		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			if (disposing) {
				Destroy ();
			}
			Disposed = true;
		}

		protected override void OnDestroyed ()
		{
			Log.Verbose ($"Destroying {GetType ()}");

			OnUnload ();

			// Dispose things here
			blackboard.Dispose ();
			ViewModel = null;
			ctx?.Dispose ();
			ctx = null;

			base.OnDestroyed ();

			Disposed = true;
		}


		public DrawingToolVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandleViewModelPropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.PropertyChanged += HandleViewModelPropertyChanged;
					if (viewModel.TimelineEventVM?.Model != null) {
						LoadPlay (viewModel.TimelineEventVM, viewModel.Frame, viewModel.Drawing, viewModel.CameraConfig);
					} else {
						LoadFrame (viewModel.Frame);
					}
				}
				ctx.UpdateViewModel (viewModel);
			}
		}

		protected bool Disposed { get; private set; } = false;

		public KeyContext GetKeyContext ()
		{
			var keyContext = new KeyContext ();
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName (GeneralUIHotkeys.COPY), blackboard.Copy)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName (GeneralUIHotkeys.PASTE), blackboard.Paste)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName (GeneralUIHotkeys.DELETE), blackboard.DeleteSelection)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_SELECT"),
					selectbutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_ERASER"),
					eraserbutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_PENCIL_TOOL"),
					penbutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_TEXT_TOOL"),
					textbutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_LINE_TOOL"),
					linebutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_X_TOOL"),
					crossbutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_SQUARE"),
					rectanglebutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_FILLED_SQUARE"),
					rectanglefilledbutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_CIRCLE"),
					ellipsebutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_FILLED_CIRCLE"),
					ellipsefilledbutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_COUNTER"),
					numberbutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName ("DRAWING_TOOL_CLEAR_ALL"),
					clearbutton.Click)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName (GeneralUIHotkeys.DELETE),
					blackboard.DeleteSelection)
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName (GeneralUIHotkeys.SAVE),
					() => {
						if (savetoprojectbutton.Visible) {
							savetoprojectbutton.Click ();
						}
					})
			);
			keyContext.AddAction (
				 new KeyAction (
					 App.Current.HotkeysService.GetByName ("DRAWING_TOOL_EXPORT_IMAGE"),
					 savebutton.Click)
			 );
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName (DrawingToolHotkeys.DRAWING_TOOL_MOVE_RIGHT),
					() => blackboard.MoveSelected (new Point (MOVE_OFFSET, 0)))
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName (DrawingToolHotkeys.DRAWING_TOOL_MOVE_LEFT),
					() => blackboard.MoveSelected (new Point (-MOVE_OFFSET, 0)))
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName (DrawingToolHotkeys.DRAWING_TOOL_MOVE_UP),
					() => blackboard.MoveSelected (new Point (0, -MOVE_OFFSET)))
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName (DrawingToolHotkeys.DRAWING_TOOL_MOVE_DOWN),
					() => blackboard.MoveSelected (new Point (0, MOVE_OFFSET)))
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName (DrawingToolHotkeys.DRAWING_TOOL_MOVE_TO_FRONT),
					() => blackboard.MoveToFront ())
			);
			keyContext.AddAction (
				new KeyAction (
					App.Current.HotkeysService.GetByName (DrawingToolHotkeys.DRAWING_TOOL_MOVE_TO_BACK),
					() => blackboard.MoveToBack ())
			);
			return keyContext;
		}

		public void OnLoad ()
		{
		}

		public void OnUnload ()
		{
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (DrawingToolVM)viewModel;
		}

		protected bool PlayerButtonVisibility {
			get {
				return playerbutton.Visible;
			}
			set {
				playerbutton.Visible = value;
			}
		}

		public void LoadPlay (TimelineEventVM playVM, Image frame, FrameDrawing drawing,
							  CameraConfig camConfig)
		{
			this.playVM = playVM;
			this.drawing = drawing;
			this.camConfig = camConfig;
			scaleFactor = (double)frame.Width / 500;
			blackboard.Background = frame;
			savetoprojectbutton.Visible = true;
			blackboard.Drawing = drawing;
			//FIXME: this needs to show a warning message?
			if (App.Current.LicenseLimitationsService.CanExecute (VASFeature.OpenZoom.ToString ())) {
				blackboard.RegionOfInterest = drawing.RegionOfInterest;
			}
			UpdateLineWidth ();
			UpdateTextSize ();
		}

		public void LoadFrame (Image frame)
		{
			drawing = new FrameDrawing ();
			scaleFactor = (double)frame.Width / 500;
			blackboard.Background = frame;
			blackboard.Drawing = drawing;
			savetoprojectbutton.Visible = false;
			UpdateLineWidth ();
			UpdateTextSize ();
		}

		void Bind ()
		{
			ctx = new BindingContext ();
			ctx.Add (zoomscale.Bind (vm => ((DrawingToolVM)vm).SetZoomCommand, App.Current.ZoomLevels.Min (),
									 App.Current.ZoomLevels.Min (), App.Current.ZoomLevels.Max (), ZOOM_STEP, ZOOM_PAGE));
		}

		int ScalledSize (int size)
		{
			return (int)Math.Round (size * scaleFactor);
		}

		int OriginalSize (int size)
		{
			return (int)Math.Round (size / scaleFactor);
		}

		void FillLineStyle ()
		{
			ListStore formatStore;
			CellRendererImage renderer = new CellRendererImage ();
			renderer.Height = TOOL_HEIGHT;

			formatStore = new ListStore (typeof (Image), typeof (LineStyle));
			formatStore.AppendValues (App.Current.ResourcesLocator.LoadImage (Constants.LINE_NORMAL),
				LineStyle.Normal);
			formatStore.AppendValues (App.Current.ResourcesLocator.LoadImage (Constants.LINE_DASHED),
				LineStyle.Dashed);
			stylecombobox.Clear ();
			stylecombobox.PackStart (renderer, true);
			stylecombobox.AddAttribute (renderer, "Image", 0);
			stylecombobox.Model = formatStore;
			stylecombobox.Active = 0;
			stylecombobox.Changed += HandleLineStyleChanged;
		}

		void FillLineType ()
		{
			ListStore formatStore;
			CellRendererImage renderer = new CellRendererImage ();
			renderer.Height = TOOL_HEIGHT;

			formatStore = new ListStore (typeof (Image), typeof (LineStyle));
			formatStore.AppendValues (App.Current.ResourcesLocator.LoadImage (Constants.LINE_NORMAL),
				LineType.Simple);
			formatStore.AppendValues (App.Current.ResourcesLocator.LoadImage (Constants.LINE_ARROW),
				LineType.Arrow);
			formatStore.AppendValues (App.Current.ResourcesLocator.LoadImage (Constants.LINE_DOUBLE_ARROW),
				LineType.DoubleArrow);
			formatStore.AppendValues (App.Current.ResourcesLocator.LoadImage (Constants.LINE_DOT),
				LineType.Dot);
			formatStore.AppendValues (App.Current.ResourcesLocator.LoadImage (Constants.LINE_DOUBLE_DOT),
				LineType.DoubleDot);
			typecombobox.Clear ();
			typecombobox.PackStart (renderer, true);
			typecombobox.AddAttribute (renderer, "Image", 0);
			typecombobox.Model = formatStore;
			typecombobox.Active = 0;
			typecombobox.Changed += HandleLineTypeChanged;
		}

		void UpdateTextSize ()
		{
			if (selectedDrawable is Text) {
				Text t = (selectedDrawable as Text);
				t.TextSize = ScalledSize (textspinbutton.ValueAsInt);
				QueueDraw ();
			} else {
				blackboard.FontSize = ScalledSize (textspinbutton.ValueAsInt);
			}

		}

		void UpdateLineWidth ()
		{
			int width;

			width = ScalledSize (linesizespinbutton.ValueAsInt);
			if (selectedDrawable != null) {
				selectedDrawable.LineWidth = width;
				QueueDraw ();
			} else {
				blackboard.LineWidth = width;
			}
		}

		void EditText (Text text)
		{
			text.Value = MessagesHelpers.QueryMessage (this, Catalog.GetString ("Text"),
				null, text.Value);
			QueueDraw ();
		}

		public abstract void EditPlayer (Text text);

		void HandleLineStyleChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			LineStyle style;

			if (ignoreChanges)
				return;

			stylecombobox.GetActiveIter (out iter);
			style = (LineStyle)stylecombobox.Model.GetValue (iter, 1);
			if (selectedDrawable != null) {
				selectedDrawable.Style = style;
				QueueDraw ();
			} else {
				blackboard.LineStyle = style;
			}
		}

		void HandleLineTypeChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			LineType type;

			if (ignoreChanges)
				return;

			typecombobox.GetActiveIter (out iter);
			type = (LineType)typecombobox.Model.GetValue (iter, 1);
			if (selectedDrawable is Line) {
				(selectedDrawable as Line).Type = type;
				QueueDraw ();
			} else {
				blackboard.LineType = type;
			}
		}

		void Clear ()
		{
			string msg = Catalog.GetString ("Do you want to clear the drawing?");
			if (MessagesHelpers.QuestionMessage (this, msg)) {
				blackboard.Clear ();
			}
		}

		void HandleBackgroundColorSet (object sender, EventArgs e)
		{
			Color c;
			if (ignoreChanges)
				return;


			c = Misc.ToLgmColor (backgroundcolorbutton.Color,
				backgroundcolorbutton.Alpha);
			if (selectedDrawable is Text) {
				Text t = (selectedDrawable as Text);
				t.FillColor = t.StrokeColor = c;
				QueueDraw ();
			} else {
				blackboard.TextBackgroundColor = c;
			}
		}

		void HandleTextColorSet (object sender, EventArgs e)
		{
			if (ignoreChanges)
				return;

			if (selectedDrawable is Text) {
				(selectedDrawable as Text).TextColor = Misc.ToLgmColor (textcolorbutton.Color);
				QueueDraw ();
			} else {
				blackboard.TextColor = Misc.ToLgmColor (textcolorbutton.Color);
			}
		}

		void HandleColorSet (object sender, EventArgs e)
		{
			if (ignoreChanges)
				return;
			if (selectedDrawable != null) {
				selectedDrawable.StrokeColor = Misc.ToLgmColor (colorbutton.Color, colorbutton.Alpha);
				if (selectedDrawable.FillColor != null) {
					Color c = Misc.ToLgmColor (colorbutton.Color, colorbutton.Alpha);
					selectedDrawable.FillColor = c;
				}
				QueueDraw ();
			} else {
				blackboard.Color = Misc.ToLgmColor (colorbutton.Color, colorbutton.Alpha);
			}
		}

		protected override bool OnKeyPressEvent (Gdk.EventKey evnt)
		{
			if (!base.OnKeyPressEvent (evnt) || !(Focus is Entry)) {
				App.Current.KeyContextManager.HandleKeyPressed (App.Current.Keyboard.ParseEvent (evnt));
			}
			return true;
		}

		void HandleToolClicked (object sender, EventArgs e)
		{
			if (sender is RadioButton button) {
				if (!button.Active) {
					return;
				}
				DrawTool tool = buttonToDrawTool [button];
				if (blackboard.Tool != tool) {
					blackboard.Tool = tool;
					UpdateSettingsVisibility (blackboard.Tool);
				}
			}
		}

		void HandleDrawToolChangedEvent (object sender, EventArgs e)
		{
			RadioButton button = buttonToDrawTool.GetKeyByValue (blackboard.Tool);

			if (blackboard.Tool != DrawTool.Selection) {
				selectedDrawable = null;
				blackboard.ClearSelection ();
			}

			if (!button.Active) {
				button.Click ();
			}
		}

		//FIXME: We need to move the logic of this method to the ViewModel
		async Task SaveToFile ()
		{
			string proposed_filename = String.Format ("{0}-{1}.png", App.Current.SoftwareName,
										   DateTime.Now.ToShortDateString ().Replace ('/', '-'));
			string filename = FileChooserHelper.SaveFile (this,
								  Catalog.GetString ("Save File as..."),
								  proposed_filename, App.Current.SnapshotsDir,
								  "PNG Images", new string [] { "*.png" });
			if (filename != null) {
				System.IO.Path.ChangeExtension (filename, ".png");
				blackboard.Save (filename);
				drawing = null;
				await App.Current.StateController.MoveBack ();
			}
		}

		//FIXME: We need to move the logic of this method to the ViewModel
		async Task SaveToProject ()
		{
			drawing.RegionOfInterest = blackboard.RegionOfInterest;
			if (!playVM.Drawings.Contains (drawing)) {
				playVM.Drawings.Add (drawing);
			}
			drawing.Miniature = blackboard.Save ();
			drawing.Miniature.ScaleInplace (Constants.MAX_THUMBNAIL_SIZE,
				Constants.MAX_THUMBNAIL_SIZE);
			playVM.Model.UpdateMiniature ();
			drawing = null;
			ViewModel.DrawingSaved ();
			await App.Current.StateController.MoveBack ();
		}

		void HandleConfigureObjectEvent (IBlackboardObject drawable, DrawTool tool)
		{
			if (drawable is Text) {
				if (tool == DrawTool.Text) {
					EditText (drawable as Text);
				} else if (tool == DrawTool.Player) {
					EditPlayer (drawable as Text);
				}
			}
		}

		void HandleDrawableChangedEvent (IEnumerable<IBlackboardObject> drawables)
		{
			selectedDrawable = (drawables == null || drawables.Count () > 1) ? null : drawables.FirstOrDefault () as Drawable;
			colorbutton.Sensitive = !(selectedDrawable is Text);

			ignoreChanges = true;
			if (selectedDrawable == null) {
				colorbutton.Color = Misc.ToGdkColor (blackboard.Color);
				colorbutton.Alpha = Color.ByteToUShort (blackboard.Color.A);
				textcolorbutton.Color = Misc.ToGdkColor (blackboard.TextColor);
				backgroundcolorbutton.Color = Misc.ToGdkColor (blackboard.TextBackgroundColor);
				backgroundcolorbutton.Alpha = Color.ByteToUShort (blackboard.TextBackgroundColor.A);
				linesizespinbutton.Value = OriginalSize (blackboard.LineWidth);
				if (blackboard.LineStyle == LineStyle.Normal) {
					stylecombobox.Active = 0;
				} else {
					stylecombobox.Active = 1;
				}
				typecombobox.Active = (int)blackboard.LineType;
			} else {
				if (selectedDrawable is Text) {
					textcolorbutton.Color = Misc.ToGdkColor ((selectedDrawable as Text).TextColor);
					backgroundcolorbutton.Color = Misc.ToGdkColor (selectedDrawable.FillColor);
					backgroundcolorbutton.Alpha = Color.ByteToUShort (selectedDrawable.FillColor.A);
					textspinbutton.Value = OriginalSize ((selectedDrawable as Text).TextSize);
				} else {
					colorbutton.Color = Misc.ToGdkColor (selectedDrawable.StrokeColor);
					colorbutton.UseAlpha = true;
					colorbutton.Alpha = Color.ByteToUShort (selectedDrawable.StrokeColor.A);
				}
				if (selectedDrawable is Line) {
					typecombobox.Active = (int)(selectedDrawable as Line).Type;
				}
				linesizespinbutton.Value = OriginalSize (selectedDrawable.LineWidth);
				if (selectedDrawable.Style == LineStyle.Normal) {
					stylecombobox.Active = 0;
				} else {
					stylecombobox.Active = 1;
				}
			}

			UpdateSettingsVisibility (blackboard.Tool);
			ignoreChanges = false;
		}

		void HandleShowMenuEvent (IBlackboardObject drawable)
		{
			Menu m = new Menu ();
			MenuItem item = new MenuItem (Catalog.GetString ("Move to Front"));
			item.Activated += (sender, e) => blackboard.MoveToFront ();
			m.Add (item);
			item = new MenuItem (Catalog.GetString ("Move to Back"));
			item.Activated += (sender, e) => blackboard.MoveToBack ();
			m.Add (item);
			item = new MenuItem (Catalog.GetString ("Delete"));
			item.Activated += (sender, e) => blackboard.DeleteSelection ();
			m.Add (item);
			if (drawable is Text) {
				MenuItem edit = new MenuItem (Catalog.GetString ("Edit"));
				edit.Activated += (sender, e) => EditText (drawable as Text);
				m.Add (edit);
			}
			m.ShowAll ();
			m.Popup ();
		}

		void HandleDeleteEvent (object o, DeleteEventArgs args)
		{
			string msg = Catalog.GetString ("Do you want to close the current drawing?");
			if (MessagesHelpers.QuestionMessage (this, msg)) {
				args.RetVal = false;
			} else {
				args.RetVal = true;
			}
		}

		void HandleRegionOfInterestChanged (object sender, EventArgs e)
		{
			if (blackboard.RegionOfInterest.Empty ||
				(blackboard.RegionOfInterest.Width == blackboard.Background.Width &&
				blackboard.RegionOfInterest.Height == blackboard.Background.Height)) {
				hscrollbar.Visible = false;
				wscrollbar.Visible = false;
			} else {
				hscrollbar.Visible = true;
				wscrollbar.Visible = true;
				hscrollbar.SetRange (0, blackboard.Background.Height -
				blackboard.RegionOfInterest.Height);
				wscrollbar.SetRange (0, blackboard.Background.Width -
				blackboard.RegionOfInterest.Width);
				ignoreChanges = true;
				wscrollbar.Value = blackboard.RegionOfInterest.Start.X;
				hscrollbar.Value = blackboard.RegionOfInterest.Start.Y;
				zoomscale.Value = blackboard.Background.Width / blackboard.RegionOfInterest.Width;
				ignoreChanges = false;
			}
		}

		void HandleScrollValueChanged (object sender, EventArgs e)
		{
			if (ignoreChanges)
				return;

			if (sender == wscrollbar) {
				blackboard.RegionOfInterest.Start.X = wscrollbar.Value;
			} else {
				blackboard.RegionOfInterest.Start.Y = hscrollbar.Value;
			}
			blackboard.RegionOfInterest = blackboard.RegionOfInterest;
		}

		void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (ViewModel.ZoomLevel)) {
				zoomlabel.Text = string.Format ("{0,3}%", (int)(ViewModel.ZoomLevel * 100));
				if (!ignoreChanges) {
					blackboard.ZoomCommand.Execute (zoomscale.Value);
				}
			}
		}

		void CreateToolSettings ()
		{
			toolSettings = new Dictionary<DrawTool, ToolSettingBase> ();

			toolSettings.Add (DrawTool.Selection, null);
			toolSettings.Add (DrawTool.Eraser, null);
			toolSettings.Add (DrawTool.Zoom, null);

			DrawToolSettings complete = new DrawToolSettings { Color = true, Size = true, Style = true, Type = true };
			toolSettings.Add (DrawTool.Line, complete);

			DrawToolSettings drawArea = new DrawToolSettings { Color = true, Size = true, Style = true };
			toolSettings.Add (DrawTool.CircleArea, drawArea);
			toolSettings.Add (DrawTool.RectangleArea, drawArea);
			toolSettings.Add (DrawTool.Cross, drawArea);
			toolSettings.Add (DrawTool.Rectangle, drawArea);
			toolSettings.Add (DrawTool.Ellipse, drawArea);

			DrawToolSettings counter = new DrawToolSettings { Color = true, Size = true };
			counter.TextSettings = new TextToolSettings { Color = true };
			toolSettings.Add (DrawTool.Counter, counter);

			TextToolSettings text = new TextToolSettings { Background = true, Color = true, Size = true };
			toolSettings.Add (DrawTool.Player, text);
			toolSettings.Add (DrawTool.Text, text);

			DrawToolSettings simple = new DrawToolSettings { Color = true, Size = true };
			toolSettings.Add (DrawTool.Pen, simple);
		}

		void UpdateSettingsVisibility (DrawTool tool)
		{
			// Retrieve the proper setting type
			ToolSettingBase settings = null;
			if (tool == DrawTool.Selection) {
				// tool selection use the visible settings of the object selected when there is only one
				settings = GetSettingsForSelectedDrawable ();
			} else {
				settings = toolSettings [tool];
			}

			DrawToolSettings drawingSettings = settings as DrawToolSettings;
			TextToolSettings textSettings = (drawingSettings != null) ? 
				drawingSettings.TextSettings : settings as TextToolSettings;

			// updates frames visibility
			linesframe.Visible = drawingSettings != null;
			textframe.Visible = textSettings != null;

			// update drawing settings visibility
			if (drawingSettings != null) {
				colorslabel.Visible = drawingSettings.Color;
				colorbutton.Visible = drawingSettings.Color;

				label3.Visible = drawingSettings.Size;
				linesizespinbutton.Visible = drawingSettings.Size;

				label4.Visible = drawingSettings.Style;
				stylecombobox.Visible = drawingSettings.Style;

				label5.Visible = drawingSettings.Type;
				typecombobox.Visible = drawingSettings.Type;
			}

			// update text settings visibility
			if (textSettings != null) {
				textcolorslabel2.Visible = textSettings.Color;
				textcolorbutton.Visible = textSettings.Color;

				backgroundcolorslabel2.Visible = textSettings.Background;
				backgroundcolorbutton.Visible = textSettings.Background;

				backgroundcolorslabel3.Visible = textSettings.Size;
				textspinbutton.Visible = textSettings.Size;
			}

			zoombox.Visible = tool == DrawTool.Zoom;
		}

		ToolSettingBase GetSettingsForSelectedDrawable () {
			if (selectedDrawable is Text) {
				return toolSettings [DrawTool.Text];
			} else if (selectedDrawable is Counter) {
				return toolSettings [DrawTool.Counter];
			} else if (selectedDrawable is Line) {
				return toolSettings [DrawTool.Line];
			} else if (selectedDrawable is Rectangle) {
				return toolSettings [DrawTool.CircleArea];
			} else if (selectedDrawable is Cross) {
				return toolSettings [DrawTool.Cross];
			} else {
				return null;
			}
		}
	}

	/// <summary>
	/// Visibilty settings of a drawing tool
	/// </summary>
	class DrawToolSettings : ToolSettingBase
	{
		public bool Style {
			get;
			set;
		}

		public bool Type {
			get;
			set;
		}

		/// <summary>
		/// Visibility Text Settings which is optional depending on the tool
		/// </summary>
		/// <value>The text settings.</value>
		public TextToolSettings TextSettings {
			get;
			set;
		}
	}

	/// <summary>
	/// Text tool settings.
	/// </summary>
	class TextToolSettings : ToolSettingBase
	{
		public bool Background {
			get;
			set;

		}
	}

	/// <summary>
	/// Abstract class for the tool settings
	/// </summary>
	abstract class ToolSettingBase {
		public bool Color {
			get;
			set;
		}

		public bool Size {
			get;
			set;
		}
	}
}
