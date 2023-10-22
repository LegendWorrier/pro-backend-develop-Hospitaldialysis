using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.PluginBase;

namespace Wasenshi.HemoDialysisPro.PluginIntegrate
{
    public static class PluginUtils
    {
        /// <summary>
        /// Wrap plugin execution in try-catch to make sure it never interfere with main system.
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        /// <param name="plugins"></param>
        /// <param name="execute"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static async Task ExecutePlugins<TPlugin>(this IEnumerable<TPlugin> plugins, Func<TPlugin, Task> execute, Action<Exception> onError, bool catchPluginError = false)
            where TPlugin: class
        {
            try
            {
                foreach (var plugin in plugins)
                {
                    await execute(plugin);
                }
            }
            catch (Exception e)
            {
                if (!catchPluginError && e is PluginException pe)
                {
                    throw pe;
                }
                onError?.Invoke(e);
            }
        }

        /// <summary>
        /// Wrap plugin execution in try-catch to make sure it never interfere with main system.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <typeparam name="TPlugin"></typeparam>
        /// <param name="plugins"></param>
        /// <param name="execute"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static async Task<TReturn?> ExecutePlugins<TReturn, TPlugin>(this IEnumerable<TPlugin> plugins, Func<TPlugin, Task<TReturn>> execute, Action<Exception> onError, bool catchPluginError = false)
            where TPlugin : class
        {
            try
            {
                foreach (var plugin in plugins)
                {
                    var result = await execute(plugin);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                if (!catchPluginError && e is PluginException pe)
                {
                    throw pe;
                }
                onError?.Invoke(e);
            }

            return default;
        }

        /// <summary>
        /// Execute the plugin in separate background thread.
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        /// <param name="plugins"></param>
        /// <param name="execute"></param>
        public static void ExecutePluginsOnBackgroundThread<TPlugin>(this IEnumerable<TPlugin> plugins, Func<TPlugin, IServiceProvider, Task> execute)
            where TPlugin : class
        {
            foreach (var plugin in plugins)
            {
                PluginTaskScheduler.AddWorkToQueue(async sp =>
                {
                    await execute(plugin, sp.ServiceProvider);
                });
            }
        }

        /// <summary>
        /// Execute the plugin in separate background thread.
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        /// <param name="plugins"></param>
        /// <param name="execute"></param>
        public static void ExecutePluginsOnBackgroundThread<TPlugin>(this IEnumerable<TPlugin> plugins, Func<TPlugin, Task> execute)
            where TPlugin : class
        {
            foreach (var plugin in plugins)
            {
                PluginTaskScheduler.AddWorkToQueue(async _ =>
                {
                    await execute(plugin);
                });
            }
        }
    }
}
