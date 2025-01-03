# Snek

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-0ba300)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Snek is a game about getting long snek. Try to get a long snek:
[snek-game.azurewebsites.net](https://snek-game.azurewebsites.net)

This project was created in December 2023 to learn more about Blazor and its (then) new rendermode,
InteractiveAuto. It is built upon the Blazor template from Microsoft with Microsoft's
Identity library, which includes a working login and account management profile. The game itself
was then added, along with a leaderboard for competitive play.

In December 2024, proper IaC files were written for the project and a GitHub Workflow
was set up to handle continuous deployment of Azure infrastructure. Additionally,
the project was updated to .NET 9 and the code was refactored.

## Technology

Snek is an ASP.NET Core Blazor web application running with the somewhat new
automatic rendermode introduced in ASP.NET Core 8.

Most of the website is rendered with Static SSR (Server Side Rendering), such as the
login and account pages. However, the game itself is rendered using automatic 
rendermode. This means on the first render, it will use Interactive SSR while
loading the .NET webassembly runtime in the background. When this is loaded, it
will switch over to Interactive Webassembly Rendering and run on the client. This
is needed to ensure low input latency when playing the game. While the game is 
loading and displaying with Interactive SSR, playing is not possible, and it will
simply display a loading screen.

## Deployment

The pipelines/workflows used in this repository are documented in [Workflows](documentation/workflows.md).

There are two workflows configured in this GitHub repository for continuous deployment.
One workflow for deploying the Blazor web application to the App Service in Azure,
and one workflow for deploying the necessary resources to Azure using the defined
bicep IaC files.

## Contributors

- [haakon8855](https://github.com/haakon8855)
- [NilsOlavKvelvaneJohansen](https://github.com/NilsOlavKvelvaneJohansen)

## License

This code is protected under the [MIT License](license).
