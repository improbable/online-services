# Improbable Online Services

[![Build Status](https://badge.buildkite.com/4b2e4663ffac60c80d6c1e6b1d296b46155533a904ede73b0b.svg)](https://buildkite.com/improbable/online-services-premerge)

## Formatting

This repo uses [dotnet-format](https://github.com/dotnet/format) to maintain a consistent style.

Install the command using `dotnet tool install -g dotnet-format`

Run the formatter against the C# projects in this repo using `dotnet format -w services/csharp` from the repo root.

### Rider
Navigate to "Preferences | Editor | Inspection Settings" and enable "Read settings from editorconfig and project settings."

Rider will now use these settings to format your code.

### Visual Studio 2019
Visual Studio 2019 honours the .editorconfig settings automatically so no changes are required.

### VS Code
The [EditorConfig](https://marketplace.visualstudio.com/items?itemName=EditorConfig.EditorConfig) plugin is required to honour the .editorconfig settings.

