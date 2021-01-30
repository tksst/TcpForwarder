# TcpForwarder

This is a simple software that forward TCP connections.

これは、シンプルにTCP接続を転送するソフトウェアです。

## Background

For some reason I need a TCP forwarder like SSH forwarding or [stone](http://www.gcd.org/sengoku/stone/) running on PowerShell, So I wrote this code.

とある理由で、PowerShell上で動く、SSH転送や[stone](http://www.gcd.org/sengoku/stone/)のようなTCP転送機能が必要となったので、このコードを書きました。

## Requirement

You need .NET Framework 4.8 to run this.
It could also work on .NET 5 and above.

.NET Framework 4.8が必要です。おそらく .NET 5 以上でも動くと思います。

## Usage

```PowerShell
Add-Type -Path .\Path\To\TcpForwarder.dll
[TcpForwarder.TcpForwarderMain]::Main("192.0.2.0", 80, 8080)
```

## License

[Apache License 2.0](https://github.com/tksst/redirector/blob/main/LICENSE)
