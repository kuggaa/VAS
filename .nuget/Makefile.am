NuGet.exe:
	python -c "from urllib import urlretrieve; urlretrieve('http://nuget.org/nuget.exe', 'NuGet.exe')"

restore: NuGet.exe
	mono NuGet.exe install ../VAS.Core/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget;${CERBERO_PREFIX}/share/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install ../VAS.DB/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget;${CERBERO_PREFIX}/share/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install ../Weavers/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget;${CERBERO_PREFIX}/share/nuget" -NonInteractive -solutionDir ../

all: restore

.PHONY: restore
