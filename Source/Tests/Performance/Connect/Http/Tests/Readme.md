## Test ##

wcat is used to run these tests using the settings.ubr and the client.ubr files.

- Start the Controller

	```
	start wcctl.exe -t client.ubr -f settings.ubr -s %COMPUTERNAME% -v 30 -c 4 -o output.xml -x 
	```

- Start Clients

	Run this on each client machine 
	```
	wcclient.exe <controllerName>
	```


## References ##

- [WCAT Download (64-bit)](http://www.iis.net/community/default.aspx?tabid=34&g=6&i=1467)
- HTTP Server Version 2.0 - http://msdn.microsoft.com/en-us/library/windows/desktop/aa364705(v=vs.85).aspx