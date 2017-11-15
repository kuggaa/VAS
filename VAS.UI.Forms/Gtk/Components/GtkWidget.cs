//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using Gtk;
using Xamarin.Forms;
using FormsView = Xamarin.Forms.View;

namespace VAS.UI.Forms.Components
{
	public class GtkWidget : FormsView
	{
		public static readonly BindableProperty WidgetTypeProperty = BindableProperty.Create (
			propertyName: "WidgetType", returnType: typeof (string), declaringType: typeof (GtkWidget), defaultValue: null,
			defaultBindingMode: BindingMode.OneWay);

		public Type WidgetType {
			get { return Type.GetType((string)GetValue (WidgetTypeProperty)); }
		}
		Widget nativeWidget;

		public Widget NativeWidget {
			get {
				return nativeWidget;
			}

			set {
				nativeWidget = value;
				this.OnPropertyChanged (nameof (NativeWidget));
			}
		}
	}
}
