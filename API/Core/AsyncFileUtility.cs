using System.Collections.Generic;

namespace GRPP.API.Core;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LabApi.Features.Console;

public class AsyncFileUtility/*Stream inner, IProgress<long>? progress = null*/
{
    private const int BufferSize = 16384;
    // private readonly Stream _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    // private readonly IProgress<long>? _progress = progress;
    // private long _processed;
    public enum CopyResult : byte
    {
        Success = 0,
        NullArgument = 1,
        InvalidFileName = 2,
        AccessDenied = 3,
        IoError = 4,
        Cancelled = 5,
        UnknownFailure = 255
    }

    
    public async Task<CopyResult> CopyFileAsync(string? source, string? destination, int? bufferSizeToUse, CancellationToken token = default, int position1 = 0, int position2 = 0) // 0=success, 1=failure unknown // 2 = nullref
    {
        if (source == null || destination == null)
        {
            Logger.Warn("In an attempt to copy files, source and/or destination were null. Saving the universe, by canceling.");
            return CopyResult.NullArgument;
        }

        if (!File.Exists(source))
        {
            Logger.Error($"Source file does not exist: {source}");
            return CopyResult.IoError;
        }

        var fileName = Path.GetFileName(source);
        if (string.IsNullOrEmpty(fileName))
        {
            Logger.Error($"Error: source file is empty. Could not determine filename. File: {fileName} Src: {source}");
            return CopyResult.InvalidFileName;
        }
        var fullDestination = Path.Combine(destination, fileName);

        try
        {
            using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSizeToUse ?? BufferSize, useAsync: true); // 16kb
            sourceStream.Position = position1;
            using var destinationStream = new FileStream(fullDestination, FileMode.Create, FileAccess.Write,
                FileShare.None, bufferSizeToUse ?? BufferSize, useAsync: true); // 16kb
            destinationStream.Position = position2;

            await sourceStream.CopyToAsync(destinationStream, bufferSizeToUse ?? BufferSize, token)
                .ConfigureAwait(false);
            return CopyResult.Success;
        }
        // catch (AccessViolationException e) // lol used the wrong one there
        catch (UnauthorizedAccessException e)
        {
            Logger.Error(e.Message +
                         $"UnauthorizedAccessException when attempting to copy {fileName} to {destination}. Please ensure you have permission to write to this folder.");
            return CopyResult.AccessDenied;
        }
        catch (IOException e)
        {
            Logger.Error($"IOException copying '{fileName}': {e}");
            return CopyResult.IoError;
        }
        catch (OperationCanceledException)
        {
            Logger.Info($"Copy of '{fileName}' was cancelled.");
            throw;
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            return CopyResult.UnknownFailure;
        }
    }

    public async Task CreateDirectoryAsync(string? dir, CancellationToken cancellationToken = default)
    {
        if (dir == null || Plugin.Singleton == null) throw new NullReferenceException(nameof(dir) + "");
        cancellationToken.ThrowIfCancellationRequested();
        // if (Directory.Exists(dir))
        // {
        //     cancellationToken.ThrowIfCancellationRequested();
        //     Logger.Warn($"Directory {dir} already exists. Skipping directory creation async..");
        //     return;
        // }
        if (!Plugin.Singleton.Config.CreateDirectoryAsync ?? false)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Logger.Info($"Creating directory {dir} without Task.Run.");
            if (Directory.Exists(dir))
            {
                Logger.Warn("Directory already exists.");
                return;
            }
            Directory.CreateDirectory(dir);
            return;
        }
        if (Plugin.Singleton.Config.CreateDirectoryAsync == null) Logger.Warn("CreateDirectoryAsync has been identified as null. Continuing with async.");
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Run(() => Directory.CreateDirectory(dir), cancellationToken);
    }

    public async Task<string?> ReadFileAsStringAsync(string? filePath, int? bufferSizeToUse, CancellationToken cancellationToken = default, int pos1 = 0, int pos2 = 0)
    {
        if (filePath == null)
        {
            Logger.Warn("In an attempt to read the file, the path was null. Saving the universe, by canceling.");
            return null;
        }
        if (!File.Exists(filePath))
        {
            Logger.Error($"File does not exist: {filePath}");
            return null;
        }
        var fileName = Path.GetFileName(filePath); // this is PURELY used for logging purposes. can be removed if performance ever becomes an issue.
        if (string.IsNullOrEmpty(fileName))
        {
            Logger.Error($"Error: file is empty. Could not determine filename. File: {fileName} Src: {filePath}");
            return null;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSizeToUse ?? BufferSize, useAsync: true);
            using var reader = new StreamReader(sourceStream, System.Text.Encoding.UTF8);
    
            cancellationToken.ThrowIfCancellationRequested();
            // This reads the entire remaining stream into one string directly
            return await reader.ReadToEndAsync();
        }
        // catch (AccessViolationException e) // lol used the wrong one there
        catch (UnauthorizedAccessException e)
        {
            Logger.Error(e.Message +
                         $"UnauthorizedAccessException when attempting to read {fileName}. Please ensure you have permission to read and write in this folder.");
            return null;
        }
        catch (IOException e)
        {
            Logger.Error($"IOException copying '{fileName}': {e}");
            return null;
        }
        catch (OperationCanceledException)
        {
            Logger.Warn($"Copy of '{fileName}' was cancelled.");
            throw;
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            return null;
        }
    }
    
    
    public async Task<string[]?> ReadFileAsArrayAsync(string? filePath, int? bufferSizeToUse, CancellationToken cancellationToken = default, int position1 = 0, int position2 = 0)
    {
        if (filePath == null)
        {
            Logger.Warn($"In an attempt to read {filePath} as an array, file was null. Saving the universe, by canceling.");
            return null;
        }

        if (!File.Exists(filePath))
        {
            Logger.Error($"Provided file does not exist: {filePath}");
            return null;
        }

        var fileName = Path.GetFileName(filePath);
        if (string.IsNullOrEmpty(fileName))
        {
            Logger.Error($"Error: file path is empty. Could not determine filename. File: {fileName} Src: {filePath}");
            return null;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSizeToUse ?? BufferSize, useAsync: true); // 16kb
            using var reader = new StreamReader(sourceStream, System.Text.Encoding.UTF8);
            cancellationToken.ThrowIfCancellationRequested();
            
            var linesList = new List<string>();
            while (await reader.ReadLineAsync() is { } line)
            {
                cancellationToken.ThrowIfCancellationRequested();
                linesList.Add(line);
            }
            string[] result = linesList.ToArray();
            return result;
        }
        // catch (AccessViolationException e) // lol used the wrong one there
        catch (UnauthorizedAccessException e)
        {
            Logger.Error(e.Message +
                         $"UnauthorizedAccessException when attempting to read {fileName}. Please ensure you have permission to read and write in this folder.");
            return null;
        }
        catch (IOException e)
        {
            Logger.Error($"IOException copying '{fileName}': {e}");
            return null;
        }
        catch (OperationCanceledException)
        {
            Logger.Warn($"Copy of '{fileName}' was cancelled.");
            throw;
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            return null;
        }
    }
    
    public async Task<bool?> WriteFileFromString(string file, string? filePath, int? bufferSizeToUse, CancellationToken cancellationToken = default, int position1 = 0, int position2 = 0)
    {
        if (filePath == null)
        {
            Logger.Warn($"In an attempt to read {filePath}, filePath was null. Saving the universe, by canceling.");
            return null;
        }

        if (File.Exists(filePath))
            Logger.Warn($"Rewriting file at {filePath}!");

        var fileName = Path.GetFileName(filePath);
        if (string.IsNullOrEmpty(fileName))
        {
            Logger.Error($"Error: file path is empty. Could not determine filename. File: {fileName} Src: {filePath}");
            return null;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var sourceStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write,
                bufferSizeToUse ?? BufferSize, useAsync: true); // 16kb
            using var write = new StreamWriter(sourceStream, System.Text.Encoding.UTF8);
            cancellationToken.ThrowIfCancellationRequested();
            await write.WriteAsync(file).ConfigureAwait(false); // .NET 4.8.1 - doesn't accept cancellationToken sadly :( -- we CAN code this in by going one level deeper though.
            return true;
        }
        // catch (AccessViolationException e) // lol used the wrong one there
        catch (UnauthorizedAccessException e)
        {
            Logger.Error(e.Message +
                         $"UnauthorizedAccessException when attempting to read {fileName}. Please ensure you have permission to read and write in this folder.");
            return null;
        }
        catch (IOException e)
        {
            Logger.Error($"IOException writing '{fileName}': {e}");
            return null;
        }
        catch (OperationCanceledException)
        {
            Logger.Warn($"Write of '{fileName}' was cancelled.");
            throw;
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            return null;
        }
    }
}