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

dnl Mono and C# compiler
dnl Check first for a 4.0 compiler or than fallback to 2.0
SHAMROCK_CHECK_MONO_MODULE(2.4.0)
PKG_CHECK_EXISTS([mono >= 2.8],
		[SHAMROCK_FIND_MONO_4_0_COMPILER
		SHAMROCK_CHECK_MONO_4_0_GAC_ASSEMBLIES([
			System.Data
			Mono.Cairo
			Mono.Posix
		])]
,
		[SHAMROCK_FIND_MONO_2_0_COMPILER
		SHAMROCK_CHECK_MONO_2_0_GAC_ASSEMBLIES([
			System.Data
			Mono.Cairo
			Mono.Posix
		])])
AC_SUBST(MCS, ["$MCS"])
SHAMROCK_FIND_MONO_RUNTIME

dnl Check for xbuild
AC_PATH_PROG(XBUILD, xbuild)

dnl Moq (optional) disable test if not found
PKG_CHECK_MODULES(MOQ, moq,
	do_tests="yes", do_tests="no")
AC_SUBST(MOQ_LIBS)
AM_CONDITIONAL(ENABLE_TESTS, test "x$do_tests" = "xyes")
if test "x$do_tests" = "xno"; then
	AC_MSG_WARN([Could not find moq: tests will not be available])
fi

AC_MSG_CHECKING([for the OS type])
ostype=""

XBUILD_CONSTANTS=HAVE_GTK

case "$host" in
  *-*-mingw*|*-*-cygwin*)
    ostype=Windows
    AC_DEFINE(OSTYPE_WINDOWS)
    XBUILD_CONSTANTS="$XBUILD_CONSTANTS OSTYPE_WINDOWS"
    ;;
  *-*-darwin*)
    AC_DEFINE(OSTYPE_OS_X)
    ostype=OS_X
    XBUILD_CONSTANTS="$XBUILD_CONSTANTS OSTYPE_OS_X"
    ;;
  *-*-linux*)
    AC_DEFINE(OSTYPE_LINUX)
    XBUILD_CONSTANTS="$XBUILD_CONSTANTS OSTYPE_LINUX"
    ostype=Linux
    ;;
esac
AM_CONDITIONAL(OSTYPE_WINDOWS, test x$ostype = xWindows)
AM_CONDITIONAL(OSTYPE_OS_X, test x$ostype = xOS_X)
AM_CONDITIONAL(OSTYPE_LINUX, test x$ostype = xLinux)
AC_SUBST(XBUILD_CONSTANTS, ["$XBUILD_CONSTANTS"])
AC_MSG_RESULT($ostype)

AC_CONFIG_FILES([
Makefile

build/Makefile
build/m4/Makefile

po/Makefile.in

AssemblyInfo/AssemblyInfo.cs
VAS.DB/Makefile
VAS.Core/Makefile


])

AC_OUTPUT