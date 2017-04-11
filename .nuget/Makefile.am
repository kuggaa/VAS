NuGet.exe:
	curl -k -L -o "NuGet.exe" "http://nuget.org/nuget.exe"

restore: NuGet.exe
	mono NuGet.exe install ../VAS.Core/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install ../VAS.DB/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install ../Weavers/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install ../VAS.Tests/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install ../VAS.Services/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../

all: restore

.PHONY: restore
