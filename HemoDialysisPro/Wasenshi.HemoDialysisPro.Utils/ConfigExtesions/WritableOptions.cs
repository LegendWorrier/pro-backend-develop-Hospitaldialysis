﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Wasenshi.HemoDialysisPro.Models.ConfigExtesions
{
    public class WritableOptions<T> : IWritableOptions<T> where T : class, new()
    {
        private readonly IHostEnvironment _environment;
        private readonly IOptionsMonitor<T> _options;
        private readonly IConfigurationRoot _configuration;
        private readonly string _section;
        private readonly string _file;

        public WritableOptions(
            IHostEnvironment environment,
            IOptionsMonitor<T> options,
            IConfigurationRoot configuration,
            string section,
            string file)
        {
            _environment = environment;
            _options = options;
            _configuration = configuration;
            _section = section;
            _file = file;
        }

        public T Value => _options.CurrentValue;
        public T Get(string name)
        {
            var target = new T();
            _configuration.Bind($"{_section}:{name}", target);
            return target;
        }

        public T GetOrDefault(string name)
        {
            var targetSection = _configuration.GetSection($"{_section}:{name}");
            if (targetSection.Exists())
            {
                var target = new T();
                targetSection.Bind(target);
                return target;
            }
            else
            {
                return _options.CurrentValue;
            }
        }

        public void Update(Action<T> applyChanges)
        {
            var fileProvider = _environment.ContentRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo(_file);
            var physicalPath = fileInfo.PhysicalPath;

            var jObject = JObject.Parse(File.ReadAllText(physicalPath));
            var sectionObject = jObject.TryGetValue(_section, out JToken section) ?
                section.ToObject<T>() : (Value ?? new T());

            applyChanges(sectionObject);

            jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
            File.WriteAllText(physicalPath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
            _configuration.Reload();
        }

        public void Update(string name, Action<T> applyChanges)
        {
            var fileProvider = _environment.ContentRootFileProvider;
            var fileInfo = fileProvider.GetFileInfo(_file);
            var physicalPath = fileInfo.PhysicalPath;

            var jObject = JObject.Parse(File.ReadAllText(physicalPath));

            var targetObject = jObject.TryGetValue($"{_section}.{name}", out JToken section) ?
                section.ToObject<T>() : (Value ?? new T());

            applyChanges(targetObject);

            jObject[_section][name] = JObject.Parse(JsonConvert.SerializeObject(targetObject));
            File.WriteAllText(physicalPath, JsonConvert.SerializeObject(jObject, Formatting.Indented));
            _configuration.Reload();
        }
    }
}
