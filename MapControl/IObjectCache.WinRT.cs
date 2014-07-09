// XAML Map Control - http://xamlmapcontrol.codeplex.com/
// Copyright © 2014 Clemens Fischer
// Licensed under the Microsoft Public License (Ms-PL)

using System.Threading.Tasks;

namespace MapControl
{
    public interface IObjectCache
    {
        Task<object> GetAsync(string key);
        Task SetAsync(string key, object value);
    }
}
