AC_DEFUN([SHAMROCK_CHECK_NUNIT],
[
	NUNIT_REQUIRED=2.4.7

	AC_ARG_ENABLE(tests, AC_HELP_STRING([--enable-tests], [Enable NUnit tests]),
		enable_tests=$enableval, enable_tests="yes")

	if test "x$enable_tests" = "xno"; then
		AM_CONDITIONAL(ENABLE_TESTS, false)
	else
		NUNIT_CONSOLE_EXE='$(top_builddir)/packages/NUnit.ConsoleRunner/tools/nunit3-console.exe'
		AM_CONDITIONAL(ENABLE_TESTS, true)
	fi
	AC_SUBST(NUNIT_CONSOLE_EXE)
	AC_SUBST(NUNIT_VERSION)
])
