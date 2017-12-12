MAKE=make
SHELL = /bin/sh
MKDIR_P = /usr/bin/install -d
INSTALL = /usr/bin/install
INSTALL_DATA = ${INSTALL} -m 644
INSTALL_DIR = cp -r

XBUILD=xbuild
MONO=mono
NUNIT_CONSOLE_EXE=packages/NUnit.ConsoleRunner/tools/nunit3-console.exe

prefix ?= /usr/local
datarootdir = ${prefix}/share
libdir = ${prefix}/lib
itlocaledir = $(prefix)/share/locale
pkglibdir = ${libdir}/${USER_PACKAGE}
datadir = ${datarootdir}/${USER_PACKAGE}
