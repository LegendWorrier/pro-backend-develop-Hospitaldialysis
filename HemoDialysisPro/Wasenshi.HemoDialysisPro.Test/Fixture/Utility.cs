using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using Wasenshi.AuthPolicy.Utillities;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Test.Fixture
{
    public static class Utility
    {
        // prepare token
        public static void WithPowerAdminToken(this HttpClient client, object id = null, dynamic claim = null)
        {
            claim = ToDynamicExpando(claim);
            var dictionary = (IDictionary<string, object>)claim;
            dictionary.Add(ClaimsPermissionHelper.PERMISSION_TYPE, Permissions.GLOBAL);
            client.SetMockBearerToken("poweradmin", Roles.AllRoles, id, (object)dictionary);
        }

        public static void WithAdminToken(this HttpClient client, object id = null, dynamic claim = null)
        {
            client.SetMockBearerToken("myadmin", new[] { Roles.Admin }, id, (object)claim);
        }

        public static void WithDoctorToken(this HttpClient client, object id = null, dynamic claim = null)
        {
            client.SetMockBearerToken("mydoctor", new[] { Roles.Doctor }, id, (object)claim);
        }

        public static void WithHeadNurseToken(this HttpClient client, object id = null, dynamic claim = null)
        {
            client.SetMockBearerToken("myheadnurse", new[] { Roles.HeadNurse }, id, (object)claim);
        }

        public static void WithBasicUserToken(this HttpClient client, dynamic    claim = null)
        {
            client.SetMockBearerToken("mynurse", new[] { Roles.Nurse }, null, (object)claim);
        }

        public static void WithPNUserToken(this HttpClient client, dynamic claim = null)
        {
            client.SetMockBearerToken("myPN", new[] { Roles.PN }, null, (object)claim);
        }

        public static void WithToken(this HttpClient client, object userId, params string[] roles)
        {
            client.WithToken(userId, roles, null);
        }

        public static void WithToken(this HttpClient client, object userId, string[] role = null, dynamic claim = null)
        {
            client.SetMockBearerToken("myuser", role ?? new[] { Roles.Nurse }, userId, (object)claim);
        }

        private static IDictionary<string, object> ToDynamicExpando(dynamic obj)
        {
            if (obj != null)
            {
                var expando = new ExpandoObject();
                var dictionary = (IDictionary<string, object>)expando;
                if (obj is IDictionary<string, object> source)
                {
                    foreach (var item in source)
                        dictionary.Add(item.Key, item.Value);
                }
                else
                {
                    foreach (var property in obj.GetType().GetProperties())
                        dictionary.Add(property.Name, property.GetValue(obj));
                }
                return dictionary;
            }
            else
            {
                return new ExpandoObject();
            }
        }


        // assert helper

        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime; // do not modify "guard" values
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        public static DateTimeOffset Truncate(this DateTimeOffset dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            if (dateTime == DateTimeOffset.MinValue || dateTime == DateTimeOffset.MaxValue) return dateTime; // do not modify "guard" values
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        public static DateTimeOffset TruncateMilli(this DateTimeOffset datetime)
        {
            return datetime.Truncate(TimeSpan.FromSeconds(1));
        }
    }
}
