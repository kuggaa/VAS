BUILT_SOURCES = $(top_srcdir)/.version

$(top_srcdir)/.version:
	echo $(VERSION) > $@-t && mv $@-t $@

dist-hook:
	echo $(VERSION) > $(distdir)/.tarball-version


ACLOCAL_AMFLAGS = -I build/m4 -I build/m4/shamrock

SUBDIRS = \
	build \
	.nuget \
	VAS.Core \
	VAS.DB \
	po


DISTCLEANFILES = 	intltool-extract\
			intltool-merge\
			intltool-update

