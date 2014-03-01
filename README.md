GeekStream
==========
Geekstream is an example application built to demonstrate OrigoDB  - http://github.com/devrexlabs/origodb
A live example can be found at http://geekstream.devrexlabs.com


The online example runs on a virtual Windows 2008 with IIS and a standalone OrigoDB Server (http://origodb.com) hosting the database. The web application passes queries and commands to the db server using a request/response pattern over a tcp/ip connection.

What to check out
------------
Start by having a look at the domain classes, commands, queries and views in the GeekStream.Core project. The database is an instance of GeekStreamModel hosted either in-process or in a standalone OrigoDB Server process. 

To see how to setup an OrigoDB client connection see global.asax.cs and the connection string in web.config.

Have a look at the controllers in the web application to see the client, proxy, commands, queries and views in action.


Try it out
-------------------------
You can easily run geekstream locally. By default the connection string in web.config points to an embedded engine. This will load an in-process db from journal files in the App_Data folder.

Hosting 
--------------------
If you're feeling brave you can set up a standalone server. Get a trial version of OrigoDB server at http://origodb.com/  Follow the quick start guide at http://origodb.com/docs/quick-start but swap the Todo.Core.dll with GeekStream.Core.dll. 
To get some data to play with use the admin utility (see below) or stop the server and copy the journal files from the App_Data/GeekStreamModel directory.

GeekStream.Admin
----------------------------
The admin project is a command line utility for collecting new items from the sources and adding/removing sources.
It uses the connection string in the app.config

