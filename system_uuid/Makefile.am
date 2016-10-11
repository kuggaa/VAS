## Process this file with automake to produce Makefile.in

AM_CPPFLAGS = \
	-DPACKAGE_SRC_DIR=\""$(srcdir)"\" \
	-DPACKAGE_DATA_DIR=\""$(datadir)"\" \
	$(CESARPLAYER_CFLAGS)

AM_CFLAGS =\
	 -Wall\
	 -g

if OSTYPE_LINUX

bin_PROGRAMS = vas-system-uuid
vas_system_uuid_SOURCES = vas-system-uuid.c

install-exec-hook:
	chmod 4755 $(DESTDIR)$(bindir)/vas-system-uuid

endif
