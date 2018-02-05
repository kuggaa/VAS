include ../build/cbuild.mk

BVWMARSHALFILES = baconvideowidget-marshal.c
GLIB_GENMARSHAL=`pkg-config --variable=glib_genmarshal glib-2.0`

baconvideowidget-marshal.h: baconvideowidget-marshal.list
	( $(GLIB_GENMARSHAL) --prefix=baconvideowidget_marshal baconvideowidget-marshal.list --header > baconvideowidget-marshal.h )
baconvideowidget-marshal.c: baconvideowidget-marshal.h
	( $(GLIB_GENMARSHAL) --prefix=baconvideowidget_marshal baconvideowidget-marshal.list --body --header > baconvideowidget-marshal.c )

libvas_SOURCES = \
	$(BVWMARSHALFILES) \
	lgm-gtk-glue.c\
	lgm-video-player.c\
	lgm-device.c\
	gstscreenshot.c \
	gst-camera-capturer.c\
	gst-remuxer.c\
	gst-video-editor.c\
	gst-nle-source.c\
	lgm-utils.c

libvas_PKGCONFIG_DEPS = gtk+-2.0 \
	gdk-2.0 \
	gio-2.0 \
	glib-2.0 \
	gobject-2.0 \
	gstreamer-0.10 \
	gstreamer-audio-0.10 \
	gstreamer-video-0.10 \
	gstreamer-pbutils-0.10 \
	gstreamer-interfaces-0.10 \
	gstreamer-tag-0.10 \
	gstreamer-app-0.10

libvas_OBJS = $(libvas_SOURCES:.c=.o)
libvas_DEPS = $(libvas_OBJS:.o=.d)
libvas_LDFLAGS = $(shell pkg-config --libs $(libvas_PKGCONFIG_DEPS))
libvas_CFLAGS = $(shell pkg-config --cflags $(libvas_PKGCONFIG_DEPS))
CFLAGS += $(libvas_CFLAGS)
LDFLAGS += $(libvas_LDFLAGS)
LIBVAS=$(OUTPUT_DIR)libvas$(SHARED_EXT)

-include $(libvas_DEPS)   # include all dep files in the makefile
$(LIBVAS): $(libvas_OBJS)
	install -d $(OUTPUT_DIR) 2>&1 | true
	${LINK}

TARGETS = $(LIBVAS)
BUILTSOURCES = $(libvas_OBJS) $(libvas_DEPS) $(TARGETS)

.PHONY: clean
clean:
	rm -f $(BUILTSOURCES)

install:
	install -d $(LIBDIR) 2>&1 | true
	install $(LIBVAS) $(LIBDIR)

all: $(TARGETS)
.DEFAULT_GOAL := all
