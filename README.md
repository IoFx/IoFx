# IoFx
IoFx is a .NET library that enables reactive IO. The main motivation for this library is **preformance**. The target audience are folks who want to build high throughput application. The API surface enables composition using different protocols and frameworks like Sockets/WCF/HttpListener. 

##Overview
IoFx is built on top of Reactive primitives. The composition model dictates the cost of processing a message. Programing models like WCF have very object models and so buiding a simple message receiver is really hard and requires implementing channel layers and dispatchers. The basic idea is to build message handlers against Reactive primitives like IObservable<T> and IObserver<T> and subscribe and project messages and messaging patterns. 

##Performance
Given that the main goal of IoFx is performance, Connect.exe is the performance benchmark tool. As I move along, I hope to add more and more scenarios that could be used to benchmark various protocols and message exchange patterns. 

```
Connection Limit Test
Test for connection limit

Connect [/?]  [/mode]  [/server]  [/port]  [/climit]  [/rate]  [/type]

[/?]       Show Help
[/climit]  Number of connection.
[/mode]    Specified either client mode or server mode.
[/port]    Server port to listen or connect to.
[/rate]    Rate of outbound messages.
[/server]  Server to connect to
[/type]    connection type.

```

Using connect for Raw to benchmark raw socket throughput.

### Raw Socket 
The main scenario is to obtain measurement for metrics like connection density and memory usage when there are large number of inbound connections. 

* Starting a Socket Server 
```
Connect.exe /mode:server /port:8080 /type:socket
```

* Starting the client 
```
Connect.exe /mode:client  /port:8080 /type:socket /server:localhost /climit:1000 /rate:10000
```

