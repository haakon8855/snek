# Snek

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-0ba300)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Snek is a game about getting long snek. Try to get a long snek:
[snek-game.azurewebsites.net](https://snek-game.azurewebsites.net)

<img src="snek.png" width=500>

This project was created in December 2023 to learn more about Blazor and its (then) new rendermode,
InteractiveAuto. It is built upon the Blazor template from Microsoft with Microsoft's
Identity library, which includes a working login and account management profile. The game itself
was then added, along with a leaderboard for competitive play.

In December 2024, proper IaC files were written for the project and a GitHub Workflow
was set up to handle continuous deployment of Azure infrastructure. Additionally,
the project was updated to .NET 9 and the code was refactored.

## Additional documentation

- [GitHub Workflows](/documentation/workflows.md)
- [Run project locally](/documentation/getting-started.md)

## Technology

Snek is an ASP.NET Core Blazor web application running with the somewhat new
automatic rendermode introduced in ASP.NET Core 8.

Most of the website is rendered with Static SSR (Server Side Rendering), such as the
login and account pages. However, the game itself is rendered using automatic 
rendermode. This means on the first render, it will use Interactive SSR while
loading the .NET WebAssembly runtime in the background. When this is loaded, it
will switch over to Interactive WebAssembly Rendering and run on the client. This
is needed to ensure low input latency when playing the game. While the game is 
loading and displaying with Interactive SSR, playing is not possible, and it will
simply display a loading screen.

## Anti-Cheat

Because the game is running on the client, cheating is inherently unsolvable.
However, mitigations can be made to make cheating more difficult.

### Initial release

In the first version of this game, a very simple approach was implemented, where
highscores were submitted via an HTTP POST request containing the following body:

```json
{
    "score": x
}
```

While being quite effective for honest players, this approach is extremely easy
to exploit by simply replaying the request with a different score. This way, a
cheater could submit any score as long as it would fit in a C# `int32`.

### Second iteration

After verifying that the logic of the game itself was working and scores were
submittable, work was put in to ensure the validity of highscore requests.

An updated API now included a checksum of the score object. This checksum was
computed in a convoluted manner to avoid hackers guessing the algorithm.

    At this point in time the source code was still closed-source, so hackers
    would need to decompile the WebAssembly binary to discover its secrets.
    Due to the way ASP.NET changed its WebAssembly compiler in .NET 8, a
    decompiler was not yet available as of december 2023.

The checksum was first padded with a static string, then sent through the SHA1
algorithm, before sending this hash through SHA256. While being trivial to
compute with the source code available, this was not as easy when this update
was pushed. The POST request now has a body with the following format:

```json
{
    "score": x,
    "checksum": "a94a8fe5ccb19ba61c4c0873d391e987982fbbd3"
}
```

This anti-cheat measure was defeated using the [Cetus] tool for hacking
WebAssembly games by changing the score value in the game's memory before the
game finishes, thus bypassing the need to manually create the checksum, as the
checksum will be calculated by the game automatically. The Cetus extension works
in a very similar way to how [CheatEngine](https://www.cheatengine.org/) works
for desktop games.

### Third iteration

To attempt mitigation of the memory-exploit, server-side replay validation was
added to the highscore-endpoint. From this API-version onwards, a replay file
is required in all submitted runs. The POST request now has a body with the
following format:

```json
{
    "replay": {
        "score": 1,
        "seed": 1343454789,
        "inputs": {
            "0": 3,
            "5": 2,
            "13": 3,
            "14": 4,
            "23": 1
        }
    },
    "checksum":"6XMHqS1TLFIM6iYKw2SiPMTlO0CWDKsJi+x+LIct4DE="
}
```

When a run is submitted through the score-endpoint, the server sets up a game
with the random seed provided by the replay. The server then plays this game
with the inputs defined in the "input"-section of the replay. When the game is
over, the server compares the score it got, with the score reported in the
replay sent by the client. If, and only if, these scores match, the score is
saved in the database.

### Future

It is still quite possible to cheat in this game, especially now that the code
is open-source. In the future we would like to add the following anti-cheat
measures to mitigate further cheating:

- Server-side seed generation
  - Generating the seed on the server ensures that the user is not able to pick
    their own seed. This mitigates cheating methods such as hand-crafting a set
    of inputs to find the optimal strategy for a specific seed.
- Game duration cheat detection
  - Verifying that the game has a duration with a lower and upper limit can
    easily be done by requiring the client to send a request notifying the
    server that a game has been started.
    - A game should have a duration of at least the number of ticks for that
      game multiplied by the length of one tick (75ms)
    - A game should at most have a duration just a little bit higher than the
      minimum duration, to account for network connections with high ping and
      other unknown factors that might delay the submission of a non-cheated
      run.
- Bot detection
  - Bot detection could be possible by detecting repeated patterns that bots
    might utilise. Of course, this strategy is very hard to conclusively
    classify runs to be played by bots or humans. This strategy would be more of
    an indicator for admins to investigate suspicious runs.

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
