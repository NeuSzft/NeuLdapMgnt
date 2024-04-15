using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NeuLdapMgnt.Api;

public sealed class RequestLogger(StreamWriter writer) : ILogger {
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel) {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        string message = $"[{DateTime.Now:yyyy.MM.dd - HH:mm:ss}] {logLevel,-11} - {formatter(state, exception)}";
        writer.WriteLineAsync(message).Wait();
    }
}

public sealed class RequestLoggerProvider(string logsDir) : ILoggerProvider {
    private StreamWriter? _logFileWriter;

    public void Dispose() {
        _logFileWriter?.DisposeAsync().AsTask().Wait();
    }

    public ILogger CreateLogger(string categoryName) {
        Directory.CreateDirectory(logsDir);
        _logFileWriter = new(Path.Combine(logsDir, $"{DateTime.Now:yyyy.MM.dd-HH_mm_ss}.log"), Encoding.UTF8, new FileStreamOptions {
            Access  = FileAccess.Write,
            Mode    = FileMode.CreateNew,
            Options = FileOptions.Asynchronous
        }) {
            AutoFlush = true
        };
        return new RequestLogger(_logFileWriter);
    }
}
