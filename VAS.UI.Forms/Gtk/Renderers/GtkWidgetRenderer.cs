//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using Gtk;
using VAS.UI.Forms.Components;
using VAS.UI.Forms.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

[assembly: ExportRenderer (typeof (GtkWidget), typeof (GtkWidgetRenderer))]
namespace VAS.UI.Forms.Renderers
{
	public class GtkWidgetRenderer : ViewRenderer <GtkWidget, Widget>
	{
		protected override void OnElementChanged (ElementChangedEventArgs<GtkWidget> e)
		{
			if (e.NewElement != null) {
				if (Control == null) {
					var widget = (Widget)Activator.CreateInstance (e.NewElement.WidgetType);
					e.NewElement.NativeWidget = widget;
					SetNativeControl (widget);
				}
			}

			base.OnElementChanged (e);
		}
	}
}
