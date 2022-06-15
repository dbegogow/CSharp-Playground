﻿using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace WebAppExpressions.Infrastructure
{
    public static class ControllerExtensions
    {
        public static IActionResult RedirectTo<TController>(
            this Controller controller,
            Expression<Action<TController>> redirectExpression)
        {
            if (redirectExpression.Body.NodeType != ExpressionType.Call)
            {
                throw new InvalidOperationException($"The provided expression is not a valid method call: {redirectExpression.Body}");
            }

            var methodCallExpression = (MethodCallExpression)redirectExpression.Body;

            var actionName = methodCallExpression.Method.Name;
            var controllerName = typeof(TController).Name.Replace(nameof(Controller), string.Empty);

            var routeValues = ExtractRouteValues(methodCallExpression);

            return controller.RedirectToAction(actionName, controllerName, routeValues);
        }

        private static RouteValueDictionary ExtractRouteValues(MethodCallExpression expression)
        {
            var names = expression.Method
                .GetParameters()
                .Select(p => p.Name)
                .ToArray();

            var values = expression.Arguments
                .Select(arg =>
                {
                    if (arg.NodeType == ExpressionType.Constant)
                    {
                        var constantExpression = (ConstantExpression)arg;
                        return constantExpression.Value;
                    }

                    return null;
                })
                .ToArray();

            var routeValueDictionary = new RouteValueDictionary();

            for (int i = 0; i < names.Length; i++)
            {
                routeValueDictionary.Add(names[i], values[i]);
            }

            return routeValueDictionary;
        }
    }
}
