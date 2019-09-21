# Yup
Say Hello to `Yup` a side project that is proof of concept that there can be good tooling for developers in UWP

Yup is a proof of concept of a mongodb database manager with the most basic stuff in place
Storing multiple connections
![databases](./databases.png)

and doing simple queries
![Query](./query.png)


this also leverages the Monaco Editor to be able to do your queries
when you have a collection selected you actually send a [MongoDB Database Command](https://docs.mongodb.com/manual/reference/command/)
that includes of course [CRUD Command Operations](https://docs.mongodb.com/manual/reference/command/#query-and-write-operation-commands).

Right now it only supports find queries (haven't really tested write operations, but theoretically you should be able to send any valid MongoDB command though showing the response is not something I've worked on).

This uses [WinUI](https://github.com/Microsoft/microsoft-ui-xaml) as well as the [Windows Community Toolkit](https://github.com/windows-toolkit/WindowsCommunityToolkit)


# Thoughts

- Working with the WinRT API is nice I like it a lot.
- It's UWP! meaning you could be managing databases from almost any form factor (haha xbox... lol).
- I Seriously See UWP as a better alternative to Electron based solutions that only target windows.
- While it's a nice yo have, I don't believe you ***MUST*** design your UWP apps with touchscreens in mind plus WinUI is improving it's base controls (boxes and such).
- TreeView Control is plaged with bugs and tends to crash my app randomly on recent releases of WinUI.


# Think it, design it, give UWP a shot
As a javascript developer myself I swear I don't want to do Electron/Javascript all the time
if you have a nice idea please consider UWP before doing Electron, it will be nice on your resources and your pc!