# Building OneDrive Cloud Player

Since v1.2.2, a custom version of the Videolan.LibVLC.UWP nuget is required to work around a critical bug in VLC. This means that the build process is a little more involved than just cloning the project and building it with Visual Studio.

First, download the .nupkg from the releases page on GitHub. Next, open Visual Studio and go to Tools -> Options -> NuGet Package Manager -> Package Sources. In this window, you can add a local folder as package source. Click the '+' button and add the directory where the .nupkg has been downloaded as the source.

Next, clean and build the solution. Visual Studio should pull the downloaded .nupkg from the specified disk location.

From there on, you can choose to deploy the built app packages locally, or sign them with your own certificate following [this guide](https://github.com/TimGels/OneDrive-Cloud-Player/wiki/Installing-the-self-signed-version-of-OneDrive-Cloud-Player).
