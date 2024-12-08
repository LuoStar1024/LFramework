using System;
using System.Collections.Generic;

namespace LFramework
{
    /// <summary>
    /// 游戏配置管理器。
    /// </summary>
    internal sealed class SettingManager : LFrameworkModule, ISettingManager
    {
        private ISettingHelper _settingHelper;

        /// <summary>
        /// 初始化游戏配置管理器的新实例。
        /// </summary>
        public SettingManager()
        {
            _settingHelper = null;
        }

        /// <summary>
        /// 获取游戏配置项数量。
        /// </summary>
        public int Count
        {
            get
            {
                if (_settingHelper == null)
                {
                    throw new LFrameworkException("Setting helper is invalid.");
                }

                return _settingHelper.Count;
            }
        }

        /// <summary>
        /// 游戏配置管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理游戏配置管理器。
        /// </summary>
        internal override void Shutdown()
        {
            Save();
        }

        /// <summary>
        /// 设置游戏配置辅助器。
        /// </summary>
        /// <param name="settingHelper">游戏配置辅助器。</param>
        public void SetSettingHelper(ISettingHelper settingHelper)
        {
            if (settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            _settingHelper = settingHelper;
        }

        /// <summary>
        /// 加载游戏配置。
        /// </summary>
        /// <returns>是否加载游戏配置成功。</returns>
        public bool Load()
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            return _settingHelper.Load();
        }

        /// <summary>
        /// 保存游戏配置。
        /// </summary>
        /// <returns>是否保存游戏配置成功。</returns>
        public bool Save()
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            return _settingHelper.Save();
        }

        /// <summary>
        /// 获取所有游戏配置项的名称。
        /// </summary>
        /// <returns>所有游戏配置项的名称。</returns>
        public string[] GetAllSettingNames()
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            return _settingHelper.GetAllSettingNames();
        }

        /// <summary>
        /// 获取所有游戏配置项的名称。
        /// </summary>
        /// <param name="results">所有游戏配置项的名称。</param>
        public void GetAllSettingNames(List<string> results)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            _settingHelper.GetAllSettingNames(results);
        }

        /// <summary>
        /// 检查是否存在指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要检查游戏配置项的名称。</param>
        /// <returns>指定的游戏配置项是否存在。</returns>
        public bool HasSetting(string settingName)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.HasSetting(settingName);
        }

        /// <summary>
        /// 移除指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要移除游戏配置项的名称。</param>
        /// <returns>是否移除指定游戏配置项成功。</returns>
        public bool RemoveSetting(string settingName)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.RemoveSetting(settingName);
        }

        /// <summary>
        /// 清空所有游戏配置项。
        /// </summary>
        public void RemoveAllSettings()
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            _settingHelper.RemoveAllSettings();
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string settingName)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetBool(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string settingName, bool defaultValue)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetBool(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入布尔值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的布尔值。</param>
        public void SetBool(string settingName, bool value)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            _settingHelper.SetBool(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string settingName)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetInt(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string settingName, int defaultValue)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetInt(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入整数值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的整数值。</param>
        public void SetInt(string settingName, int value)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            _settingHelper.SetInt(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string settingName)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetFloat(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string settingName, float defaultValue)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetFloat(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入浮点数值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的浮点数值。</param>
        public void SetFloat(string settingName, float value)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            _settingHelper.SetFloat(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string settingName)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetString(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string settingName, string defaultValue)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetString(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入字符串值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的字符串值。</param>
        public void SetString(string settingName, string value)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            _settingHelper.SetString(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        /// <typeparam name="T">要读取对象的类型。</typeparam>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的对象。</returns>
        public T GetObject<T>(string settingName)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetObject<T>(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        /// <param name="objectType">要读取对象的类型。</param>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的对象。</returns>
        public object GetObject(Type objectType, string settingName)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (objectType == null)
            {
                throw new LFrameworkException("Object type is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetObject(objectType, settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        /// <typeparam name="T">要读取对象的类型。</typeparam>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultObj">当指定的游戏配置项不存在时，返回此默认对象。</param>
        /// <returns>读取的对象。</returns>
        public T GetObject<T>(string settingName, T defaultObj)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetObject(settingName, defaultObj);
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        /// <param name="objectType">要读取对象的类型。</param>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultObj">当指定的游戏配置项不存在时，返回此默认对象。</param>
        /// <returns>读取的对象。</returns>
        public object GetObject(Type objectType, string settingName, object defaultObj)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (objectType == null)
            {
                throw new LFrameworkException("Object type is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            return _settingHelper.GetObject(objectType, settingName, defaultObj);
        }

        /// <summary>
        /// 向指定游戏配置项写入对象。
        /// </summary>
        /// <typeparam name="T">要写入对象的类型。</typeparam>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="obj">要写入的对象。</param>
        public void SetObject<T>(string settingName, T obj)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            _settingHelper.SetObject(settingName, obj);
        }

        /// <summary>
        /// 向指定游戏配置项写入对象。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="obj">要写入的对象。</param>
        public void SetObject(string settingName, object obj)
        {
            if (_settingHelper == null)
            {
                throw new LFrameworkException("Setting helper is invalid.");
            }

            if (string.IsNullOrEmpty(settingName))
            {
                throw new LFrameworkException("Setting name is invalid.");
            }

            _settingHelper.SetObject(settingName, obj);
        }
    }
}
