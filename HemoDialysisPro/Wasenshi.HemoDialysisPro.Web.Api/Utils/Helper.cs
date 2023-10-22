using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Web.Api.Controllers;
using Wasenshi.HemoDialysisPro.Utils;
using Microsoft.AspNetCore.Mvc;
using Wasenshi.HemoDialysisPro.Models.Settings;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils
{
    public static class Helper
    {
        private static readonly MethodInfo MethodContains = typeof(Enumerable).GetMethods(
                BindingFlags.Static | BindingFlags.Public)
            .Single(m => m.Name == nameof(Enumerable.Contains)
                         && m.GetParameters().Length == 2).MakeGenericMethod(typeof(int));

        private static readonly MethodInfo MethodAny = typeof(Enumerable).GetMethods(
                BindingFlags.Static | BindingFlags.Public)
            .Single(m => m.Name == nameof(Enumerable.Any)
                         && m.GetParameters().Length == 2).MakeGenericMethod(typeof(int));

        private static readonly MethodInfo MethodCount = typeof(Enumerable).GetMethods(
                BindingFlags.Static | BindingFlags.Public)
            .Single(m => m.Name == nameof(Enumerable.Count)
                         && m.GetParameters().Length == 1).MakeGenericMethod(typeof(int));

        public static Expression<Func<T, bool>> GetUnitFilter<T>(this ClaimsPrincipal user,
            Expression<Func<T, IEnumerable<int>>> unitsAccessor)
            where T : class
        {
            List<int> units = user.GetUnitList().ToList();

            ParameterExpression param = unitsAccessor.Parameters[0];
            ParameterExpression innerParam = Expression.Parameter(typeof(int));
            var contain = Expression.Call(MethodContains, Expression.Constant(units), innerParam);
            var anyCall = Expression.Call(MethodAny, unitsAccessor.Body,
                Expression.Lambda<Func<int, bool>>(contain, innerParam));
            var countCall = Expression.Call(MethodCount, unitsAccessor.Body);
            var checkEmptyList = Expression.Equal(countCall, Expression.Constant(0));

            Expression<Func<T, bool>> unitExpression =
                Expression.Lambda<Func<T, bool>>(Expression.OrElse(checkEmptyList, anyCall), param);

            Expression<Func<T, bool>> whereCondition = x => user.IsInRole(Roles.PowerAdmin);
            whereCondition = whereCondition.OrElse(unitExpression);

            return whereCondition;
        }

        public static Expression<Func<T, bool>> GetUnitFilter<T>(this ClaimsPrincipal user,
            Expression<Func<T, int>> unitAccessor)
            where T : class
        {
            List<int> units = user.GetUnitList().ToList();

            ParameterExpression param = unitAccessor.Parameters[0];
            var body = Expression.Call(MethodContains, Expression.Constant(units), unitAccessor.Body);
            Expression<Func<T, bool>> unitExpression = Expression.Lambda<Func<T, bool>>(body, param);

            Expression<Func<T, bool>> whereCondition = x => user.IsInRole(Roles.PowerAdmin);
            whereCondition = whereCondition.OrElse(unitExpression);

            return whereCondition;
        }

        public static bool IsDoctorAndSeeOwnPatientOnly(this ControllerBase controller, GlobalSetting setting)
        {
            return controller.IsDoctor() && (setting.Patient?.DoctorCanSeeOwnPatientOnly ?? false);
        }

        public static bool IsDoctor(this ControllerBase controller)
        {
            return !controller.User.IsInRole(Roles.PowerAdmin) && controller.User.IsInRole(Roles.Doctor);
        }

        public const string PERMISSION_CHANGE = "permission-change";

        public static void SetPermissionChangeSignal(this IRedisClient redis, Guid userId)
        {
            redis.Set($"{PERMISSION_CHANGE}:{userId}", true);
        }

        public static bool CheckPermissionChangeSignal(this IRedisClient redis, Guid userId)
        {
            return redis.ContainsKey($"{PERMISSION_CHANGE}:{userId}");
        }

        public static void ResetPermissionChangeSignal(this IRedisClient redis, Guid userId)
        {
            redis.Remove($"{PERMISSION_CHANGE}:{userId}");
        }
    }
}
