## Dev environment and tools
### Dotnet
You'll be using the dotnet command to build and run this project, so it's essential.  Make sure not to use dotnet 2.x,
because it's so new that it can't build this month old project.  I'm running dotnet 1.1.0 (although dotnet --version
says it's 1.0.3, because dotnet versioning is a goddamn nightmare).

### Yarn
Yarn is a package manager for JavaScript.  By default, Fable 1.0 uses it for package management.  I don't think version
matters here, just use the latest.

### Code + Ionide
This one isn't actually required, but I've put some work into the setup to make using Visual Studio Code and the Ionide
plugin a nice workflow.  If you're using Code and Ionide you get tooltip support, Intellisense, Code Lens, and you can
start the watch server by just pressing Ctrl + Shift + B (after doing package restoration, see following section).

However, if you decide to work without using Code, you can still build by following the directions below.

## Build and running the app

1. Install npm dependencies: `yarn install`
    - fsevents might fail to install.  That's normal and fine.
2. Install dotnet dependencies: `dotnet restore`
3. Start Fable server and Webpack dev server: `dotnet fable npm-run start`
    - This step does have a fairly long startup time, so don't be worried if it seems like it might be hung for a few
      minutes.
4. In your browser, open: [http://localhost:8080/](http://localhost:8080/)

Any modification you do to the F# code will be reflected in the web page after saving.

## Technologies in use
### F#
There's a great F# tutorial available at [F# for fun and profit](https://fsharpforfunandprofit.com/).

### Fable
Fable is the transpiler this project uses to convert F# into JavaScript.  We're using the 1.0 beta, but the
documentation at [the website](http://fable.io/) is still good enough.

### Fable-elmish
Fable-elmish is a front end framework written for Fable and based off [the Elm architecture](https://guide.elm-lang.org/architecture/)
(which honestly doesn't seem important enough to warrant its own name, since it's basically MVC).  I like it because it
lets you define your app view using F# that directly maps to HTML, so you can type check it and shit.  The official
documentation at [their Github](https://fable-elmish.github.io/elmish/) seems nice, but I haven't read it yet.  This
tutorial I found [here](http://inchingforward.com/2017/03/getting-started-with-fable-elmish/) was a decent tutorial,
but they made their Fable setup by scratch and I hydrated mine from a template, so there are some differences (like
their files being .fsx files and mine being .fs files).

## To-do list

- In the active game, don't make car modifications live until the Next Phase button is clicked.
    - Maybe.  There are reasons to modify these stats immediately.
- Everything should probably be collapsible.