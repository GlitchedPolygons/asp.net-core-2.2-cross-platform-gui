[![License Shield](https://img.shields.io/badge/license-BSD%203--Clause-brightgreen)](https://github.com/GlitchedPolygons/asp.net-core-2.2-cross-platform-gui/blob/master/LICENSE)
# Create Cross-Platform GUI Applications 
## Use ASP.NET Core 2.2 and host it locally into a frontend like e.g. [Electron](https://github.com/electron/electron)

Why screw around with [Qt](https://www.qt.io) or [JavaFX](https://openjfx.io/) if you can just design your client app 
as if it were a website and bridge it with a locally hosted ASP.NET Core backend?

Not only is it better in terms of [separation of concerns](https://en.wikipedia.org/wiki/Separation_of_concerns)
(because your frontend only receives the data it needs from the backend for presentation), 
you also have the choice of running the app in the user's browser of choice (like [pgAdmin 4](https://www.pgadmin.org/) does)
or serve it in the background while some lightweight web view wrapper app displays the localhost site in its window.
