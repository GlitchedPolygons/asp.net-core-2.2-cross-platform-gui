[![License Shield](https://img.shields.io/badge/license-BSD%203--Clause-brightgreen?style=flat-square)](https://github.com/GlitchedPolygons/asp.net-core-2.2-cross-platform-gui/blob/master/LICENSE)

# Create Cross-Platform GUI Applications 
## Use [ASP.NET Core 2.2](https://dotnet.microsoft.com/) and host it locally into a frontend like e.g. [Electron](https://github.com/electron/electron)

Why screw around with [Qt](https://www.qt.io) or [JavaFX](https://openjfx.io/) if you can just design your client app 
as if it were a website and bridge it with a locally hosted ASP.NET Core C# backend?

Not only is it better in terms of [separation of concerns](https://en.wikipedia.org/wiki/Separation_of_concerns)
(because your frontend only receives the data it needs from the backend for presentation), 
you also have the choice of running the app in the user's browser of choice (like [pgAdmin 4](https://www.pgadmin.org/) does)
or serve it in the background while some lightweight web view wrapper app displays the localhost site in its window.

When you run the application, the included [mkcert](https://github.com/FiloSottile/mkcert) build will automatically deploy HTTPS certificates to the `wwwroot/ssl` folder on startup (check out the [`Program`](https://github.com/GlitchedPolygons/asp.net-core-2.2-cross-platform-gui/blob/master/src/Program.cs) entry point class for more information). When opened in the browser, HTTPS should work _out-of-the-box_. When running in the background, depending on what frontend you use you might need to do some adjustments to get the SSL certificates to work.

### To do:
* Once you forked the repo, use the IDE of your choice to rename the project to your desired application name.
* * Don't forget to also rename the namespace to match your new name. 
* * * [Rider](https://www.jetbrains.com/rider/) or [ReSharper](https://www.jetbrains.com/resharper/) are pretty good at this...
* Open [`Program.cs`](https://github.com/GlitchedPolygons/asp.net-core-2.2-cross-platform-gui/blob/master/src/Program.cs) and decide if the default setup with the `#define`s at the top of the file suits your needs.
* Localization is provided using the [default ASP.NET Core approach](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-2.2).
* * To localize your views, just add the corresponding `Views.ControllerName.ActionName.CULTURE_SPECIFIER.resx` file to `Resources/`.
* * * Culture specifiers are usually two lowercase letters; e.g. `de`, `it`, etc...
* * Update the supported cultures array inside [`Startup.cs`](https://github.com/GlitchedPolygons/asp.net-core-2.2-cross-platform-gui/blob/master/src/Startup.cs) to reflect the languages you want to support.
* * You can also use the [`Layout`](https://github.com/GlitchedPolygons/asp.net-core-2.2-cross-platform-gui/blob/master/src/Models/Dummies/Layout.cs) dummy class to localize strings generally (inside the corresponding `Views.Shared._Layout.CULTURE_SPECIFIER.resx` file). In this case, `@inject IStringLocalizer<Layout>` into your `cshtml` views.
* Document your code using appropriate xml docs.
