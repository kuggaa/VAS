MAKE=make
SHELL = /bin/sh
MKDIR_P = /usr/bin/install -d
INSTALL = /usr/bin/install
INSTALL_DATA = ${INSTALL} -m 644
INSTALL_DIR = cp -r

XBUILD=xbuild
MONO=mono
NUNIT_CONSOLE_EXE=packages/NUnit.ConsoleRunner/tools/nunit3-console.exe
NUNIT = $(MONO) --debug $(NUNIT_CONSOLE_EXE)

prefix ?= /usr/local
datarootdir = ${prefix}/share
libdir = ${prefix}/lib
ifeq ($(OSTYPE), OSTYPE_WINDOWS)
  exelibdir = ${prefix}/bin
else
  exelibdir = ${libdir}
endif
itlocaledir = $(prefix)/share/locale
pkglibdir = ${libdir}/${USER_PACKAGE}
datadir = ${datarootdir}/${USER_PACKAGE}

MAKE_ENVS=LIBDIR=$(DESTDIR)$(exelibdir) \
	OUTPUT_DIR=$(OUTPUT_DIR) \
	OSTYPE=$(OSTYPE) \
	WITH_WIBU=$(WITH_WIBU) \
	DEVEL=$(DEVEL) \
	CPU=$(CPU) \
	prefix=$(prefix)

define build
$(XBUILD) /property:DefineConstants="$(XBUILD_CONSTANTS)" \
  /property:BuildType=makefiles \
  /property:BuildProjectReferences=false \
  /property:Platform="Any CPU" \
  /property:Configuration=$(2) $(1)
endef

define install
$(MKDIR_P) $(DESTDIR)$(2)
$(INSTALL) $(wildcard $(1)/bin/*.dll) $(DESTDIR)$(2)
$(INSTALL) $(wildcard $(1)/bin/*.pdb) $(DESTDIR)$(2)
$(INSTALL) $(wildcard $(1)/bin/*.dll.config) $(DESTDIR)$(2)
$(MKDIR_P) $(DESTDIR)$(3)
$(INSTALL_DIR) $(1)/data/theme $(DESTDIR)$(3)
$(INSTALL_DIR) $(1)/data/icons $(DESTDIR)$(3)
$(INSTALL_DIR) $(1)/data/images $(DESTDIR)$(3)
endef
