/*
    Copyright (c) 2014, CTL Global, Inc.
    Copyright (c) 2013, iD Commerce + Logistics
    All rights reserved.

    Redistribution and use in source and binary forms, with or without modification, are permitted
    provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions
    and the following disclaimer. Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the documentation and/or other
    materials provided with the distribution.
 
    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
    IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
    FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
    CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
    CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
    THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
    OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
    POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    static class DbDynamicMethods<T>
    {
        static Action<DbCommand, T> addParameters;

        public static Action<DbCommand, T> AddParameters
        {
            get
            {
                if (addParameters == null)
                {
                    addParameters = (Action<DbCommand, T>)DbDynamicMethods.CreateAddParameters(typeof(T));
                }

                return addParameters;
            }
        }
    }

    static class DbDynamicMethods
    {
        public static object CreateAddParameters(Type valueType)
        {
            ParameterExpression cmdParam = Expression.Parameter(typeof(DbCommand), "cmd");
            ParameterExpression valueParam = Expression.Parameter(valueType, "value");

            Expression cmdParamsVar = Expression.Variable(typeof(DbParameterCollection), "cmdParams");

            Expression paramVar = Expression.Variable(typeof(DbParameter), "param");
            Expression paramName = Expression.Property(paramVar, "ParameterName");
            Expression paramValue = Expression.Property(paramVar, "Value");

            Expression createParam = Expression.Assign(paramVar, Expression.Call(cmdParam, typeof(DbCommand).GetMethod("CreateParameter", Type.EmptyTypes)));
            Expression addParam = Expression.Call(cmdParamsVar, typeof(DbParameterCollection).GetMethod("Add", new[] { typeof(object) }), paramVar);

            List<Expression> expressions = new List<Expression>();

            expressions.Add(Expression.Assign(cmdParamsVar, Expression.Property(cmdParam, "Parameters")));

            foreach (var member in valueType.GetMembers())
            {
                Expression value = null;

                PropertyInfo prop = member as PropertyInfo;
                if (prop != null && prop.GetGetMethod() != null)
                {
                    value = Expression.Property(valueParam, prop);
                }

                FieldInfo field = member as FieldInfo;
                if (field != null)
                {
                    value = Expression.Field(valueParam, field);
                }

                if (value == null)
                {
                    continue;
                }

                expressions.Add(createParam);
                expressions.Add(Expression.Assign(paramName, Expression.Constant(member.Name)));
                expressions.Add(Expression.Assign(paramVar, value));
                expressions.Add(addParam);
            }

            BlockExpression block = Expression.Block(expressions);
            LambdaExpression lambda = Expression.Lambda(typeof(Action<,>).MakeGenericType(typeof(DbCommand), valueType), block, cmdParam, valueParam);

            return lambda.Compile();
        }
    }
}
