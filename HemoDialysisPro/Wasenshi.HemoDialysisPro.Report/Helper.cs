using MessagePack;
using SkiaSharp;
using System;
using System.IO;
using Telerik.Reporting.Expressions;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Report
{
    public static class Helper
    {
        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static string Capitalize(string text)
        {
            return text.Capitalize();
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static DateTime? ToDate(DateOnly? date)
        {
            if (!date.HasValue)
            {
                return null;
            }
            return date.Value.ToDateTime(new TimeOnly(0, 0));
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static DateTimeOffset? ToLocal(DateTime? datetime, string code = "TH")
        {
            if (!datetime.HasValue)
            {
                return null;
            }
            var tz = TimezoneUtils.GetTimeZone(code);
            return TimeZoneInfo.ConvertTime(new DateTimeOffset(datetime.Value, TimeSpan.Zero), tz);
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static Guid EmptyGuid()
        {
            return Guid.Empty;
        }

        // Drawing
        const float mmPerChar = 2f;

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static Telerik.Reporting.Drawing.Unit AutoWidth(Telerik.Reporting.TextBox textBox)
        {
            return AutoWidth(textBox, null);
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static Telerik.Reporting.Drawing.Unit AutoWidth(Telerik.Reporting.TextBox textBox, string maxWidth)
        {
            var limitW = string.IsNullOrWhiteSpace(maxWidth) ? textBox.Width : Telerik.Reporting.Drawing.Unit.Parse(maxWidth);

            return Telerik.Reporting.Drawing.Unit.Min(Telerik.Reporting.Drawing.Unit.Mm(mmPerChar * textBox.Value.Length), limitW);
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static Telerik.Reporting.Drawing.PointU AutoPosition(Telerik.Reporting.TextBox textBox, string content, string offset)
        {
            return AutoPosition(textBox, content, offset, null);
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static Telerik.Reporting.Drawing.PointU AutoPosition(Telerik.Reporting.TextBox textBox, string content, string offset, string maxPos)
        {
            var limitX = string.IsNullOrWhiteSpace(maxPos) ? textBox.Location.X : Telerik.Reporting.Drawing.Unit.Parse(maxPos);
            var offsetX = string.IsNullOrWhiteSpace(offset) ? Telerik.Reporting.Drawing.Unit.Zero : Telerik.Reporting.Drawing.Unit.Parse(offset);

            var posX = Telerik.Reporting.Drawing.Unit.Min(Telerik.Reporting.Drawing.Unit.Mm(mmPerChar * content?.Length ?? 0) + offsetX, limitX);

            return new Telerik.Reporting.Drawing.PointU(posX, textBox.Location.Y);
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static string ImageUri(string imageFileName)
        {
            var reportBasePath = CoreReportResolver.BasePath;
            return Path.Combine(reportBasePath, imageFileName);
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static byte[] Image(string imageFileName)
        {
            return File.ReadAllBytes(ImageUri(imageFileName));
        }

        /// <summary>
        /// Convert an object to a byte array.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ObjectToByteArray(this object obj)
        {
            //var resolver = CompositeResolver.Create(
            //    new[] { MessagePack.Formatters.TypelessFormatter.Instance },
            //    new[] { (IFormatterResolver)DynamicContractlessObjectResolver.Instance, ContractlessStandardResolver.Instance, StandardResolver.Instance });
            //var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
            var bytes = MessagePackSerializer.Typeless.Serialize(obj);
            //var data = new MemoryStream();
            //var formatter = new BinaryFormatter();
            //formatter.Serialize(data, obj);

            //var bytes = data.ToArray();
            return bytes;
        }

        /// <summary>
        /// Deserialize the binary data into an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T ByteArrayToObject<T>(this byte[] bytes)
        {
            return (T)ByteArrayToObject(bytes);
        }

        /// <summary>
        /// Deserialize the binary data into an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static object ByteArrayToObject(this byte[] bytes)
        {
            //var resolver = CompositeResolver.Create(
            //    new[] { MessagePack.Formatters.TypelessFormatter.Instance },
            //    new[] { (IFormatterResolver)DynamicContractlessObjectResolver.Instance, ContractlessStandardResolver.Instance, StandardResolver.Instance });
            //var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
            return MessagePackSerializer.Typeless.Deserialize(bytes);

            //var formatter = new BinaryFormatter();
            //var data = new MemoryStream(bytes);
            //var result = formatter.Deserialize(data);
            //return result;
        }
    }
}
