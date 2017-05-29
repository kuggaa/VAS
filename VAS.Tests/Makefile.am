OJECT_FILE = VAS.Tests.csproj
ASSEMBLY= VAS.Tests.dll

include $(srcdir)/../build/build.mk

XBUILD_CMD += /property:SolutionDir=$(abs_top_builddir)

if ENABLE_TESTS
check:
	$(MONO) --debug $(NUNIT_CONSOLE_EXE) $(ASSEMBLY_FILE)

if HAVE_COVEM
do-coverage:
	@rm -rf $(ASSEMBLY).covcfg.*
	BABOON_CFG=$(ASSEMBLY).covcfg $(COVEM) $(NUNIT_CONSOLE_EXE) $(ASSEMBLY_FILE) || true

print-coverage:
	@python print_coverage.py $(ASSEMBLY).covcfg.covreport

render-coverage:
	cov-html $(ASSEMBLY).covcfg.covdb vas

show-coverage:
	$(COV_GTK) $(ASSEMBLY).covcfg.covdb

coverage:do-coverage print-coverage
endif
endif

CLEANFILES += \
	VAS.Tests.dll.covcfg.* \
	TestsResult.xml

