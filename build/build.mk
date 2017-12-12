OUTPUT_DIR ?= bin/
ASSEMBLY_FILE ?= $(OUTPUT_DIR)$(ASSEMBLY)

XBUILD_CMD = $(XBUILD) \
 /property:DefineConstants="$(XBUILD_CONSTANTS)" \
 /property:BuildType=makefiles \
 /property:BuildProjectReferences=false \
 /property:Configuration=Release \
 $(PROJECT_FILE) $(XBUILD_EXTRA_FLAGS)

OUTPUT_FILES ?= \
	$(ASSEMBLY_FILE) \
	$(ASSEMBLY_FILE).mdb

.PHONY: $(ASSEMBLY_FILE)

all-local: $(ASSEMBLY_FILE)

$(ASSEMBLY_FILE):
	$(AM_V_GEN) $(XBUILD_CMD)

clean-local:
	$(AM_V_GEN) $(XBUILD_CMD) /t:Clean
	rm -rf obj/

CLEANFILES = $(OUTPUT_FILES)
