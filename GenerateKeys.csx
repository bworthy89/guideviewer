#!/usr/bin/env dotnet-script
#r "GuideViewer.Core/bin/Debug/net8.0/GuideViewer.Core.dll"
#r "GuideViewer.Data/bin/Debug/net8.0/GuideViewer.Data.dll"

using GuideViewer.Core.Utilities;

// Generate 5 admin and 5 technician keys
ProductKeyGenerator.PrintKeys(adminCount: 5, techCount: 5);
