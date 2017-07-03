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
using Gtk;
using Gdk;
using System.Runtime.InteropServices;

namespace VAS
{
	public static class GtkGlue
	{
		const string LIBGDK = "libgdk-2.0.dll";
		const string LIBGTK = "libgtk-2.0.dll";
		const string LIBPANGO = "libpango-1.0.dll";
		const string LIBGDK_PIXBUF = "libgdk_pixbuf-2.0.dll";
		const string LIBVAS = "libvas.dll";

		[DllImport (LIBVAS)]
		static extern void lgm_gtk_glue_gdk_event_button_set_button (IntPtr evt, uint button);

		[DllImport (LIBPANGO)]
		static extern void pango_layout_set_height (IntPtr layout, int height);

		[DllImport (LIBGTK)]
		static extern void gtk_menu_item_set_label (IntPtr menu, IntPtr label);

		[DllImport (LIBGTK)]
		static extern IntPtr gtk_message_dialog_get_message_area (IntPtr dialog);

		[DllImport (LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern double gtk_widget_get_scale_factor (IntPtr widget);

		[DllImport (LIBGDK, CallingConvention = CallingConvention.Cdecl)]
		static extern double gdk_screen_get_monitor_scale_factor (IntPtr widget, int monitor);

		[DllImport (LIBGDK_PIXBUF, CallingConvention = CallingConvention.Cdecl)]
		static extern void gdk_pixbuf_set_hires_variant (IntPtr px, IntPtr variant2x);

		[DllImport (LIBGDK_PIXBUF, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gdk_pixbuf_get_hires_variant (IntPtr px);

		/// <summary>
		/// Gets the high resolution @2x variant of the pixbuf
		/// </summary>
		/// <returns>The high resolution variant.</returns>
		/// <param name="px">The Pixbuf.</param>
		public static Pixbuf GetHiResVariant (this Pixbuf px)
		{
			return new Pixbuf (gdk_pixbuf_get_hires_variant (px.Handle));
		}

		/// <summary>
		/// Sets the high resolution @2x variant of the pixbuf
		/// </summary>
		/// <returns>The high resolution variant.</returns>
		/// <param name="px">Px.</param>
		public static void SetHiResVariant (this Pixbuf px, Pixbuf variant2x)
		{
			gdk_pixbuf_set_hires_variant (px.Handle, variant2x.Handle);
		}

		/// <summary>
		/// Gets the current scale factor of the widget.
		/// </summary>
		/// <returns>The scale factor.</returns>
		/// <param name="widget">The widget.</param>
		public static double GetScaleFactor (this Widget widget)
		{
			return gtk_widget_get_scale_factor (widget.Handle);
		}

		/// <summary>
		/// Gets the scale factor of the monitor.
		/// </summary>
		/// <returns>The scale factor.</returns>
		/// <param name="screen">Screen.</param>
		/// <param name="monitor">Monitor.</param>
		public static double GetScaleFactor (this Screen screen, int monitor)
		{
			return gdk_screen_get_monitor_scale_factor (screen.Handle, monitor);
		}

		public static void SetLabel (this MenuItem menu, string label)
		{
			gtk_menu_item_set_label (menu.Handle, GLib.Marshaller.StringToFilenamePtr (label));
		}

		public static VBox MessageDialogGetMessageArea (this MessageDialog dialog)
		{
			IntPtr handle = gtk_message_dialog_get_message_area (dialog.Handle);

			return new VBox (handle);
		}

		public static void SetButton (this EventButton ev, uint button)
		{
			lgm_gtk_glue_gdk_event_button_set_button (ev.Handle, button);
		}

		/// <summary>
		/// pango_layout_set_height (SetPangoLayoutHeight) has 2 different behaviors:
		/// If height is positive, it'll the layout max height (and at least one line).
		/// If height is negative, it'll the max number of lines per paragraph,
		/// if height is -1, first line of each paragraph is ellipsized.
		/// More info: https://developer.gnome.org/pango/stable/pango-Layout-Objects.html#pango-layout-set-height
		/// </summary>
		/// <param name="layout">Layout.</param>
		/// <param name="height">Height in Pango Units.</param>
		public static void SetPangoLayoutHeight (this Pango.Layout layout, int height)
		{
			pango_layout_set_height (layout.Handle, height);
		}

		/// <summary>
		/// Sets the link handler for a given GtkLabel.
		/// </summary>
		/// <param name="label">GtkLabel to set the handler for.</param>
		/// <param name="urlHandler">URL handler.</param>
		public static void SetLinkHandler (this Label label, Action<string> urlHandler)
		{
			new UrlHandlerClosure (urlHandler).ConnectTo (label);
		}

		class UrlHandlerClosure
		{
			Action<string> urlHandler;

			public UrlHandlerClosure (Action<string> urlHandler)
			{
				this.urlHandler = urlHandler;
			}

			[GLib.ConnectBefore]
			void HandleLink (object sender, ActivateLinkEventArgs args)
			{
				urlHandler (args.Url);
				args.RetVal = true;
			}

			public void ConnectTo (Gtk.Label label)
			{
				var signal = GLib.Signal.Lookup (label, "activate-link", typeof (ActivateLinkEventArgs));
				signal.AddDelegate (new EventHandler<ActivateLinkEventArgs> (HandleLink));
			}

			class ActivateLinkEventArgs : GLib.SignalArgs
			{
				public string Url { get { return (string)base.Args [0]; } }
			}
		}
	}
}

