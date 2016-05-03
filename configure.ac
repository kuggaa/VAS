dnl Warning: This is an automatically generated file, do not edit!
dnl Process this file with autoconf to produce a configure script.
AC_PREREQ([2.54])
AC_INIT([VAS],[0.2],
	[contact@fluendo.com])
AM_INIT_AUTOMAKE([1.11 foreign])
AM_SILENT_RULES([yes])
AM_MAINTAINER_MODE([enable])

AC_CONFIG_MACRO_DIR([build/m4])
AC_SUBST([ACLOCAL_AMFLAGS], ["-I build/m4/shamrock \${ACLOCAL_FLAGS}"])

AC_PROG_CC
AC_PROG_OBJC
AM_PROG_LIBTOOL

dnl pkg-config
AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
        AC_MSG_ERROR([You need to install pkg-config])
fi

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_EXPAND_BINDIR
SHAMROCK_EXPAND_DATADIR

AC_PROG_INSTALL

ASSEMBLY_VERSION=`echo $PACKAGE_VERSION |cut -d'-' -f1`
AC_SUBST(ASSEMBLY_VERSION)

#*******************************************************************************
# Internationalization
#*******************************************************************************
GETTEXT_PACKAGE=vas
AC_SUBST(GETTEXT_PACKAGE)
AC_DEFINE_UNQUOTED(GETTEXT_PACKAGE,"$GETTEXT_PACKAGE", [GETTEXT package name])

dnl Check for gettext utils
AC_PATH_PROG(MSGFMT, msgfmt, no)
if test "x$MSGFMT" = "xno"; then
	AC_MSG_ERROR([gettext not found])
else 
	AC_SUBST(MSGFMT,[msgfmt])
fi

IT_PROG_INTLTOOL([0.40.0])
AM_GLIB_GNU_GETTEXT

#*******************************************************************************
# END Internationalization
#*******************************************************************************

AC_CONFIG_FILES([
Makefile
po/Makefile.in
])

AC_OUTPUT