ifeq ($(OSTYPE), OSTYPE_OS_X)
    CFLAGS += -x objective-c -DOSTYPE_OS_X
    SHARED_EXT = .dylib
endif
ifeq ($(OSTYPE), OSTYPE_WINDOWS)
    CFLAGS += -x c -DOSTYPE_WINDOWS
    LDFLAGS += -no-undefined
    SHARED_EXT = .dll
endif
ifeq ($(OSTYPE), OSTYPE_LINUX)
    CFLAGS += -x c -DOSTYPE_LINUX
    SHARED_EXT = .so
endif

LINK=$(CC) -shared -o $@ $^ $(LDFLAGS) $(CFLAGS)

%.o: %.c
	$(CC) $(CFLAGS) -o $@ -c $<

%.d: %.c
	@$(CPP) $(CFLAGS) $< -MM -MT $(@:.d=.o) >$@
