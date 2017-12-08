//
//  Copyright (C) 2018 
//
//
using System;
using VAS.Core.Common;
using VAS.Core.Handlers.Drawing;
using VAS.Core.Interfaces.Drawing;

namespace VAS.Services
{
	/// <summary>
	/// Preview widget used only by the preview service to specify the size that the canvas must respect for the preview
	/// </summary>
	/// <remarks>Only the size notification is implemented from the IWidget</remarks>
	internal class PreviewWidget : IWidget
	{
		public event DrawingHandler DrawEvent;
		public event ButtonPressedHandler ButtonPressEvent;
		public event ButtonReleasedHandler ButtonReleasedEvent;
		public event MotionHandler MotionEvent;
		public event SizeChangedHandler SizeChangedEvent;
		public event ShowTooltipHandler ShowTooltipEvent;

		double width, height;

		public void Dispose ()
		{
		}

		public uint MoveWaitMS { get; set; }

		public double Width { 
			get => width;
			set {
				width = value;
				SizeChangedEvent?.Invoke ();
			}
		}

		public double Height { 
			get => height; 
			set {
				height = value;
				SizeChangedEvent?.Invoke ();
			} 
		}

		public void ReDraw (Area area = null)
		{
		}

		public void ReDraw (IMovableObject drawable)
		{
		}

		public void SetCursor (CursorType type)
		{
		}

		public void SetCursorForTool (DrawTool tool)
		{
		}

		public void ShowTooltip (string text)
		{
		}
	}
}
