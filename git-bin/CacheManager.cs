﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitBin
{
    public interface ICacheManager
    {
        byte[] ReadFileFromCache(string filename);
        void WriteFileToCache(string filename, byte[] contents, int contentLength);
        void WriteFileToCache(string filename, Stream stream);
        string[] GetFilenamesNotInCache(IEnumerable<string> filenamesToCheck);
        string[] ListFiles();
        string GetPathForFile(string filename);
    }

    public class CacheManager : ICacheManager
    {
        private readonly DirectoryInfo _cacheDirectoryInfo;

        public CacheManager(IConfigurationProvider configurationProvider)
        {
            _cacheDirectoryInfo = Directory.CreateDirectory(configurationProvider.CacheDirectory);
        }

        public byte[] ReadFileFromCache(string filename)
        {
            var path = GetPathForFile(filename);

            if (!File.Exists(path))
                throw new ಠ_ಠ("Tried to read file from cache that does not exist. [" + path + ']');

            return File.ReadAllBytes(path);
        }

        public void WriteFileToCache(string filename, byte[] contents, int contentLength)
        {
            var path = GetPathForFile(filename);

            if (!File.Exists(path))
            {
                var filestream = File.Create(path, contentLength, FileOptions.WriteThrough);
                filestream.Write(contents, 0, contentLength);
                filestream.Close();
            }
        }

        public void WriteFileToCache(string filename, Stream stream)
        {
            var path = GetPathForFile(filename);

            if (!File.Exists(path))
            {
                var buffer = new byte[8192];
                
                var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, buffer.Length, FileOptions.WriteThrough);

                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                }

                fileStream.Close();
                stream.Dispose();
            }
        }

        public string[] ListFiles()
        {
            var allFiles = _cacheDirectoryInfo.GetFiles();
            var filenamesInCache = allFiles.Select(fi => fi.Name);

            return filenamesInCache.ToArray();
        }

        public string[] GetFilenamesNotInCache(IEnumerable<string> filenamesToCheck)
        {
            var filenamesInCache = ListFiles();
            var filenamesNotInCache = filenamesToCheck.Except(filenamesInCache);

            return filenamesNotInCache.ToArray();
        }

        public string GetPathForFile(string filename)
        {
            return Path.Combine(_cacheDirectoryInfo.FullName, filename);
        }
    }
}