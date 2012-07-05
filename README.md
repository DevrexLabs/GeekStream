GeekStream
==========
Geekstream is an example application built to demonstrate LiveDB Server - http://livedb.devrex.se/
A live example can be found at http://geekstream.devrex.se

GeekStream is a google-ish search engine for for syndication feeds and items. Besides search, there are some predefined streams in the footer:
* Popular feeds
* Recent items
* Popular items



Running the application
-----------------------------

1. Build the solution. A post build event copies the core domain assemly to the db folder. 
2. Start LiveDB Server using the StartServer.bat shortcut in the db folder.
3. Start the GeekStream web app from within Visual Studio.

What to check out
------------
Start by having a look at the GeekStreamModel class in the GeekStream.Core assembly. The in-memory database 
is an instance of this class hosted by the server. If you're into DDD think of the model as an aggregate root and the entire database is a single aggregate.

The model is modified by commands and queried using Query objects. You'll find these in the Commands and Queries folders in the GeekStream.Core project. 
Also, check out the classes in the Views folder. Views are used as return types from queries and commands. 

The LiveDB connection is intialized in Global.asax.cs and exposed as a static property. It's thread safe so 
Mvc action methods grab a connection and use it to execute queries and commands.

You can see the queries and commands in the servers console window.