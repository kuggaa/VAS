BUILT_SOURCES = $(top_srcdir)/.version

$(top_srcdir)/.version:
	echo $(VERSION) > $@-t && mv $@-t $@

dist-hook:
	echo $(VERSION) > $(distdir)/.tarball-version


ACLOCAL_AMFLAGS = -I build/m4 -I build/m4/shamrock

SUBDIRS = \
	build \
	.nuget \
	Weavers \
	VAS.Core \
	VAS.DB \
	VAS.Drawing \
	VAS.UI.Gtk2 \
	VAS.UI.Helpers.Gtk2 \
	VAS.UI.Multimedia.Gtk2 \
	VAS.Drawing.Cairo \
	VAS.Multimedia \
	VAS.Services \
	libvas \
	system_uuid \
	po

if ENABLE_TESTS
SUBDIRS += VAS.Tests
endif

DISTCLEANFILES = 	intltool-extract\
			intltool-merge\
			intltool-update

