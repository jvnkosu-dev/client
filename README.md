# jvnkosu! client

A free-to-win rhythm game based on osu!(lazer). Click is just a *rhythm* away!

## Disclaimer

*osu!* is a registered trademark of ppy Pty Ltd.  
jvnkosu! is not affiliated with, or endorsed by ppy Pty Ltd., but makes use of its open-source components and resources.

## License
Client source code is licensed under the MIT license, see the [LICENCE](LICENCE) file in repository root for more info.

Game assets are included as a NuGet package and licensed under the CC BY-NC 4.0, which prohibits commercial use. See [ppy/osu-resources](https://github.com/ppy/osu-resources) for more info.

Registered trademarks "osu!" and "ppy" are property of ppy Pty Ltd., and protected by trademark law.

## Compiling from source
Building jvnkosu! from source is pretty much possible (and welcome here).  

First, you must have a desktop platform with [.NET Core SDK 8](https://dotnet.microsoft.com/download) installed. Windows, Linux, macOS should work well. You can check if you have the SDK installed by running `dotnet --version` in your command prompt/terminal.  

Then, download the source code. You may download it as an archive and unzip it, but using [Git](https://git-scm.com/) instead is recommended:
```
git clone https://gitea.jvnko.boats/jvnkosu/client
```


To **run** the project, switch to project's directory and run the following:
```
dotnet run --project osu.Desktop
```

To **compile**:
```
dotnet build osu.Desktop
```

To reduce performance overhead in custom builds, it's recommended to build with the `-c Release` flag, that will use the release profile and remove possibly unneeded debugging code.

### See the [original readme](README.original.md) for more info.