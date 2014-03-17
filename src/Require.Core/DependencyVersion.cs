using System;
using System.Globalization;

namespace Require
{
    /// <summary>
    /// 依赖版本号
    /// </summary>
    public sealed class DependencyVersion : ICloneable, IComparable, IComparable<DependencyVersion>, IEquatable<DependencyVersion>
    {
        public static DependencyVersion Parse(string version)
        {
            DependencyVersion result = new DependencyVersion();

            var items = version.Split('.');

            if (items.Length > 0)
            {
                result.Major = GetValue(items[0]);
            }

            if (items.Length > 1)
            {
                result.Minor = GetValue(items[1]);
                if (result.Minor != null && result.Major == null)
                {
                    throw new ArgumentException(string.Format(@"依赖版本号 ""{0}"" 格式错误, 当前Minor有值，所以Major不能设置为 ""*"" ", version));
                }
            }

            if (items.Length > 2)
            {
                result.Build = GetValue(items[2]);

                if (result.Build != null && result.Minor == null)
                {
                    throw new ArgumentException(string.Format(@"依赖版本号 ""{0}"" 格式错误, 当前Build有值，所以Major和Minor不能设置为 ""*"" ", version));
                }
            }

            if (items.Length > 3)
            {
                result.Revision = GetValue(items[3]);
                if (result.Revision != null && result.Build == null)
                {
                    throw new ArgumentException(string.Format(@"依赖版本号 ""{0}"" 格式错误, 当前Revision有值，所以Major,Minor,Build不能设置为 ""*"" ", version));
                }
            }

            return result;
        }

        private static int? GetValue(string v)
        {
            return v == "*" ? (int?)null : int.Parse(v);
        }

        private static int CompareValue(int? v1, int? v2)
        {
            if (v1 == null && v2 == null)
            {
                return 0;
            }
            else if (v1 != null && v2 != null)
            {
                return ((Int32)v1).CompareTo(v2.Value);
            }
            else if (v1 == null)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        #region < 构造函数 >
        public DependencyVersion()
        { }

        public DependencyVersion(int? major, int? minor, int? build, int? revision)
        {
            this.Major = major;
            this.Minor = minor;
            this.Build = build;
            this.Revision = revision;
        }
        #endregion

        #region < 属性 >
        public int? Major { get; set; }

        public int? Minor { get; set; }

        public int? Build { get; set; }

        public int? Revision { get; set; }

        
        #endregion


        #region < 接口实现 > 
        public object Clone()
        {
            return new DependencyVersion(this.Major, this.Minor, this.Build, this.Revision);
        }

        public int CompareTo(DependencyVersion version)
        {
            if (version == null) throw new ArgumentNullException("version");

            var result = CompareValue(this.Major, version.Major);
            if (result != 0) return result;

            result = CompareValue(this.Minor, version.Minor);
            if (result != 0) return result;

            result = CompareValue(this.Build, version.Build);
            if (result != 0) return result;

            return CompareValue(this.Revision, version.Revision);
        }

        public int CompareTo(object version)
        {
            var version2 = version as DependencyVersion;
            if (version2 == null) throw new ArgumentException("必须是 {0} 类型", typeof(DependencyVersion).Name);
            return CompareTo(version2);
        }

        public bool Equals(DependencyVersion obj)
        {
            if (obj == null) return false;
            return (((this.Major == obj.Major) && (this.Minor == obj.Minor)) && ((this.Build == obj.Build) && (this.Revision == obj.Revision)));
        }
        #endregion

        #region < override >
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,"{0}.{1}.{2}.{3}",
                this.Major == null ? "*" : this.Major.Value.ToString(),
                this.Minor == null ? "*" : this.Minor.Value.ToString(),
                this.Build == null ? "*" : this.Build.Value.ToString(),
                this.Revision == null ? "*" : this.Revision.Value.ToString()
            );
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DependencyVersion);
        }

        public override int GetHashCode()
        {
            int num = 0;
            num |= (this.Major==null ? 0 : ((this.Major.Value & 15) << 0x1c));
            num |= (this.Minor==null ? 0 : ((this.Minor.Value & 0xff) << 20));
            num |= (this.Build==null ? 0 : ((this.Build.Value & 0xff) << 12));
            return (num | (this.Revision==null ? 0 : (this.Revision.Value & 0xfff)));

        }
        #endregion

        #region < 运算符重载 >
        public static bool operator ==(DependencyVersion v1, DependencyVersion v2)
        {
            if (object.ReferenceEquals(v1, null))
            {
                return object.ReferenceEquals(v2, null);
            }
            return v1.Equals(v2);
        }
        public static bool operator >(DependencyVersion v1, DependencyVersion v2)
        {
            return (v2 < v1);
        }

        public static bool operator >=(DependencyVersion v1, DependencyVersion v2)
        {
            return (v2 <= v1);
        }

        public static bool operator !=(DependencyVersion v1, DependencyVersion v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(DependencyVersion v1, DependencyVersion v2)
        {
            if (v1 == null) throw new ArgumentNullException("v1");
            return (v1.CompareTo(v2) < 0);
        }

        public static bool operator <=(DependencyVersion v1, DependencyVersion v2)
        {
            if (v1 == null) throw new ArgumentNullException("v1");
            return (v1.CompareTo(v2) <= 0);
        }
        #endregion
    }
}
